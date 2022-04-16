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
using System.Text.RegularExpressions;
using FTP_Client.HelperModels;
using Renci.SshNet.Sftp;

namespace FTP_Client.Controllers
{
    [Authorize]
    public class ConnectionController : Controller
    {
        private IFTPClient _fTPClient;
        private readonly IConnectionRepository _connectionRepository;
        private readonly IViewRenderService _viewRenderService;
        private readonly IConfigReader _configReader;

        public ConnectionController(IConnectionRepository connectionRepository, IViewRenderService viewRenderService, IConfigReader configReader)
        {
            _connectionRepository = connectionRepository;
            this._viewRenderService = viewRenderService;
            this._configReader = configReader;
        }

        public IActionResult Test()
        {
            return StatusCode(200, new { data = "Test Works" });
        }

        public IActionResult Index(string connectionID, string ip, int port)
        {
            ViewBag.ConnectionID = connectionID;
            ViewBag.Ip = ip;
            ViewBag.Port = port;
            ViewBag.MyIP = new IPHelper().GetMyIPAddress();
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

                if (string.IsNullOrWhiteSpace(username))
                    username = string.Empty;

                if (string.IsNullOrWhiteSpace(password))
                    password = string.Empty;

                // connecting to the remote server root
                _fTPClient = new SFTPClient(connection.IPAddress, connection.Port.Value, username, password);
                List<SftpFile> files = _fTPClient.GetFiles("").ToList();

                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("Password", password);

                return StatusCode(200, new { success = true, errors = new List<string> { }, connectionId, ip = connection.IPAddress, port = connection.Port });
            }
            catch (ArgumentException)
            {
                return BadRequest(new { success = false, errors = new List<string> { "You need to enter a username and a password" } });
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
                List<SftpFile> files = _fTPClient.GetFiles(path).ToList();

                ViewBag.ConnectionId = connectionId;
                TempData["ConnectionID"] = connectionId;
                return StatusCode(200, new { success = true, errors = new List<string> { }, remoteFiles = _viewRenderService.RenderToString("Views/Shared/Connections/_RemoteFileList.cshtml", files) });
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

                System.IO.File.WriteAllBytes(Path.Combine(localFilePath, Path.GetFileName(remoteServerPath)), data);

                // read the file system to get the latest updates

                return StatusCode(200, new { success = true, errors = new List<string> { }, localFiles = _viewRenderService.RenderToString("Views/Shared/Connections/_LocalFileList.cshtml", Directory.GetFileSystemEntries(localFilePath).ToList()) });
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
                
                if(Directory.Exists(localFilePath))
                {
                    // directory transfer
                    return BadRequest(new { success = false, errors = new List<string> { "Directory Transfer is not yet supported" } });
                }

                // connecting to the remote server root
                _fTPClient = new SFTPClient(connection.IPAddress, connection.Port.Value, HttpContext.Session.GetString("Username"), HttpContext.Session.GetString("Password"));

                byte[] data = System.IO.File.ReadAllBytes(localFilePath);
                _fTPClient.UploadFile($"{remoteServerPath}/{Path.GetFileName(localFilePath)}", data);

                //System.IO.File.WriteAllBytes(localFilePath, data);

                // get the files and dirs from the remote server
                SftpFile[] remoteFiles = null;
                if (remoteServerPath != "\\")
                    remoteFiles = _fTPClient.GetFiles(Path.GetDirectoryName(remoteServerPath)).ToArray();
                else
                    remoteFiles = _fTPClient.GetFiles(remoteServerPath ?? string.Empty).ToArray();

                return StatusCode(200, new { success = true, errors = new List<string> { }, remotefiles = _viewRenderService.RenderToString("Views/Shared/Connections/_RemoteFileList.cshtml", remoteFiles.ToList()) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, errors = new List<string> { ex.Message } });
            }
        }

        #region Local Actions
        [HttpPost]
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
                // # ERROR
                List<string> files = new List<string>();
                if (string.IsNullOrWhiteSpace(path))
                {
                    files = Directory.GetLogicalDrives().ToList();
                }
                else
                {
                    files = Directory.GetFileSystemEntries(path).ToList();
                }

                return StatusCode(200, new { success = true, errors = new List<string> { }, localFiles = _viewRenderService.RenderToString("Views/Shared/Connections/_LocalFileList.cshtml", files), path });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, errors = new List<string> { ex.Message } });
            }
        }

        [HttpPost]
        public IActionResult BackLocal(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || Regex.IsMatch(path, @"^[A-Za-z]{1,2}:\\$"))
                {
                    var drives = Directory.GetLogicalDrives().ToList();
                    return StatusCode(200, new { success = true, errors = new List<string> { }, files = _viewRenderService.RenderToString("Views/Shared/Connections/_LocalFileList.cshtml", drives), currentPath = path });
                }

                char delimiter = '\0';
                if (OperatingSystem.IsLinux())
                    delimiter = '/';
                else
                    delimiter = '\\';

                string[] dirs = path.Split(delimiter);
                if (string.IsNullOrWhiteSpace(dirs[dirs.Length - 1]))
                {
                    // if the last element is a slash
                    // removing the last 2 elments of the array
                    dirs = dirs.Take(dirs.Length - 2).ToArray();
                    path = string.Join(delimiter, dirs);
                    path += delimiter;
                    var files = Directory.GetFileSystemEntries(path).ToList();
                    return StatusCode(200, new { success = true, errors = new List<string> { }, files = _viewRenderService.RenderToString("Views/Shared/Connections/_LocalFileList.cshtml", files), currentPath = path });
                }

                dirs = dirs.Take(dirs.Length - 1).ToArray();
                path = string.Join(delimiter, dirs);
                path += delimiter;
                return StatusCode(200, new { success = true, errors = new List<string> { }, files = _viewRenderService.RenderToString("Views/Shared/Connections/_LocalFileList.cshtml", Directory.GetFileSystemEntries(path).ToList()), currentPath = path });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, errors = new List<string> { ex.Message } });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> BackRemote(string conId, string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(conId))
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid Connection ID" } });

                var connection = await _connectionRepository.GetConnectionByID(conId);
                if (connection == null)
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid Connection ID" } });

                _fTPClient = new SFTPClient(connection.IPAddress, connection.Port.Value, HttpContext.Session.GetString("Username"), HttpContext.Session.GetString("Password"));
                char delimiter = '/';

                string[] dirs = path.Split(delimiter);
                if (string.IsNullOrWhiteSpace(dirs[dirs.Length - 1]))
                {
                    // if the last element is a slash
                    // removing the last 2 elments of the array
                    dirs = dirs.Take(dirs.Length - 2).ToArray();
                    path = string.Join(delimiter, dirs);
                    path += delimiter;
                    var files = _fTPClient.GetFiles(path).ToList();
                    return StatusCode(200, new { success = true, errors = new List<string> { }, files = _viewRenderService.RenderToString("Views/Shared/Connections/_RemoteFileList.cshtml", files), currentPath = path });
                }

                dirs = dirs.Take(dirs.Length - 1).ToArray();
                path = string.Join(delimiter, dirs);
                path += delimiter;
                return StatusCode(200, new { success = true, errors = new List<string> { }, files = _viewRenderService.RenderToString("Views/Shared/Connections/_RemoteFileList.cshtml", _fTPClient.GetFiles(path).ToList()), currentPath = path });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, errors = new List<string> { ex.Message } });
            }
        }

        [HttpPost]
        public IActionResult ViewContent(string path)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(Path.GetExtension(path)))
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid file Name" } });

                var fileContent = System.IO.File.ReadAllBytes(path);

                return StatusCode(200, new { success = true, errors = new List<string>{ }, fileView = _viewRenderService.RenderToString("Views/Shared/Connections/_ViewContent.cshtml", new FileContent { Content = fileContent, FileName = Path.GetFileName(path), FullPath = path }) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, errors = new List<string> { ex.Message } });
            }
        }

        [HttpPost]
        public IActionResult SaveFile(string path, string content)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(path) || content == null)
                    return BadRequest(new { success = false, errors = new List<string> { "Invalid file" } });

                System.IO.File.WriteAllText(path, content);
                return StatusCode(200, new { success = true, errors = new List<string> { } });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, errors = new List<string> { ex.Message } });
            }
        }
        #endregion
    }
}
