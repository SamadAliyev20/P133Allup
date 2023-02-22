using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P133Allup.DataAccessLayer;
using P133Allup.Models;
using P133Allup.ViewModels;

namespace P133Allup.Areas.Manage.Controllers
{
	[Area("manage")]
	public class CategoryController : Controller
	{
		private readonly AppDbContext _context;
		private readonly IWebHostEnvironment _env;

		public CategoryController(AppDbContext context, IWebHostEnvironment env)
		{
			_context = context;
			_env = env;
		}

		public async Task<IActionResult> Index(int pageindex = 1)
		{
			IQueryable<Category> categories = _context.Categories.Include(c => c.Products).Where(c => c.IsDeleted == false && c.IsMain);

			return View(PageNatedList<Category>.Create(categories,pageindex,3));
			
		}

		public async Task<IActionResult> Create()
		{
			ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();


			return View();
		}

		[HttpPost]

		public async Task<IActionResult> Create(Category category)
		{
			ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();

			if (!ModelState.IsValid)
			{
				return View(category);
			}
			if (await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.Name.ToLower().Contains(category.Name.Trim().ToLower())))
			{
				ModelState.AddModelError("Name", $"Bu adda {category.Name.Trim()} kateqoriya mövcuddur!");
				return View(category);
			}
			if (category.IsMain)
			{
				if (category.File == null)
				{
					ModelState.AddModelError("File", "Şekil boş ola bilmez!");
					return View(category);
				}

				if (category.File?.ContentType != "image/jpeg")
				{
					ModelState.AddModelError("File", "File uzantısı düzgün deyil!");
					return View();
				}
				if ((category.File?.Length / 1024) > 300)
				{
					ModelState.AddModelError("File", "File Ölçüsü 300 kb çox ola bilmez!");
					return View();
				}
				if (category.File != null)
				{
					string fileName = $"{Guid.NewGuid().ToString()}-{category.File.FileName}";

					string filePath = Path.Combine(_env.WebRootPath, "assets", "images", fileName);

					category.Image = fileName;

					using (FileStream stream = new FileStream(filePath, FileMode.Create))
					{
						await category.File.CopyToAsync(stream);
					}
				}
				category.ParentId = null;
			}
			else
			{
				if (category.ParentId == null)
				{
					ModelState.AddModelError("ParentId", "Üst kateqoriya mütleq seçilməlidir!");
					return View(category);
				}
				if (!await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.IsMain && c.Id == category.ParentId))
				{
					ModelState.AddModelError("ParentId", "Düzgün Üst kateqoriya  seçilməlidir!");
					return View(category);
				}
				category.Image = null;

			}
			category.Name = category.Name.Trim();
			category.CreatedAt = DateTime.UtcNow.AddHours(4);
			category.CreatedBy = "System";


			await _context.Categories.AddAsync(category);

			await _context.SaveChangesAsync();



			return RedirectToAction(nameof(Index));
		}

		[HttpGet]

		public async Task<IActionResult> Detail(int? id)
		{
            if (id == null)
            {
                return BadRequest();
            }
            Category category = await _context.Categories
                .Include(c => c.Children.Where(c => c.IsDeleted == false)).ThenInclude(c=>c.Products.Where(c=>c.IsDeleted==false))
                .Include(c => c.Products.Where(c => c.IsDeleted == false))
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (category == null) return NotFound();

            return View(category);
        }

		[HttpGet]
		public async Task<IActionResult> Update(int? id)
		{

			if (id == null)
			{
				return BadRequest();
			}
			Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

			ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();

			if (category == null) return NotFound();
			return View(category);
		}
		[HttpPost]
		public async Task<IActionResult> Update(int? id, Category category)
		{
			if (id == null) { return BadRequest(); }

			if (!ModelState.IsValid)
			{
				return View(category);
			}
			ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();


			if (id != category.Id) return BadRequest();

			Category dbCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

			if (dbCategory == null) return NotFound();

			if (category.IsMain)
			{
				if (dbCategory.IsMain)
				{
					if (category.File != null)
					{
						if (category.File?.ContentType != "image/jpeg")
						{
							ModelState.AddModelError("File", "File uzantısı düzgün deyil!");
							return View();
						}
						if ((category.File?.Length / 1024) > 300)
						{
							ModelState.AddModelError("File", "File Ölçüsü 300 kb çox ola bilmez!");
							return View();
						}
						if (category.File != null)
						{
							string fileName = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}-{Guid.NewGuid().ToString()}-{category.File.FileName}";

							string filePath = Path.Combine(_env.WebRootPath, "assets", "images", fileName);

							dbCategory.Image = fileName;

							using (FileStream stream = new FileStream(filePath, FileMode.Create))
							{
								await category.File.CopyToAsync(stream);
							}
						}
					}

				}
				else
				{
					if (category.File == null)
					{
						ModelState.AddModelError("File", "Şekil boş ola bilmez!");
						return View(category);
					}

					if (category.File?.ContentType != "image/jpeg")
					{
						ModelState.AddModelError("File", "File uzantısı düzgün deyil!");
						return View();
					}
					if ((category.File?.Length / 1024) > 300)
					{
						ModelState.AddModelError("File", "File Ölçüsü 300 kb çox ola bilmez!");
						return View();
					}
					if (category.File != null)
					{
						string fileName = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}-{Guid.NewGuid().ToString()}-{category.File.FileName}";

						string filePath = Path.Combine(_env.WebRootPath, "assets", "images", fileName);

						dbCategory.Image = fileName;

						using (FileStream stream = new FileStream(filePath, FileMode.Create))
						{
							await category.File.CopyToAsync(stream);
						}
					}

				}

				dbCategory.ParentId = null;

			}
			else
			{
				if (category.ParentId == null)
				{
					ModelState.AddModelError("ParentId", "Üst kateqoriya mütleq seçilməlidir!");
					return View(category);
				}
				if (!await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.IsMain && c.Id == category.ParentId))
				{
					ModelState.AddModelError("ParentId", "Düzgün Üst kateqoriya  seçilməlidir!");
					return View(category);
				}
				dbCategory.Image = null;
				dbCategory.ParentId = category.ParentId;

			}
			dbCategory.Name = category.Name.Trim();
			dbCategory.IsMain = category.IsMain;
			dbCategory.UpdatedBy = "System";
			dbCategory.UpdatedAt = DateTime.UtcNow.AddHours(4);

			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));



		}

		[HttpGet]

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return BadRequest();
			}
			Category category = await _context.Categories
				.Include(c => c.Children.Where(c=>c.IsDeleted==false))
				.Include(c => c.Products.Where(c => c.IsDeleted == false))
				.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

			if (category == null) return NotFound();

			return View(category);
		}

		[HttpGet]

		public async Task<IActionResult> DeleteCategory(int? id)
		{
            if (id == null)
            {
                return BadRequest();
            }
            Category category = await _context.Categories
                .Include(c => c.Children.Where(c => c.IsDeleted == false))
                .Include(c => c.Products.Where(c => c.IsDeleted == false))
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (category == null) return NotFound();

			category.IsDeleted= true;
			category.DeletedAt= DateTime.UtcNow.AddHours(4);
			category.DeletedBy = "System";

			await _context.SaveChangesAsync();

			return RedirectToAction("Index");
        }



	}
}
