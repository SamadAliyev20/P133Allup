using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P133Allup.DataAccessLayer;
using P133Allup.Models;

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

		public async Task<IActionResult> Index()
		{

			return View(await _context.Categories.Include(c => c.Products).Where(c => c.IsDeleted == false && c.IsMain).ToListAsync());
		}

		public async Task<IActionResult> Create()
		{
			ViewBag.Categories = await _context.Categories.Include(c => c.Products).Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();


			return View();
		}
		[HttpPost]

		public async Task<IActionResult> Create(Category category)
		{
			ViewBag.Categories = await _context.Categories.Include(c => c.Products).Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();

			if (!ModelState.IsValid)
			{
				return View(category);
			}
			if (await _context.Categories.AnyAsync(c=>c.IsDeleted==false && c.Name.ToLower().Contains(category.Name.Trim().ToLower())))
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
					string fileName = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}-{Guid.NewGuid().ToString()}-{category.File.FileName}";

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
			category.CreatedAt= DateTime.UtcNow.AddHours(4);
			category.CreatedBy = "System";


			await _context.Categories.AddAsync(category);

			await _context.SaveChangesAsync();



			return RedirectToAction(nameof(Index));
		}


	}
}
