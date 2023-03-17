using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P133Allup.DataAccessLayer;
using P133Allup.Extentions;
using P133Allup.Helpers;
using P133Allup.Models;
using P133Allup.ViewModels;

namespace P133Allup.Areas.Manage.Controllers
{
	[Area("Manage")]

	public class ProductController : Controller
	{
		private readonly AppDbContext _context;
		private readonly IWebHostEnvironment _env;

		public ProductController(AppDbContext context, IWebHostEnvironment env)
		{
			_context = context;
			_env = env;
		}

		[HttpGet]
		public IActionResult Index(int pageIndex = 1)
		{
			IQueryable<Product> products = _context.Products.Where(p => p.IsDeleted == false);
			return View(PageNatedList<Product>.Create(products,pageIndex,3));
		}
		[HttpGet]
		public async Task<IActionResult> Create()
		{
			ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
			ViewBag.Categories = await _context.Categories
				.Include(b => b.Children).Where(c => c.IsDeleted == false)
				.Where(c => c.IsDeleted == false && c.IsMain)
				.ToListAsync();

			ViewBag.Tags= await _context.Tags.Where(t=>t.IsDeleted== false).ToListAsync();
	
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Product product)
		{
			ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
			ViewBag.Categories = await _context.Categories
				.Include(b => b.Children).Where(c => c.IsDeleted == false)
				.Where(c => c.IsDeleted == false && c.IsMain)
				.ToListAsync();

			ViewBag.Tags = await _context.Tags.Where(t => t.IsDeleted == false).ToListAsync();

			if (!ModelState.IsValid) return View(product);

			if (!await _context.Brands.AnyAsync(b => b.IsDeleted == false && b.Id == product.BrandId))
			{
				ModelState.AddModelError("BrandId",$"Daxil Olunan Brand Id {product.BrandId} yanlisdir");

				return View(product);
			}
			if (!await _context.Categories.AnyAsync(b => b.IsDeleted == false && b.Id == product.CategoryId))
			{
				ModelState.AddModelError("BrandId", $"Daxil Olunan Category Id {product.CategoryId} yanlisdir");

				return View(product);
			}
			if (product.TagIds != null && product.TagIds.Count() > 0)
			{
				List<ProductTag> productTags = new List<ProductTag>();
				foreach (int tagId in product.TagIds)
				{
					if (!await _context.Tags.AnyAsync(b => b.IsDeleted == false && b.Id == tagId))
					{
						ModelState.AddModelError("BrandId", $"Daxil Olunan Tag Id {tagId} yanlisdir");

						return View(product);
					}
					ProductTag productTag = new ProductTag
					{
						TagId = tagId,
						CreatedAt = DateTime.UtcNow.AddHours(4),
						CreatedBy = "System"

					};

					productTags.Add(productTag);
				}

				product.ProductTags = productTags;
			}
			else
			{
				ModelState.AddModelError("TagIds", "Tag mutleq secilmelidir");
				return View(product);
			}

			if (product.MainFile != null)
			{
				if (!product.MainFile.CheckFileContentType("image/jpeg"))
				{
					ModelState.AddModelError("MainFile", "Main File ,Jpeg tipinde olmalidir!");
					return View(product);
				}
				if (!product.MainFile.CheckFileLength(1500))
				{
					ModelState.AddModelError("MainFile", "Main File ,Main File 1.5 MB dan cox ola bilmez!");
					return View(product);
				}
				product.MainImage = await product.MainFile.CreateFileAsync(_env, "assets","images","product");
			}
			else
			{
				ModelState.AddModelError("MainFile", "Main File ,Mutleq Secilmelidir!");
				return View(product);
			}
			if (product.HoverFile != null)
			{
				if (!product.HoverFile.CheckFileContentType("image/jpeg"))
				{
					ModelState.AddModelError("MainFile", "Hover File ,Jpeg tipinde olmalidir!");
					return View(product);
				}
				if (!product.HoverFile.CheckFileLength(300))
				{
					ModelState.AddModelError("HoverFile", "Hover File  ,Main File 300 KB dan cox ola bilmez!");
					return View(product);
				}
				product.HoverImage = await product.HoverFile.CreateFileAsync(_env, "assets", "images", "product");
			}
			else
			{
				ModelState.AddModelError("HoverFile", "Hover File ,Mutleq Secilmelidir!");
				return View(product);
			}

			if ( product.Files?.Count() > 6)
			{
				ModelState.AddModelError("Files", "Maks 6 şekil yükleye bilersiniz");
				return View(product);
			}

			if (product.Files == null)
			{
				ModelState.AddModelError("Files", " şekil Mutleq secilmelidir!");
				return View(product);
			}

			if ( product.Files.Count() > 0)
			{
				List<ProductImage> productImages = new List<ProductImage>();

				foreach (IFormFile file in product.Files)
				{
					if (!file.CheckFileContentType("image/jpeg"))
					{
						ModelState.AddModelError("Files", $"{file.FileName} ,Jpeg tipinde olmalidir!");
						return View(product);
					}
					if (!file.CheckFileLength(300))
					{
						ModelState.AddModelError("Files", $"{file.FileName}  ,Main File 300 KB dan cox ola bilmez!");
						return View(product);
					}
					ProductImage productImage = new ProductImage
					{
						Image = await file.CreateFileAsync(_env, "assets", "images", "product"),
						CreatedAt = DateTime.UtcNow.AddHours(4),
						CreatedBy = "System"
					};

					productImages.Add(productImage);

				}

				product.ProductImages = productImages;
			}

			string code = product.Title.Substring(0,2);
			code = code + _context.Brands.FirstOrDefault(c=>c.Id==product.BrandId).Name.Substring(0,1);
			code = code + _context.Categories.FirstOrDefault(c => c.Id == product.CategoryId).Name.Substring(0, 1);

			product.Seria = code.ToLower().Trim();
			product.Code = _context.Products.Where(p => p.Seria == product.Seria).OrderByDescending(p => p.Id).FirstOrDefault() != null ?
				_context.Products.Where(p => p.Seria == product.Seria).OrderByDescending(p => p.Id).FirstOrDefault().Code + 1 : 1;

			await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public async Task<IActionResult> Update(int? id)
		{
			if (id == null) return BadRequest();

			Product product = await _context.Products
				.Include(p => p.ProductImages.Where(p=>p.IsDeleted == false))
				.Include(p => p.ProductTags.Where(pt=>pt.IsDeleted==false))
				.FirstOrDefaultAsync(p=>p.IsDeleted == false && p.Id == id);

			if (product == null) return NotFound();
            ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
            ViewBag.Categories = await _context.Categories
                .Include(b => b.Children).Where(c => c.IsDeleted == false)
                .Where(c => c.IsDeleted == false && c.IsMain)
                .ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => t.IsDeleted == false).ToListAsync();

			product.TagIds = product.ProductTags != null && product.ProductTags.Count() > 0 ?
				product.ProductTags.Select(x => (byte)x.TagId).ToList():new List<byte>();

			return View(product);



        }

		[HttpGet]
		public async Task<IActionResult> DeleteImage(int? id,int? imageId)
		{
			if(id == null) return BadRequest();
			if (imageId == null) return BadRequest();
			Product product = await _context.Products
				.Include(p => p.ProductImages.Where(p => p.IsDeleted == false))
				.FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == id);

			if (product == null) return NotFound();
			if (product.ProductImages?.Count() <= 1) return BadRequest();
			
			if(!product.ProductImages.Any(p=>p.Id == imageId)) return BadRequest();
			product.ProductImages.FirstOrDefault(p => p.Id == imageId).IsDeleted = true;

			await _context.SaveChangesAsync();
			FileHelper.DeleteFile(product.ProductImages.FirstOrDefault(p => p.Id == imageId).Image, _env,"assets","images","product");
	         List<ProductImage> productImages = product.ProductImages.Where(p => p.IsDeleted == false).ToList();
			return PartialView("_ProductImagePartial",productImages);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Update(int? id,Product product)
		{
			ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
			ViewBag.Categories = await _context.Categories
				.Include(b => b.Children).Where(c => c.IsDeleted == false)
				.Where(c => c.IsDeleted == false && c.IsMain)
				.ToListAsync();
			ViewBag.Tags = await _context.Tags.Where(t => t.IsDeleted == false).ToListAsync();
			if (!ModelState.IsValid)		
			{
				return View(product);
			}
			if (id == null) return BadRequest();
			if (id != product.Id) return BadRequest();
			Product dbproduct = await _context.Products
			.Include(pt=>pt.ProductImages.Where(p=>p.IsDeleted == false))
			.Include(p => p.ProductTags.Where(pt => pt.IsDeleted == false))
			.FirstOrDefaultAsync(p=>p.IsDeleted == false && p.Id == id); 
			if(dbproduct == null) return NotFound();
			int canupload = 6 - dbproduct.ProductImages.Count();
			if (product.Files !=null && canupload < product.Files.Count())
			{
				ModelState.AddModelError("Files", $"Maks {canupload} qeder şekil yükleye bilersiniz");
				return View(product);
			}
			if (product.Files != null && product.Files.Count() > 0)
			{
				List<ProductImage> productImages = new List<ProductImage>();

				foreach (IFormFile file in product.Files)
				{
					if (!file.CheckFileContentType("image/jpeg"))
					{
						ModelState.AddModelError("Files", $"{file.FileName} ,Jpeg tipinde olmalidir!");
						return View(product);
					}
					if (!file.CheckFileLength(300))
					{
						ModelState.AddModelError("Files", $"{file.FileName}  ,Main File 300 KB dan cox ola bilmez!");
						return View(product);
					}
					ProductImage productImage = new ProductImage
					{
						Image = await file.CreateFileAsync(_env, "assets", "images", "product"),
						CreatedAt = DateTime.UtcNow.AddHours(4),
						CreatedBy = "System"
					};

					productImages.Add(productImage);

				}

				dbproduct.ProductImages.AddRange(productImages);
			}
			if (product.MainFile != null)
			{
				if (!product.MainFile.CheckFileContentType("image/jpeg"))
				{
					ModelState.AddModelError("MainFile", "Main File ,Jpeg tipinde olmalidir!");
					return View(product);
				}
				if (!product.MainFile.CheckFileLength(1500))
				{
					ModelState.AddModelError("MainFile", "Main File ,Main File 1.5 MB dan cox ola bilmez!");
					return View(product);
				}
				FileHelper.DeleteFile(dbproduct.MainImage, _env, "assets", "images", "product");
				dbproduct.MainImage = await product.MainFile.CreateFileAsync(_env, "assets", "images", "product");
			}
			if (product.HoverFile != null)
			{
				if (!product.HoverFile.CheckFileContentType("image/jpeg"))
				{
					ModelState.AddModelError("HoverFile", "Hover File ,Jpeg tipinde olmalidir!");
					return View(product);
				}
				if (!product.MainFile.CheckFileLength(1500))
				{
					ModelState.AddModelError("HoverFile", "Hover File  1.5 MB dan cox ola bilmez!");
					return View(product);
				}
				FileHelper.DeleteFile(dbproduct.HoverImage, _env, "assets", "images", "product");
				dbproduct.HoverImage = await product.HoverFile.CreateFileAsync(_env, "assets", "images", "product");
			}
			if (product.TagIds != null && product.TagIds.Count() > 0)
			{
				_context.ProductTags.RemoveRange(dbproduct.ProductTags);

				List<ProductTag> productTags = new List<ProductTag>();
				foreach (int tagId in product.TagIds)
				{
					if (!await _context.Tags.AnyAsync(b => b.IsDeleted == false && b.Id == tagId))
					{
						ModelState.AddModelError("BrandId", $"Daxil Olunan Tag Id {tagId} yanlisdir");

						return View(product);
					}
					ProductTag productTag = new ProductTag
					{
						TagId = tagId,
						CreatedAt = DateTime.UtcNow.AddHours(4),
						CreatedBy = "System"

					};

					productTags.Add(productTag);
				}

				dbproduct.ProductTags = productTags;
			}
			else
			{
				ModelState.AddModelError("TagIds", "Tag mutleq secilmelidir");
				return View(product);
			}
			dbproduct.Title= product.Title;
			dbproduct.Description= product.Description;
			dbproduct.Price= product.Price;
			dbproduct.DiscountedPrice= product.DiscountedPrice;
			
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		} 





	}
}
