using FTP_Client.Models;
using FTP_Client.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FTP_Client.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IConnectionRepository _connectionRepository;

        public List<Connection> Connections { get; private set; }

        public IndexModel(IConnectionRepository connectionRepository)
        {
            _connectionRepository = connectionRepository;
        }

        
        public async Task OnGet()
        {
            var userID = HttpContext.User.Claims.ToList()[0].Value;
            Connections = await _connectionRepository.GetConnectionsByUserID(Convert.ToInt64(userID));
        }
    }
}
