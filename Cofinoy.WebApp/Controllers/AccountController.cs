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
using System.Linq;
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
        private readonly IEmailService _emailService;

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
                            IEmailService emailService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
            this._emailService = emailService;
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
            if (!ModelState.IsValid)
            {
                // Required fields are missing
                return View(model);
            }

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
                    return RedirectToAction("DrinkManagement", "Menu");
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Invalid credentials
                ViewData["ToastMessage"] = "Incorrect email or password";
                return View(model);
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
            if (!ModelState.IsValid)
            {
                ViewData["ToastMessage"] = "Please fix the errors in the form.";
                ViewData["ToastType"] = "danger";
                return View(model);
            }

            try
            {
                _userService.AddUser(model);
                TempData["ToastMessage"] = "Registration successful! You can now log in.";
                TempData["ToastType"] = "success";
                return RedirectToAction("Login", "Account");
            }
            catch (InvalidDataException ex)
            {
                ViewData["ToastMessage"] = ex.Message;
                ViewData["ToastType"] = "danger";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewData["ToastMessage"] = Resources.Messages.Errors.ServerError;
                ViewData["ToastType"] = "danger";
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginRequired()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RequestReset()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RequestReset(RequestResetViewModel model)
        {

            var user = _userService.GetUserByEmail(model.Email);
            if (user == null)
            {
                ViewData["ToastMessage"] = "No account found with this email.";
                ViewData["ToastType"] = "danger";
                return View();
            }

            var code = new Random().Next(100000, 999999).ToString();
            user.ResetCode = code;
            user.ResetCodeExpiry = DateTime.UtcNow.AddMinutes(10);
            _userService.UpdateUser(user);

            await _emailService.SendPasswordResetCodeAsync(model.Email, code);

            TempData["Email"] = model.Email;
            TempData["ToastMessage"] = "Verification code sent to your email.";
            TempData["ToastType"] = "success";

            return RedirectToAction("SendCode");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SendCode()
        {
            if (TempData["Email"] == null)
                return RedirectToAction("RequestReset");

            ViewBag.Email = TempData["Email"].ToString();
            TempData.Keep("Email");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult SendCode(SendCodeViewModel model)
        {
            var email = TempData["Email"]?.ToString();
            var user = _userService.GetUserByEmail(email);
            if (user == null || user.ResetCode != model.ResetCode || user.ResetCodeExpiry < DateTime.UtcNow)
            {
                TempData["ToastMessage"] = "Invalid or expired verification code.";
                TempData["ToastType"] = "danger";
                ViewBag.Email = email;
                return View(model);
            }

            TempData["VerifiedEmail"] = email;
            return RedirectToAction("NewPassword");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult NewPassword()
        {
            if (TempData["VerifiedEmail"] == null)
                return RedirectToAction("RequestReset");

            ViewBag.Email = TempData["VerifiedEmail"].ToString();
            TempData.Keep("VerifiedEmail");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult NewPassword(NewPasswordViewModel model)
        {
            var email = TempData["VerifiedEmail"]?.ToString();
            Console.WriteLine(email);
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("RequestReset");

            if (!ModelState.IsValid)
            {
                TempData.Keep("VerifiedEmail"); // keep TempData for next request
                ViewBag.Email = email;
                return View(model);
            }

            var user = _userService.GetUserByEmail(email);
            if (user == null)
            {
                Console.WriteLine("User not found"); 
                TempData["ToastMessage"] = "User not found.";
                TempData["ToastType"] = "danger";
                return RedirectToAction("RequestReset");
            }

            // Update password
            user.Password = PasswordManager.EncryptPassword(model.Password);
            user.ResetCode = null;
            user.ResetCodeExpiry = null;

            _userService.UpdateUser(user); // ensure SaveChanges is called inside

            TempData["ToastMessage"] = "Password successfully reset!";
            TempData["ToastType"] = "success";

            return RedirectToAction("Login");
        }


        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
