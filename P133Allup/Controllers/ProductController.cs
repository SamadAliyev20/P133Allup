using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using P133Allup.DataAccessLayer;
using P133Allup.Models;
using P133Allup.ViewModels.BasketViewModels;

namespace P133Allup.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ProductModal(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Product product = await _context.Products.Include(x=>x.ProductImages).FirstOrDefaultAsync(x => x.IsDeleted == false && x.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return PartialView("_ModalPartial", product);
}

        public async Task<IActionResult> Search(int? categoryId,string search)
        {

            if (categoryId != null && categoryId > 0)
            {
                if (!await _context.Categories.AnyAsync(c => c.Id == categoryId))
                {
                    return BadRequest();
                }
             
            }
            IEnumerable<Product> products = await _context.Products
                 .Where(p => p.IsDeleted == false && categoryId != null && categoryId > 0 ? p.CategoryId == categoryId : true &&
                 (p.Title.ToLower().Contains(search.ToLower()) || p.Brand.Name.ToLower().Contains(search.ToLower()))).ToListAsync();

            return PartialView("_SearchPartial",products);
        }
        public IActionResult ChangeBasketProductCount(int? id, int count)
        {
            if (id == null)
            {
                return BadRequest();
            }
            if (!_context.Products.Any(p => p.Id == id))
            {
                return NotFound();
            }
            string basket = HttpContext.Request.Cookies["basket"];
            List<BasketVM> basketVMs = null;
            if (basket != null)
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);
                basketVMs.Find(p => p.Id == id).Count = count;
                basket = JsonConvert.SerializeObject(basketVMs);
                HttpContext.Response.Cookies.Append("basket", basket);
                foreach (BasketVM basketVM in basketVMs)
                {
                    basketVM.Title = _context.Products.FirstOrDefault(p => p.Id == basketVM.Id).Title;
                    basketVM.Image = _context.Products.FirstOrDefault(p => p.Id == basketVM.Id).MainImage;
                    basketVM.Price = _context.Products.FirstOrDefault(p => p.Id == basketVM.Id).Price;
                }
                return PartialView("_BasketProductTablePartial", basketVMs);
            }
            else
            {
                return BadRequest();
            }
        }

    }
}

