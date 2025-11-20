using Cofinoy.Data.Interfaces;
using Cofinoy.Data.Models;
using Cofinoy.Services.Interfaces;
using Cofinoy.Services.Manager;
using Cofinoy.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Cofinoy.Resources.Constants.Enums;

namespace Cofinoy.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public LoginResult AuthenticateUser(string email, string password, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _repository.GetUsers().Where(x => x.Email == email &&
                                                     x.Password == passwordKey).FirstOrDefault();
            return user != null ? LoginResult.Success : LoginResult.Failed;
        }

        public void AddUser(UserServiceModel model)
        {
            var user = new User();
            if (!_repository.UserExists(model.Email))
            {
                _mapper.Map(model, user);
                user.Password = PasswordManager.EncryptPassword(model.Password);
                _repository.AddUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
        }

        public User GetUserByEmail(string email)
        {
            return _repository.GetUsers().FirstOrDefault(u => u.Email == email);
        }

        public void UpdateUser(User user)
        {
            _repository.UpdateUser(user);
        }

        public bool UserExists(string email)
        {
            return _repository.UserExists(email);
        }

        public ProfileDetailsServiceModel GetProfileDetails(string email)
        {
            var user = _repository.GetUsers().FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return null;
            }

            return new ProfileDetailsServiceModel
            {
                User = user
            };
        }

        public UpdatePersonalInfoResult UpdatePersonalInfo(string currentEmail, ServiceModels.PersonalInfoServiceModel model)
        {
            var user = _repository.GetUsers().FirstOrDefault(u => u.Email == currentEmail);

            if (user == null)
            {
                return new UpdatePersonalInfoResult
                {
                    Success = false,
                    Message = "User not found.",
                    Errors = new Dictionary<string, string[]>()
                };
            }

            bool emailChanged = false;

            // Check if email is being updated
            if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
            {
                if (_repository.UserExists(model.Email))
                {
                    return new UpdatePersonalInfoResult
                    {
                        Success = false,
                        Errors = new Dictionary<string, string[]>
                        {
                            { "Email", new[] { "This email address is already in use." } }
                        }
                    };
                }

                user.Email = model.Email;
                emailChanged = true;
            }

            // Update all fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Nickname = model.Nickname;
            user.BirthDate = model.BirthDate.HasValue ? DateOnly.FromDateTime(model.BirthDate.Value) : default(DateOnly);
            user.PhoneNumber = model.PhoneNumber;

            _repository.UpdateUser(user);

            return new UpdatePersonalInfoResult
            {
                Success = true,
                Message = "Personal info updated successfully.",
                EmailChanged = emailChanged,
                UpdatedUser = user
            };
        }

        public UpdateAddressResult UpdateAddress(string email, ServiceModels.AddressServiceModel model)
        {
            var user = _repository.GetUsers().FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return new UpdateAddressResult
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            // Update all fields (allow nulls/empty)
            user.Country = model.Country;
            user.City = model.City;
            user.postalCode = model.PostalCode;

            _repository.UpdateUser(user);

            return new UpdateAddressResult
            {
                Success = true,
                Message = "Address updated successfully."
            };
        }

        public ChangePasswordResult ChangePassword(string email, ServiceModels.ChangePasswordServiceModel model)
        {
            var result = new ChangePasswordResult();

            // Get user from repository
            var user = _repository.GetUsers().FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                result.Success = false;
                result.Errors.Add("CurrentPassword", new[] { "User not found." });
                return result;
            }

            // Verify current password
            if (!PasswordManager.VerifyPassword(model.CurrentPassword, user.Password))
            {
                result.Success = false;
                result.Errors.Add("CurrentPassword", new[] { "Current password is incorrect." });
                return result;
            }

            // Check if new password is same as current password
            if (PasswordManager.VerifyPassword(model.NewPassword, user.Password))
            {
                result.Success = false;
                result.Errors.Add("NewPassword", new[] { "New password cannot be the same as the current password." });
                return result;
            }

            // Check if new password matches confirm password
            if (model.NewPassword != model.ConfirmPassword)
            {
                result.Success = false;
                result.Errors.Add("ConfirmPassword", new[] { "Passwords do not match." });
                return result;
            }

            // Validate password length
            if (model.NewPassword.Length < 8)
            {
                result.Success = false;
                result.Errors.Add("NewPassword", new[] { "Password must be at least 8 characters long." });
                return result;
            }

            // Validate password strength (uppercase, lowercase, special character)
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.NewPassword, @"^(?=.{8,}$)(?=.*[a-z])(?=.*[A-Z])(?=.*\W).*$"))
            {
                result.Success = false;
                result.Errors.Add("NewPassword", new[] { "Password must include uppercase, lowercase, and special characters." });
                return result;
            }

            // Update password
            user.Password = PasswordManager.EncryptPassword(model.NewPassword);
            _repository.UpdateUser(user);

            result.Success = true;
            result.Message = "Password changed successfully.";
            return result;
        }


    }
}