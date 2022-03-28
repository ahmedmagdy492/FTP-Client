using FTP_Client.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FTP_Client.Repository
{
    public interface IConnectionRepository
    {
        Task<Connection> CreateConnection(Connection connection);
        Task<bool> DeleteConnection(Connection connection);
        Task<List<Connection>> GetConnectionsByUserID(long? userId);
    }
}