using Cofinoy.Data.Models;
using Cofinoy.Services.ServiceModels;
using static Cofinoy.Resources.Constants.Enums;

namespace Cofinoy.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUser(string email, string password, ref User user);
        void AddUser(UserViewModel model);
    }
}
