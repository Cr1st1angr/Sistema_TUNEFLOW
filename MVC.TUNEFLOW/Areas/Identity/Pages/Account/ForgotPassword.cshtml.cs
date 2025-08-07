// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace MVC.TUNEFLOW.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                return RedirectToPage("./ForgotPasswordConfirmation");

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                null,
                new { area = "Identity", code },
                Request.Scheme);

            var htmlMessage = $@"
            <div style='font-family: Arial, sans-serif; background-color: #fff8e1; padding: 30px; border-radius: 10px; color: #333;'>
             <h2 style='color: #ff9800;'>🔐 ¿Olvidaste tu contraseña?</h2>
            <p>Hola 👋,</p>

            <p>No te preocupes, ¡a todos nos pasa! Solo haz clic en el botón de abajo para restablecer tu contraseña y volver a disfrutar de la música.</p>

            <div style='text-align: center; margin: 30px 0;'>
             <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='background-color: #ff9800; color: white; padding: 12px 25px; text-decoration: none; border-radius: 6px; font-weight: bold;'>
            Restablecer contraseña
              </a>
             </div>

            <p>Si no solicitaste este cambio, puedes ignorar este mensaje. Tu cuenta está segura.</p>

            <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
             <p style='font-size: 12px; color: #999;'>
            Este correo fue enviado automáticamente por TuneFlow. No respondas a este mensaje.
             </p>
            </div>";

            await _emailSender.SendEmailAsync(Input.Email, "Restablecer contraseña", htmlMessage);


            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}