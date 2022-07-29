using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Net.Codecrete.QrCodeGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTP_Client.Controllers
{
    public class MobileConnectionController : Controller
    {
        private readonly IConfiguration _configuration;

        public MobileConnectionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GenerateCode()
        {
            string endPoint = _configuration["Options:UrlForQrCodeGeneration"];
            var qrCode = QrCode.EncodeText(endPoint, QrCode.Ecc.Medium);
            string svg = qrCode.ToSvgString(4);
            return StatusCode(200, new { success = true, message = "Success", errors = new List<string> { }, data = svg });
        }

        [HttpPost]
        public IActionResult SendFiles()
        {

        }
    }
}
