using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FTP_Client.Pages
{
    [Authorize]
    public class ConnectionsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
