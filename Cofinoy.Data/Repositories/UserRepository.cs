using Cofinoy.Data.Interfaces;
using Cofinoy.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofinoy.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {

        }

        public IQueryable<User> GetUsers()
        {
            return this.GetDbSet<User>();
        }

        public bool UserExists(string email)
        {
            return this.GetDbSet<User>().Any(x => x.Email == email);
        }

        public void AddUser(User user)
        {
            this.GetDbSet<User>().Add(user);
            UnitOfWork.SaveChanges();
        }

        public void UpdateUser(User user)
        {
            var dbUser = this.GetDbSet<User>().FirstOrDefault(u => u.Id == user.Id);
            if (dbUser != null)
            {
                dbUser.FirstName = user.FirstName;
                dbUser.LastName = user.LastName;
                dbUser.Nickname = user.Nickname;
                dbUser.BirthDate = user.BirthDate;
                dbUser.PhoneNumber = user.PhoneNumber;
                dbUser.Email = user.Email;
                dbUser.Country = user.Country;
                dbUser.City = user.City;
                dbUser.postalCode = user.postalCode;

                UnitOfWork.SaveChanges();
            }
        }


    }
}
