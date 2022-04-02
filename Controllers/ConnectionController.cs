using FTP_Client.Helpers;
using FTP_Client.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTP_Client.Filters;
using FTP_Client.ViewModels;
using FTP_Client.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FTP_Client.Controllers
{
    [Authorize]
    public class ConnectionController : Controller
    {
        private IFTPClient _fTPClient;
        private readonly IConnectionRepository _connectionRepository;

        public ConnectionController(IConnectionRepository connectionRepository)
        {
            _connectionRepository = connectionRepository;
        }

        public IActionResult Test()
        {
            return StatusCode(200, new { data = "Test Works" });
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewConnection(NewConnectionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errorList = ValidationHelper.GetValidationErrMsgs(ModelState.Values);
                    return BadRequest(new { success = false, errors = errorList });
                }

                model.UserID = Convert.ToInt64(HttpContext.User.Claims.ToList()[0].Value);
                var connection = new NewConnectionToConnectionMapper().Map(model);
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

        [HttpPost]
        public async Task<IActionResult> Connect(string connectionId, string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(connectionId))
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid Connection ID" } });

                var connection = await _connectionRepository.GetConnectionByID(connectionId);
                if (connection == null)
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid Connection ID" } });

                // connecting to the remote server root
                _fTPClient = new SFTPClient(connection.IPAddress, connection.Port.Value, username, password);
                List<string> files = _fTPClient.GetFiles("").ToList();

                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("Password", password);

                return StatusCode(200, new { success = true, errors = new List<string> { }, files = files });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, errors = new List<string> { ex.Message } });
            }
        }

        /// <summary>
        /// connects to the remote server to fetch the files in a specific path, considering that the session has not been ended yet
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="path">file path without the domain or ip and port</param>
        /// <returns></returns>
        [HttpPost]
        [SessionFilter]
        public async Task<IActionResult> NavigateRemote(string connectionId, string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(connectionId))
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid Connection ID" } });

                var connection = await _connectionRepository.GetConnectionByID(connectionId);
                if (connection == null)
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid Connection ID" } });

                // connecting to the remote server root
                _fTPClient = new SFTPClient(connection.IPAddress, connection.Port.Value, HttpContext.Session.GetString("Username"), HttpContext.Session.GetString("Password"));
                List<string> files = _fTPClient.GetFiles(path).ToList();

                return StatusCode(200, new { success = true, errors = new List<string> { }, remoteFiles = files });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, errors = new List<string> { ex.Message } });
            }
        }

        [HttpPost]
        [SessionFilter]
        public async Task<IActionResult> NavigateLocal(string connectionId, string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(connectionId))
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid Connection ID" } });

                var connection = await _connectionRepository.GetConnectionByID(connectionId);
                if (connection == null)
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid Connection ID" } });

                // read the entires in the local file system
                string[] files = Directory.GetFiles(path);

                return StatusCode(200, new { success = true, errors = new List<string> { }, localFiles = files });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, errors = new List<string> { ex.Message } });
            }
        }

        [HttpPost]
        [SessionFilter]
        public async Task<IActionResult> DownloadFile(string connectionId, string remoteServerPath, string localFilePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(connectionId))
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid Connection ID" } });

                var connection = await _connectionRepository.GetConnectionByID(connectionId);
                if (connection == null)
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid Connection ID" } });

                // connecting to the remote server root
                _fTPClient = new SFTPClient(connection.IPAddress, connection.Port.Value, HttpContext.Session.GetString("Username"), HttpContext.Session.GetString("Password"));
                byte[] data = _fTPClient.DownloadFile(remoteServerPath);

                System.IO.File.WriteAllBytes(localFilePath, data);

                // read the file system to get the latest updates

                return StatusCode(200, new { success = true, errors = new List<string> { }, localFiles = Directory.GetDirectories(Path.GetDirectoryName(localFilePath)) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, errors = new List<string> { ex.Message } });
            }
        }

        [HttpPost]
        [SessionFilter]
        public async Task<IActionResult> UploadFile(string connectionId, string remoteServerPath, string localFilePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(connectionId))
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid Connection ID" } });

                var connection = await _connectionRepository.GetConnectionByID(connectionId);
                if (connection == null)
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid Connection ID" } });

                // connecting to the remote server root
                _fTPClient = new SFTPClient(connection.IPAddress, connection.Port.Value, HttpContext.Session.GetString("Username"), HttpContext.Session.GetString("Password"));

                byte[] data = System.IO.File.ReadAllBytes(localFilePath);
                _fTPClient.UploadFile(remoteServerPath, data);

                System.IO.File.WriteAllBytes(localFilePath, data);

                // get the files and dirs from the remote server
                string[] remoteFiles = _fTPClient.GetFiles(Path.GetDirectoryName(remoteServerPath)).ToArray();

                return StatusCode(200, new { success = true, errors = new List<string> { }, remoteFiles });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, errors = new List<string> { ex.Message } });
            }
        }
    }
}
