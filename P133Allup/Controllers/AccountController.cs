using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using P133Allup.Models;
using P133Allup.ViewModels.AccountViewModels;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace P133Allup.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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


    }
}
