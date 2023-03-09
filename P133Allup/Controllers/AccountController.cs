using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P133Allup.DataAccessLayer;
using P133Allup.Models;
using P133Allup.ViewModels.AccountViewModels;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace P133Allup.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _context;

		public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppDbContext context)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_context = context;
		}

		public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            AppUser appUser = new AppUser

            {
                Name = registerVM.Name,
                Email = registerVM.Email,
                SurName = registerVM.SurName,
                UserName = registerVM.UserName,
                FatherName = registerVM.FatherName
            };

            IdentityResult identityResult = await _userManager.CreateAsync(appUser, registerVM.Password);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                }
                return View(registerVM);


            }

            await _userManager.AddToRoleAsync(appUser, "Member");



            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }
            AppUser appUser = await _userManager.FindByEmailAsync(loginVM.Email);

         if (appUser == null)
            {
                ModelState.AddModelError("", "Email Ve ya Sifre Yanlisdir");
                return View(loginVM);
            }
          SignInResult signInResult = await _signInManager.PasswordSignInAsync(appUser, 
              loginVM.Password,loginVM.RemindMe,true);


            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelError("", "Hesabiniz Bloklanib");
                return View(loginVM);

            }

            if (!signInResult.Succeeded) 
            {
                ModelState.AddModelError("", "Email Ve ya Sifre Yanlisdir");
                return View(loginVM);

            }

            return RedirectToAction("Index", "Home");

        }

        [HttpGet]

        public async Task<ActionResult> Logout() 
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize(Roles ="Member")]

        public async Task<IActionResult> Profile()
        {
            AppUser appUser = await _userManager.Users
                .Include(u=>u.Addresses.Where(a=>a.IsDeleted== false))
                .FirstOrDefaultAsync(u=>u.NormalizedUserName == User.Identity.Name.ToUpperInvariant());

            ProfileVM profileVM = new ProfileVM
            {
                Addresses = appUser.Addresses

            };
            return View(profileVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="Member")]

        public async Task<IActionResult> AddAddress(Address address)
        {
			AppUser appUser = await _userManager.Users
			   .Include(u => u.Addresses.Where(a => a.IsDeleted == false))
			   .FirstOrDefaultAsync(u => u.NormalizedUserName == User.Identity.Name.ToUpperInvariant());

			ProfileVM profileVM = new ProfileVM
			{
				Addresses = appUser.Addresses

			};

			if (!ModelState.IsValid)
            
            { 

                return View(nameof(Profile),profileVM);
            }

            if (address.IsMain && appUser.Addresses != null && appUser.Addresses.Count() > 0 && appUser.Addresses.Any(a=>a.IsMain))
            {
                appUser.Addresses.FirstOrDefault(a => a.IsMain).IsMain = false;
            }
            address.UserId = appUser.Id;
            address.CreatedBy = $"{appUser.Name} {appUser.SurName}";
            address.CreatedAt= DateTime.UtcNow.AddHours(4);

            await  _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();

           return RedirectToAction(nameof(Profile));
        }


    }
}
