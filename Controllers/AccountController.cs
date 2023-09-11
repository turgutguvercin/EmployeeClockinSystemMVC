using EmployeeClockinSystem.Data;
using EmployeeClockinSystem.Interfaces;
using EmployeeClockinSystem.Models;
using EmployeeClockinSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeClockinSystem.Controllers
{
    public class AccountController : Controller
    {
        // Dependency injection.
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IAccountRepository _accountRepository;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ApplicationDbContext context, IAccountRepository accountRepository)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _accountRepository = accountRepository;
        }

        // Display the Login page.
        public IActionResult Login()
        {
            var response = new LoginViewModel();
            return View(response);
        }

        // Handle login form submission.
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (!ModelState.IsValid) return View(loginVM);
            var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);
            if (user != null)
            {

                // Attempt to sign in.
                var passwordCheck = await _userManager.CheckPasswordAsync(user, loginVM.Password);
                if (passwordCheck)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);
                    if (result.Succeeded)
                    {
                        // Redirect to tour listing page after successful login.
                        return RedirectToAction("Index", "Dashboard");

                    }
                }
                // Password is wrong.
                TempData["Error"] = "Invalid login details.";
                return View(loginVM);

            }

            // User not found.
            TempData["Error"] = "Invalid login details.";
            return View(loginVM);
        }

        // Display the Registration page.
        public IActionResult Register()
        {
            var response = new RegisterViewModel();
            return View(response);
        }

        // Handle registration form submission.
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);

            // Check if user already exists by email.
            var user = await _userManager.FindByEmailAsync(registerVM.EmailAddress);
            if (user != null)
            {
                TempData["Error"] = "This email address is already in use";
                return View(registerVM);
            }

            // Create user.
            var newUser = new AppUser()
            {
                Email = registerVM.EmailAddress,
                UserName = registerVM.EmailAddress,
            };
            var newUserResponse = await _userManager.CreateAsync(newUser, registerVM.Password);
            if (newUserResponse.Succeeded)
            {
            
                await _userManager.AddToRoleAsync(newUser, UserRoles.User);
                TempData["SuccessMessage"] = "Registration successful! Welcome. You will be redirected to login page.";
                ViewBag.JavaScriptFunction = "redirectAfterTimeout();";
            }

            return View(registerVM);

        }

        // Log the user out.
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        // List the all users for admin
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _accountRepository.GetAllUsers();
            List<AccountUserViewModel> result = new List<AccountUserViewModel>();

            foreach (var user in users)
            {

                var accountUserVM = new AccountUserViewModel
                {
                    UserId = user.Id, 
                    UserEmail = user.Email,
                    EmployeeName = user.Employee?.FullName,
                    Position = user.Employee?.Position,
                    IsEmployee = user.Employee != null
                };
                result.Add(accountUserVM);
            }

            return View(result);
        }

        // Edit user if they are identified as an employee
        public async Task<IActionResult> EditUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new AccountUserViewModel()
            {
                UserId = user.Id,
                UserEmail = user.Email,
                EmployeeName = user.Employee?.FullName,
                Position = user.Employee?.Position,
                IsEmployee = user.Employee != null

            };

            return View(model);
        }

        // Register a user as an employee.
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddEmployeeDetails(AccountUserViewModel accountUserVM)
        {
            if (ModelState.IsValid)
            {
                // Check if the user already has an associated Employee entry
                var user = await _accountRepository.GetUserById(accountUserVM.UserId);
                var existingEmployee = user.Employee;
            
                if (existingEmployee != null)
                {
                    // Update details of the existing employee
                    existingEmployee.FullName = accountUserVM.EmployeeName;
                    existingEmployee.Position = accountUserVM.Position;

                    _accountRepository.UpdateEmployee(existingEmployee);
                }
                else
                {
                    // Create a new Employee entry
                    var employee = new Employee
                    {
                        AppUserId = accountUserVM.UserId,
                        FullName = accountUserVM.EmployeeName,
                        Position = accountUserVM.Position
                    };

                 _accountRepository.AddEmployee(employee);
                }

                return RedirectToAction("GetAllUsers");
            }

            return View(accountUserVM);
        }

    }
}