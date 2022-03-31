using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTP_Client.Helpers
{
    public class FTPClient : IFTPClient
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _url;

        public FTPClient(string url)
        {
            _username = string.Empty;
            _password = string.Empty;
            _url = url;
        }

        public FTPClient(string url, string username, string password) : this(url)
        {
            _username = username;
            _password = password;
        }

        public IEnumerable<string> GetFiles(string path)
        {
            FtpWebRequest directoryListRequest = (FtpWebRequest)WebRequest.Create(Path.Combine(_url, path));
            
            directoryListRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            directoryListRequest.Credentials = new NetworkCredential(_username, _password);

            using (FtpWebResponse response = (FtpWebResponse)directoryListRequest.GetResponse())
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    string responsePayload = sr.ReadToEnd();
                    string[] results = responsePayload.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    return results;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="path">the path including the filename</param>
        /// <param name="data">the contents of the file in bytes</param>
        /// <exception cref="Exception">throws Exception if the transfer was not successfull</exception>
        public void UploadFile(string path, byte[] data)
        {
            FtpWebRequest copyRequest = (FtpWebRequest)WebRequest.Create(Path.Combine(_url, path));

            copyRequest.Method = WebRequestMethods.Ftp.UploadFile;
            copyRequest.Credentials = new NetworkCredential(_username, _password);

            Stream reqStream = copyRequest.GetRequestStream();
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();

            using (FtpWebResponse response = (FtpWebResponse)copyRequest.GetResponse())
            {
                string statusCode = (Convert.ToInt16(response.StatusCode)).ToString();
                if (!statusCode.StartsWith("2"))
                    throw new Exception(response.StatusDescription);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="path">the path of the file to be downloaded including the filename</param>
        /// <returns>the contents of the file in bytes</returns>
        /// <exception cref="Exception">throws an Exception if the file is download operation was not successfull</exception>
        public byte[] DownloadFile(string path)
        {
            FtpWebRequest copyRequest = (FtpWebRequest)WebRequest.Create(Path.Combine(_url, path));

            copyRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            copyRequest.Credentials = new NetworkCredential(_username, _password);

            using (FtpWebResponse response = (FtpWebResponse)copyRequest.GetResponse())
            {
                byte[] data = new byte[response.GetResponseStream().Length];
                Stream resStream = response.GetResponseStream();
                resStream.Read(data, 0, data.Length);
                resStream.Close();

                string statusCode = (Convert.ToInt16(response.StatusCode)).ToString();
                if (!statusCode.StartsWith("2"))
                    throw new Exception(response.StatusDescription);

                return data;
            }
        }

    }
}
