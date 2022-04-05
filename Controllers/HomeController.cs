using FTP_Client.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTP_Client.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IConnectionRepository _connectionRepository;

        public HomeController(IConnectionRepository connectionRepository)
        {
            _connectionRepository = connectionRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetConnectionList()
        {
            try
            {
                var userID = HttpContext.User.Claims.ToList()[0].Value;
                var connections = await _connectionRepository.GetConnectionsByUserID(Convert.ToInt64(userID));

                return PartialView("Home/_ConnectionList", connections);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return PartialView("_Result");
            }
        }
    }
}
