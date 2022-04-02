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

        public async Task<IActionResult> Index()
        {
            try
            {
                var userID = HttpContext.User.Claims.ToList()[0].Value;
                ViewBag.Connections = await _connectionRepository.GetConnectionsByUserID(Convert.ToInt64(userID));
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = ex.Message;
                return RedirectToAction("Error", "Public");
            }
        }
    }
}
