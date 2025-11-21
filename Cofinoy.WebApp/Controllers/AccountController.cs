using AutoMapper;
using Cofinoy.Data.Models;
using Cofinoy.Services.Interfaces;
using Cofinoy.Services.Manager;
using Cofinoy.Services.ServiceModels;
using Cofinoy.Services.Services;
using Cofinoy.WebApp.Authentication;
using Cofinoy.WebApp.Models;
using Cofinoy.WebApp.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Required fields are missing
                return View(model);
            }

            //Creates a new session named "HasSession" and sets its value to "Exist"
            this._session.SetString("HasSession", "Exist");

            User user = null;
            var loginResult = _userService.AuthenticateUser(model.Email, model.Password, ref user);
            if (loginResult == LoginResult.Success) //If _userService.AuthenticateUser returns Success
            {
                await this._signInManager.SignInAsync(user);
                this._session.SetString("UserName", user.Nickname);

                if (user.Email != null && user.Email.Equals("admin@cofinoy.com", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "Home");
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
        public IActionResult Register(UserServiceModel model)
        {
            if (!ModelState.IsValid)
            {
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
                //Shows error if user already exists
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
            // Gets the information of the user through email
            var user = _userService.GetUserByEmail(model.Email);
            if (user == null)
            {
                ViewData["ToastMessage"] = "No account found with this email.";
                ViewData["ToastType"] = "danger";
                return View();
            }

            // If a reset code already exists and hasn't expired, don't overwrite it.
            // Instead inform the user (toast) and redirect to the SendCode page.
            if (!string.IsNullOrEmpty(user.ResetCode) && user.ResetCodeExpiry.HasValue && user.ResetCodeExpiry.Value > DateTime.UtcNow)
            {
                TempData["Email"] = model.Email;
                TempData["ToastMessage"] = "A verification code was already sent and is still valid. Please check your email.";
                TempData["ToastType"] = "info";
                return RedirectToAction("SendCode");
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
            // Use Peek so TempData isn't consumed on failed attempts
            var email = TempData.Peek("Email")?.ToString();
            var user = _userService.GetUserByEmail(email);
            if (user == null || user.ResetCode != model.ResetCode || (user.ResetCodeExpiry.HasValue && user.ResetCodeExpiry.Value < DateTime.UtcNow))
            {
                TempData["ToastMessage"] = "Invalid or expired verification code.";
                TempData["ToastType"] = "danger";
                ViewBag.Email = email;
                TempData.Keep("Email");
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

            // Prevent setting the new password to the same value as the existing one
            if (PasswordManager.VerifyPassword(model.Password, user.Password))
            {
                // Keep VerifiedEmail so the user can try again
                TempData.Keep("VerifiedEmail");
                ViewBag.Email = email;

                TempData["ToastMessage"] = "New password cannot be the same as your current password.";
                TempData["ToastType"] = "danger";

                return View(model);
            }

            // Update password
            user.Password = PasswordManager.EncryptPassword(model.Password);
            user.ResetCode = null;
            user.ResetCodeExpiry = null;

            _userService.UpdateUser(user); // ensure SaveChanges is called inside

            TempData["ToastMessage"] = "Password has been sucessfully reset!";
            TempData["ToastType"] = "success";

            return RedirectToAction("Login");
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

            var profileDetails = _userService.GetProfileDetails(email);

            if (profileDetails == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new ProfileViewModel
            {
                User = profileDetails.User,
                ChangePassword = new ChangePasswordViewModel()
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdatePersonalInfo([FromBody] PersonalInfoViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return Json(new { success = false, errors });
                }

                var currentEmail = User.FindFirstValue(ClaimTypes.Email);

                var serviceModel = _mapper.Map<Services.ServiceModels.PersonalInfoServiceModel>(model);
                var result = _userService.UpdatePersonalInfo(currentEmail, serviceModel);

                if (!result.Success)
                {
                    return Json(new { success = false, errors = result.Errors, message = result.Message });
                }

                // Refresh session and claims if email changed
                if (result.EmailChanged)
                {
                    await _signInManager.SignOutAsync();
                    await _signInManager.SignInAsync(result.UpdatedUser);
                    _session.SetString("UserName", result.UpdatedUser.Nickname);
                }

                return Json(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating personal info");
                return Json(new { success = false, errors = new Dictionary<string, string[]> { { "General", new[] { "An error occurred while updating personal info." } } } });
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult UpdateAddress([FromBody] AddressViewModel model)
        {
            try
            {
                var currentEmail = User.FindFirstValue(ClaimTypes.Email);

                var serviceModel = _mapper.Map<Services.ServiceModels.AddressServiceModel>(model);
                var result = _userService.UpdateAddress(currentEmail, serviceModel);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address");
                return Json(new { success = false, errors = new { General = new[] { "An error occurred while updating address." } } });
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return Json(new { success = false, errors });
                }

                var email = User.FindFirstValue(ClaimTypes.Email);

                // Map ViewModel to ServiceModel
                var serviceModel = _mapper.Map<Services.ServiceModels.ChangePasswordServiceModel>(model);

                // Call service method
                var result = _userService.ChangePassword(email, serviceModel);

                if (!result.Success)
                {
                    return Json(new { success = false, errors = result.Errors });
                }

                return Json(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return Json(new
                {
                    success = false,
                    errors = new Dictionary<string, string[]> {
                { "General", new[] { "An error occurred while changing password." } }
            }
                });
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
