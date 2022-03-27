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
            if(!ModelState.IsValid)
                return Page();

            LoginViewModel.Password = _hashing.Hash(LoginViewModel.Password);
            var loginResult = await _authRepository.Login(LoginViewModel);
            if(loginResult == null)
                return Page();

            await HttpContext.SignInAsync(Autenticate(loginResult));

            return RedirectToPage("./Index");
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
