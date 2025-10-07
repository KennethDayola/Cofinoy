using Cofinoy.Data.Interfaces;
using Cofinoy.Data.Models;
using Cofinoy.Services.Interfaces;
using Cofinoy.Services.Manager;
using Cofinoy.Services.ServiceModels;
using AutoMapper;
using System;
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

        public void AddUser(UserViewModel model)
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
            return _repository.GetUsers().FirstOrDefault(x => x.Email == email);
        }

        public void UpdateUser(User user)
        {
            var existingUser = _repository.GetUsers().FirstOrDefault(u => u.Id == user.Id);
            if (existingUser == null)
                throw new InvalidOperationException("User not found.");

            // Update only properties we allow
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Nickname = user.Nickname;
            existingUser.BirthDate = user.BirthDate;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Email = user.Email;
            existingUser.Country = user.Country;
            existingUser.City = user.City;
            existingUser.postalCode = user.postalCode;

            _repository.UpdateUser(existingUser); // Implement this in repository
        }

        public bool EmailExists(string email)
        {
            return _repository.GetUsers().Any(u => u.Email == email);
        }


    }
}
