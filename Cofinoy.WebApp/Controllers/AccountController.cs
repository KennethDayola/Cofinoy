using AutoMapper;
using Cofinoy.Data.Models;
using Cofinoy.Services.Interfaces;
using Cofinoy.Services.Manager;
using Cofinoy.Services.ServiceModels;
using Cofinoy.WebApp.Authentication;
using Cofinoy.WebApp.Models;
using Cofinoy.WebApp.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using static Cofinoy.Resources.Constants.Enums;

namespace Cofinoy.WebApp.Controllers
{
    public class AccountController : ControllerBase<AccountController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public AccountController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
        }

        /// <summary>
        /// Login Method
        /// </summary>
        /// <returns>Created response view</returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            TempData["returnUrl"] = System.Net.WebUtility.UrlDecode(HttpContext.Request.Query["ReturnUrl"]);
            this._sessionManager.Clear();
            this._session.SetString("SessionId", System.Guid.NewGuid().ToString());
            return this.View();
        }

        /// <summary>
        /// Authenticate user and signs the user in when successful.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns> Created response view </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            this._session.SetString("HasSession", "Exist");

            User user = null;
            var loginResult = _userService.AuthenticateUser(model.Email, model.Password, ref user);
            if (loginResult == LoginResult.Success)
            {
                // Authentication OK
                await this._signInManager.SignInAsync(user);
                this._session.SetString("UserName", user.Nickname);

                if (user.Email != null && user.Email.Equals("admin@cofinoy.com", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Nilogin");
                    return RedirectToAction("DrinkManagement", "Menu");
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (!ModelState.IsValid)
                    return View(model);

                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register(UserViewModel model)
        {
            try
            {
                _userService.AddUser(model);
                TempData["ToastMessage"] = "Registration successful! You can now log in.";
                TempData["ToastType"] = "success";
                return RedirectToAction("Login", "Account");
            }
            catch(InvalidDataException ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
                TempData["ErrorMessage"] = ex.Message;
            }
            catch(Exception ex)
            {
                TempData["ToastMessage"] = Resources.Messages.Errors.ServerError;
                TempData["ToastType"] = "danger";
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
            }
            return View();
        }

        [AllowAnonymous]
        public IActionResult LoginRequired()
        {
            
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            _sessionManager.Clear();    
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ProfileDetails()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the logged-in user’s full details
            var user = _userService.GetUserByEmail(email);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdatePersonalInfo([FromBody] User model)
        {
            try
            {
                var currentEmail = User.FindFirstValue(ClaimTypes.Email);
                var user = _userService.GetUserByEmail(currentEmail);

                if (user == null)
                    return Json(new { success = false, message = "User not found." });

                // Check if email is being updated
                if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
                {
                    if (_userService.EmailExists(model.Email))
                        return Json(new { success = false, message = "Email is already in use." });

                    user.Email = model.Email;
                }

                // Update other personal info
                if (!string.IsNullOrEmpty(model.FirstName)) user.FirstName = model.FirstName;
                if (!string.IsNullOrEmpty(model.LastName)) user.LastName = model.LastName;
                if (!string.IsNullOrEmpty(model.Nickname)) user.Nickname = model.Nickname;
                if (model.BirthDate != default(DateOnly)) user.BirthDate = model.BirthDate;
                if (!string.IsNullOrEmpty(model.PhoneNumber)) user.PhoneNumber = model.PhoneNumber;

                // Save updates
                _userService.UpdateUser(user);

                // Refresh session and claims if email changed
                if (model.Email != currentEmail)
                {
                    // Sign out the old claims
                    await _signInManager.SignOutAsync();

                    // Sign in with updated claims
                    await _signInManager.SignInAsync(user);

                    // Update session if you store email there
                    _session.SetString("UserName", user.Nickname);
                }

                return Json(new { success = true, message = "Personal info updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating personal info." });
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult UpdateAddress([FromBody] User model)
        {
            try
            {
                var currentEmail = User.FindFirstValue(ClaimTypes.Email);
                var user = _userService.GetUserByEmail(currentEmail);

                if (user == null)
                    return Json(new { success = false, message = "User not found." });

                if (!string.IsNullOrEmpty(model.Country)) user.Country = model.Country;
                if (!string.IsNullOrEmpty(model.City)) user.City = model.City;
                if (!string.IsNullOrEmpty(model.postalCode)) user.postalCode = model.postalCode;

                _userService.UpdateUser(user);

                return Json(new { success = true, message = "Address updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating address." });
            }
        }

        [HttpGet]
        public JsonResult IsAuthenticated()
        {
            var result = new
            {
                isAuthenticated = User.Identity.IsAuthenticated,
                userName = User.Identity.Name,
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            Console.WriteLine($"🟡 AUTH CHECK - Authenticated: {result.isAuthenticated}, User: {result.userName}, ID: {result.userId}");

            return Json(result);
        }
    }
}
