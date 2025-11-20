using Cofinoy.Data.Models;
using Cofinoy.Services.ServiceModels;
using static Cofinoy.Resources.Constants.Enums;

namespace Cofinoy.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUser(string email, string password, ref User user);
        void AddUser(UserServiceModel model);
        User GetUserByEmail(string email);
        void UpdateUser(User user);
        bool UserExists(string email);

        ProfileDetailsServiceModel GetProfileDetails(string email);
        UpdatePersonalInfoResult UpdatePersonalInfo(string currentEmail, PersonalInfoServiceModel model);
        UpdateAddressResult UpdateAddress(string email, AddressServiceModel model);
        ChangePasswordResult ChangePassword(string email, ChangePasswordServiceModel model);

    }
}
