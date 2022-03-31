using FTP_Client.Helpers;
using FTP_Client.Mappers;
using FTP_Client.Repository;
using FTP_Client.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
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
                {
                    var errorList = ValidationHelper.GetValidationErrMsgs(ModelState.Values);
                    return BadRequest(new { success = false, errors = errorList });
                }

                var isUsernameTaken = await _authRepository.IsUsernameAlreadyTaken(RegisterViewModel.Username);
                if (isUsernameTaken)
                {
                    ModelState.AddModelError("Username", "Username is Already Taken");
                    var errorList = ValidationHelper.GetValidationErrMsgs(ModelState.Values);
                    return BadRequest(new { success = false, errors = errorList });
                }

                RegisterViewModel.Password = _hashing.Hash(RegisterViewModel.Password);

                // mapping
                var user = new RegisterViewModelToUserMapper().Map(RegisterViewModel);

                var registerAccount = await _authRepository.Register(user);
                return StatusCode(200, new { success = true, errors = new List<string> { }, redirectUrl = "./Login" });
            }
            catch (System.Exception ex)
            {
                // log error
                return BadRequest(new { success = false, errors = new List<string> { ex.Message } });
            }
        }
    }
}
