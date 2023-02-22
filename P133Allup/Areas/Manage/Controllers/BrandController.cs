using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P133Allup.DataAccessLayer;
using P133Allup.Models;
using P133Allup.ViewModels;
using System.Drawing.Drawing2D;

namespace P133Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    public class BrandController : Controller
    {
        private readonly AppDbContext _context;

        public BrandController(AppDbContext context)
        {
            _context = context;
        }

        public async Task <IActionResult> Index(int pageIndex=1)
        {
            IQueryable<Brand> brands = _context.Brands
                .Include(p => p.Products.Where(p => p.IsDeleted == false))
                .Where(p => p.IsDeleted == false).OrderByDescending(b => b.Id);
            return View(PageNatedList<Brand>.Create(brands,pageIndex,3));
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task <IActionResult> Create(Brand brand)
        {
            if (!ModelState.IsValid)
            {
                return View(brand);
            }
            if (await _context.Brands.AnyAsync(b=>b.IsDeleted==false && b.Name.ToLower().Contains(brand.Name.Trim().ToLower())))
            {
                ModelState.AddModelError("Name", $" {brand.Name} brend'i artiq movcudur.");

                return View(brand);
            }
            brand.Name= brand.Name.Trim();
            brand.CreatedBy = "System";
            brand.CreatedAt = DateTime.UtcNow.AddHours(4);

            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {

            if (id == null) return BadRequest();

            Brand brand = await _context.Brands.Include(b=>b.Products.Where(b=>b.IsDeleted==false))
                .FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (brand == null) return NotFound();


            return View(brand);
        }

        [HttpGet]
        
        public async Task <IActionResult> Update(int? id)
        {
            if (id==null) return BadRequest();

            Brand brand = await _context.Brands.FirstOrDefaultAsync(b=> b.IsDeleted==false && b.Id==id);

            if (brand==null) return NotFound();   

            return View(brand);
        }

        [HttpPost]

        public async Task<IActionResult> Update(int? id,Brand brand)
        {
            if (!ModelState.IsValid)
            {
                return View(brand);
            }
            if (id == null) return BadRequest();

            if(id != brand.Id) return BadRequest();

            Brand DBbrand = await _context.Brands.FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (brand == null) return NotFound();

            if (await _context.Brands.AnyAsync(b => b.IsDeleted == false && b.Name.ToLower().Contains(brand.Name.Trim().ToLower())&&brand.Id !=b.Id))
            {
                ModelState.AddModelError("Name", $" {brand.Name} brend'i artiq movcudur.");

                return View(brand);
            }
            DBbrand.Name = brand.Name.Trim();
            DBbrand.UpdatedBy = "System";
            DBbrand.UpdatedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));


        }

        [HttpGet]

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Brand brand = await _context.Brands.Include(b => b.Products.Where(b => b.IsDeleted == false))
                .FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (brand == null) return NotFound();


            return View(brand);
        }
        [HttpGet]
        public async Task<IActionResult> DeleteBrand(int? id)
        {
            if (id == null) return BadRequest();

            Brand brand = await _context.Brands.Include(b => b.Products.Where(b => b.IsDeleted == false))
                .FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (brand == null) return NotFound();

            brand.IsDeleted= true;
            brand.DeletedBy = "System";
            brand.DeletedAt = DateTime.UtcNow.AddHours(4);

            foreach (Product product in brand.Products)
            {
                product.IsDeleted = true;
                product.DeletedBy = "System";
                product.DeletedAt = DateTime.UtcNow.AddHours(4);
            }


            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }

    }
}
