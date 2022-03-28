using FTP_Client.Helpers;
using FTP_Client.Mappers;
using FTP_Client.Repository;
using FTP_Client.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FTP_Client.Pages
{
    [Authorize]
    public class NewConnectionPopupModel : PageModel
    {
        private readonly IConnectionRepository _connectionRepository;

        public NewConnectionPopupModel(IConnectionRepository connectionRepository)
        {
            _connectionRepository = connectionRepository;
        }

        [BindProperty]
        public NewConnectionViewModel NewConnectionModel { get; set; }

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

                NewConnectionModel.UserID = Convert.ToInt64(HttpContext.User.Claims.ToList()[0].Value);
                var connection = new NewConnectionToConnectionMapper().Map(NewConnectionModel);
                var createConnection = await _connectionRepository.CreateConnection(connection);

                return StatusCode(200, new { success = true, errors = new List<string> { "Connection Created Successfully" } });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { success = false, errors = new List<string> { "Connection Name Already Used" } });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, errors = new List<string> { ex.Message } });
            }
        }
    }
}
