using FTP_Client.Helpers;
using FTP_Client.Mappers;
using FTP_Client.Repository;
using FTP_Client.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Threading.Tasks;

namespace FTP_Client.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthRepository _authRepository;
        private readonly IHashing _hashing;

        public RegisterModel(IAuthRepository authRepository, IHashing hashing)
        {
            _authRepository = authRepository;
            _hashing = hashing;
        }

        [BindProperty]
        public RegiserViewModel RegisterViewModel { get; set; }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                var isUsernameTaken = await _authRepository.IsUsernameAlreadyTaken(RegisterViewModel.Username);
                if (isUsernameTaken)
                {
                    ModelState.AddModelError("Username", "Username is Already Taken");
                    return Page();
                }

                RegisterViewModel.Password = _hashing.Hash(RegisterViewModel.Password);

                // mapping
                var user = new RegisterViewModelToUserMapper().Map(RegisterViewModel);

                var registerAccount = await _authRepository.Register(user);
                return RedirectToPage("/Login");
            }
            catch (System.Exception ex)
            {
                // log error
                return Page();
            }
        }
    }
}
