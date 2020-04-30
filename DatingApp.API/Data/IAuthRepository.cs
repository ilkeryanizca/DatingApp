using System.Threading.Tasks;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public interface IAuthRepository
    {
        Task<User> Register(User pUser, string pPassword);
        Task<User> Login(string pUserName, string pPassword);
        Task<bool> UserExist(string pUserName);
    }
}