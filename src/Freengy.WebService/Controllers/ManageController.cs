// Created by Laxale 01.12.2016
//
//


namespace Freengy.WebService.Controllers 
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using Freengy.WebService.Models;
    using Freengy.WebService.Models.ManageViewModels;
    using Freengy.WebService.Services;


    [Authorize]
    public class ManageController : Controller 
    {
        #region vars

        private readonly ILogger logger;
        private readonly ISmsSender smsSender;
        private readonly IEmailSender emailSender;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        #endregion vars


        #region ctor

        public ManageController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory) 
        {
            this.smsSender = smsSender;
            this.emailSender = emailSender;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = loggerFactory.CreateLogger<ManageController>();
        }
        
        #endregion ctor


        //
        // GET: /Manage/Index
        [HttpGet]
        public async Task<IActionResult> Index(ManageMessageId? message = null) 
        {
            this.ViewData["StatusMessage"] =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                return this.View("Error");
            }

            var model = new IndexViewModel
            {
                HasPassword = await this.userManager.HasPasswordAsync(user),
                PhoneNumber = await this.userManager.GetPhoneNumberAsync(user),
                TwoFactor = await this.userManager.GetTwoFactorEnabledAsync(user),
                Logins = await this.userManager.GetLoginsAsync(user),
                BrowserRemembered = await this.signInManager.IsTwoFactorClientRememberedAsync(user)
            };

            return this.View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account) 
        {
            ManageMessageId? message = ManageMessageId.Error;

            var user = await this.GetCurrentUserAsync();
            if (user == null) return this.RedirectToAction(nameof(this.ManageLogins), new { Message = message });

            var result = await this.userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
            if (!result.Succeeded) return this.RedirectToAction(nameof(this.ManageLogins), new {Message = message});

            await this.signInManager.SignInAsync(user, isPersistent: false);
            message = ManageMessageId.RemoveLoginSuccess;

            return this.RedirectToAction(nameof(ManageLogins), new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public IActionResult AddPhoneNumber() 
        {
            return this.View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberViewModel model) 
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            // Generate the token and send it
            var user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                return this.View("Error");
            }

            string code = await this.userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
            await this.smsSender.SendSmsAsync(model.PhoneNumber, "Your security code is: " + code);

            return this.RedirectToAction(nameof(VerifyPhoneNumber), new { PhoneNumber = model.PhoneNumber });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableTwoFactorAuthentication() 
        {
            var user = await this.GetCurrentUserAsync();

            if (user == null) return this.RedirectToAction(nameof(this.Index), "Manage");

            await this.userManager.SetTwoFactorEnabledAsync(user, true);
            await this.signInManager.SignInAsync(user, isPersistent: false);

            this.logger.LogInformation(1, "User enabled two-factor authentication.");

            return this.RedirectToAction(nameof(Index), "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableTwoFactorAuthentication() 
        {
            var user = await this.GetCurrentUserAsync();
            if (user == null) return this.RedirectToAction(nameof(this.Index), "Manage");

            await this.userManager.SetTwoFactorEnabledAsync(user, false);
            await this.signInManager.SignInAsync(user, isPersistent: false);

            this.logger.LogInformation(2, "User disabled two-factor authentication.");

            return this.RedirectToAction(nameof(Index), "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        [HttpGet]
        public async Task<IActionResult> VerifyPhoneNumber(string phoneNumber) 
        {
            var user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                return this.View("Error");
            }

            var code = await this.userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
            // Send an SMS to verify the phone number
            return phoneNumber == null ? this.View("Error") : this.View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model) 
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this.GetCurrentUserAsync();
            if (user != null)
            {
                var result = await this.userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.Code);
                if (result.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, isPersistent: false);
                    return this.RedirectToAction(nameof(Index), new { Message = ManageMessageId.AddPhoneSuccess });
                }
            }

            // If we got this far, something failed, redisplay the form
            this.ModelState.AddModelError(string.Empty, "Failed to verify phone number");

            return this.View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePhoneNumber() 
        {
            var user = await this.GetCurrentUserAsync();
            if (user == null) return this.RedirectToAction(nameof(this.Index), new {Message = ManageMessageId.Error});

            var result = await this.userManager.SetPhoneNumberAsync(user, null);
            if (!result.Succeeded) return this.RedirectToAction(nameof(this.Index), new {Message = ManageMessageId.Error});

            await this.signInManager.SignInAsync(user, isPersistent: false);
            return this.RedirectToAction(nameof(this.Index), new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword() 
        {
            return this.View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model) 
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this.GetCurrentUserAsync();
            if (user == null) return this.RedirectToAction(nameof(this.Index), new {Message = ManageMessageId.Error});

            var result = await this.userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await this.signInManager.SignInAsync(user, isPersistent: false);
                this.logger.LogInformation(3, "User changed their password successfully.");

                return this.RedirectToAction(nameof(this.Index), new { Message = ManageMessageId.ChangePasswordSuccess });
            }

            this.AddErrors(result);
            return this.View(model);
        }

        //
        // GET: /Manage/SetPassword
        [HttpGet]
        public IActionResult SetPassword() 
        {
            return this.View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model) 
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this.GetCurrentUserAsync();
            if (user == null) return this.RedirectToAction(nameof(this.Index), new {Message = ManageMessageId.Error});

            var result = await this.userManager.AddPasswordAsync(user, model.NewPassword);
            if (result.Succeeded)
            {
                await this.signInManager.SignInAsync(user, isPersistent: false);
                return this.RedirectToAction(nameof(this.Index), new { Message = ManageMessageId.SetPasswordSuccess });
            }

            this.AddErrors(result);
            return this.View(model);
        }

        //GET: /Manage/ManageLogins
        [HttpGet]
        public async Task<IActionResult> ManageLogins(ManageMessageId? message = null) 
        {
            this.ViewData["StatusMessage"] =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.AddLoginSuccess ? "The external login was added."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";

            var user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                return this.View("Error");
            }

            var userLogins = await this.userManager.GetLoginsAsync(user);
            var otherLogins = this.signInManager.GetExternalAuthenticationSchemes().Where(auth => userLogins.All(ul => auth.AuthenticationScheme != ul.LoginProvider)).ToList();

            this.ViewData["ShowRemoveButton"] = user.PasswordHash != null || userLogins.Count > 1;

            return this.View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LinkLogin(string provider) 
        {
            // Request a redirect to the external login provider to link a login for the current user
            string redirectUrl = this.Url.Action("LinkLoginCallback", "Manage");
            var properties = this.signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, this.userManager.GetUserId(User));

            return this.Challenge(properties, provider);
        }

        //
        // GET: /Manage/LinkLoginCallback
        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback() 
        {
            var user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                return this.View("Error");
            }

            var info = await this.signInManager.GetExternalLoginInfoAsync(await this.userManager.GetUserIdAsync(user));
            if (info == null)
            {
                return this.RedirectToAction(nameof(ManageLogins), new { Message = ManageMessageId.Error });
            }

            var result = await this.userManager.AddLoginAsync(user, info);
            var message = result.Succeeded ? ManageMessageId.AddLoginSuccess : ManageMessageId.Error;

            return this.RedirectToAction(nameof(ManageLogins), new { Message = message });
        }

        #region Helpers

        private void AddErrors(IdentityResult result) 
        {
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public enum ManageMessageId 
        {
            AddPhoneSuccess,
            AddLoginSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        private Task<ApplicationUser> GetCurrentUserAsync() 
        {
            return this.userManager.GetUserAsync(this.HttpContext.User);
        }

        #endregion
    }
}