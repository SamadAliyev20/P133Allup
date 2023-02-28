using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using P133Allup.Areas.Manage.ViewModels.AccountVMs;
using P133Allup.Models;

namespace P133Allup.Areas.Manage.Controllers
{

	[Area("Manage")]
	public class AccountController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_roleManager = roleManager;
		}

		public async Task<IActionResult> Register()
		{
			return View();
		}

		[HttpPost]

		public async Task<IActionResult> Register(RegisterVM registerVM)
		{
			if (!ModelState.IsValid) return View(registerVM);

			AppUser appUser = new AppUser
			{
				Name = registerVM.Name,
				Email = registerVM.Email,
				SurName = registerVM.SurName,
				FatherName = registerVM.FatherName,
				UserName = registerVM.UserName
			};			
			IdentityResult identityResult =  await _userManager.CreateAsync(appUser,registerVM.Password);
			

			if (!identityResult.Succeeded)
			{
				foreach (IdentityError identityError in identityResult.Errors)
				{
					ModelState.AddModelError("", identityError.Description);	
				}
				return View(registerVM);
			}
			await _userManager.AddToRoleAsync(appUser, "Admin");
			return RedirectToAction("index", "dashboard", new { areas = "manage" });
		}

		//[HttpGet]
		//public async Task<IActionResult> CreateRole()
		//{
		//	await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
		//	await _roleManager.CreateAsync(new IdentityRole("Admin"));
		//	await _roleManager.CreateAsync(new IdentityRole("Member"));

		//	return Content("Success");

		//}

		//[HttpGet]
		
		//public async Task<IActionResult> CreateUser()
		//{
		//	AppUser appUser = new AppUser
		//	{
  //            Name="Super",
		//	  SurName="Admin",
		//	  FatherName="SuperAdminFather",
		//	  UserName="SuperAdmin",
		//	  Email="superadmin@gmail.com"

		//	};

		//	await _userManager.CreateAsync(appUser,"SuperAdmin133");
		//	await _userManager.AddToRoleAsync(appUser, "SuperAdmin");

		//	return Content("Success");
		//}
	}
}