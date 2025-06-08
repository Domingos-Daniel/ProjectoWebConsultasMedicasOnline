using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ConsultasMedicasOnline.Models;

namespace ConsultasMedicasOnline.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<Usuario> signInManager, 
            ILogger<LoginModel> logger,
            UserManager<Usuario> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "O email é obrigatório")]
            [EmailAddress(ErrorMessage = "Digite um email válido")]
            public string Email { get; set; }

            [Required(ErrorMessage = "A senha é obrigatória")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Lembrar de mim")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        
            if (ModelState.IsValid)
            {
                // Verificar se o usuário existe
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email ou senha incorretos.");
                    return Page();
                }

                // Verificar se a conta está ativa
                if (!user.Ativo)
                {
                    ModelState.AddModelError(string.Empty, "Sua conta foi desativada. Entre em contato com o suporte.");
                    return Page();
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    ModelState.AddModelError(string.Empty, "Sua conta foi temporariamente bloqueada devido a várias tentativas de login malsucedidas.");
                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email ou senha incorretos.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
