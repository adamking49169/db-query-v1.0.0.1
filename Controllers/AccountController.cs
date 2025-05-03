using db_query_v1._0._0._1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using db_query_v1._0._0._1.Services;

namespace db_query_v1._0._0._1.Controllers
{
    public class AccountController : Controller
    {
         private readonly IPlanService _planService;
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IPlanService planService, IUserService userService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _planService = planService;
            _userService = userService;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: /Account/Register
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    NormalizedUserName = _userManager.NormalizeName(model.Email),
                    Email = model.Email,
                    NormalizedEmail = _userManager.NormalizeEmail(model.Email),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth,
                    Gender = "",
                    Phone = "",
                    Address = model.Address,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Specializations = string.Join(",", model.Specializations),
                    EmailConfirmed = true, // Set to true for testing purposes
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Optional: Add a TempData message for a success flash message
                    TempData["SuccessMessage"] = $"Welcome, {user.FirstName}!";

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Clear sensitive fields
            model.Password = string.Empty;
            model.ConfirmPassword = string.Empty;

            return View(model);
        }


        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {


                //var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                var user = await _userManager.FindByEmailAsync(model.Email);
                //var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null)
                {
                    // Optionally: check email confirmation up-front
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("", "You need to confirm your email before you can log in.");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(
                        user.UserName,
                        model.Password,
                        model.RememberMe,
                        lockoutOnFailure: true);

                    if (result.Succeeded)
                        return RedirectToAction("Index", "Home");

                    if (result.IsLockedOut)
                        ModelState.AddModelError("", "Your account is locked out.");
                    else if (result.IsNotAllowed)
                        ModelState.AddModelError("", "Login not allowed. Make sure your email is confirmed.");
                    else if (result.RequiresTwoFactor)
                        return RedirectToAction("LoginWith2fa", new { model.RememberMe });
                    else
                        ModelState.AddModelError("", "Invalid password.");
                }
                else
                {
                    ModelState.AddModelError("", "No account found with this email.");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // GET: /Account/Settings
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            // TODO: create a SettingsViewModel to populate with user data
            var model = new SettingsViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                Specializations = user.Specializations?.Split(',') ?? Array.Empty<string>()
            };

            return View(model);
        }

        // POST: /Account/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            // Update user fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.Phone;
            user.Address = model.Address;
            user.DateOfBirth = model.DateOfBirth;
            user.Specializations = string.Join(",", model.Specializations);

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Your settings have been saved.";
                return RedirectToAction(nameof(Settings));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        

        // GET: /Account/Upgrade
        [HttpGet]
        public IActionResult Upgrade()
        {
            var userId = User.Identity.Name; // or however you get your user
            var vm = new UpgradeViewModel
            {
                CurrentPlan = _userService.GetCurrentPlan(userId),
                AvailablePlans = _planService.GetAllPlanNames()
            };

            return View(vm);
        }

        // POST: /Account/Upgrade
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upgrade(UpgradeViewModel model)
        {
            // repopulate the list if we need to redisplay
            model.AvailablePlans = _planService.GetAllPlanNames();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity.Name;
            _userService.UpgradePlan(userId, model.SelectedPlan);

            TempData["SuccessMessage"] =
                $"🎉 Your plan has been upgraded to “{model.SelectedPlan}”!";

            // PRG pattern—redirect back to GET so refresh won’t repost
            return RedirectToAction(nameof(Upgrade));
        }

      

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
