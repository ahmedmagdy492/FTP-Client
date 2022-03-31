using FTP_Client.Helpers;
using FTP_Client.Models;
using FTP_Client.Repository;
using FTP_Client.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FTP_Client.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthRepository _authRepository;
        private readonly IHashing _hashing;

        [BindProperty]
        public LoginViewModel LoginViewModel { get; set; }

        public LoginModel(IAuthRepository authRepository, IHashing hashing)
        {
            _authRepository = authRepository;
            _hashing = hashing;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errorList = ValidationHelper.GetValidationErrMsgs(ModelState.Values);
                    return BadRequest(new { success = false, errors = errorList });
                }

                LoginViewModel.Password = _hashing.Hash(LoginViewModel.Password);
                var loginResult = await _authRepository.Login(LoginViewModel);
                if (loginResult == null)
                {
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid Username or Password" } });
                }

                await HttpContext.SignInAsync(Autenticate(loginResult));

                return StatusCode(200, new { success = true, errors =new List<string> { }, redirectUrl = "./Index" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, errors = new List<string> { ex.Message } });
            }
        }

        #region Methods
        private ClaimsPrincipal Autenticate(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Name, user.Name.ToString()),
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            return new ClaimsPrincipal(new[] { identity });
        }
        #endregion
    }
}
