using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using GraduationProject.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Globalization;
using GraduationProject.Data.Repositories.Interfaces;

namespace GraduationProject.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AuthUser> _signInManager;
        private readonly UserManager<AuthUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IUserRepository<GraduationProject.Data.Models.User> _userrepository; 
        public List<string> Countries { get; set; }
        public RegisterModel(
            UserManager<AuthUser> userManager,
            SignInManager<AuthUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IUserRepository<GraduationProject.Data.Models.User> userrepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _userrepository = userrepository;
            AddCountries();

        }
        void AddCountries()
        {
            Countries = new List<string>();
            CultureInfo[] cultureInfos = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo cultureInfo in cultureInfos)
            {
                RegionInfo regionInfo = new(cultureInfo.LCID);
                if (!Countries.Contains(regionInfo.EnglishName))
                {
                    Countries.Add(regionInfo.EnglishName);
                }
            }
            Countries.Remove("Israel");
            Countries.Add("Palastine");
            Countries.Sort();

        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
           

            
            [Required]
            [Display(Prompt = "UserName")]
            public string UserName { get; set; }
            
            [Required]
            [EmailAddress]
            [Display(Prompt = "Email")]
            public string Email { get; set; }

            [Range(1, 9999, ErrorMessage = "You must Set Your Birth Date")]
            [Display(Prompt = "BirthDate")]
            public int BirthDate { get; set; }
            
            [Required(ErrorMessage = "You must Select A Country")]
            [Display(Prompt = "Country")]
            public string Country { get; set; }


            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Prompt = "Password")]
            public string Password { get; set; }

        }

        public async Task OnGetAsync(string returnUrl = null)
        {

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new AuthUser { UserName = Input.UserName, Email = Input.Email};
                var isEmail = await _userManager.FindByEmailAsync(Input.Email);
                if (isEmail == null)
                {
                    var result = await _userManager.CreateAsync(user, Input.Password);
                    if (result.Succeeded)
                    {
                        
                        AddUserToEntity(user.Id, Input);
                        _logger.LogInformation("User created a new account with password.");

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    foreach (var error in result.Errors)
                    {

                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }else
                {
                    ModelState.AddModelError(string.Empty, "Email '"+Input.Email+"' is already taken.");
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
        private void AddUserToEntity(string Id, InputModel Input)
        {
            var newUser = new GraduationProject.Data.Models.User { UserIdentityId = Id, UserName = Input.UserName, Country = Input.Country, BirthDateYear = Input.BirthDate, FirstName = Input.UserName }; 
            _userrepository.Add(newUser);
        }
    }
}
