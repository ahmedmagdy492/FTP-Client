using Renci.SshNet.Sftp;
using System.Collections.Generic;

namespace FTP_Client.Helpers
{
    public interface IFTPClient
    {
        byte[] DownloadFile(string path);
        IEnumerable<SftpFile> GetFiles(string path);
        void UploadFile(string path, byte[] data);
    }
}