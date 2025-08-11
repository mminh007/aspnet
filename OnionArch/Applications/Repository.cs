using Entities;
using Entities.User;
using System;

namespace Infrastructure
{
    public class Repository : IRepository
    {
        private readonly List<Users> _users = new();

        public void AddUser(Users user)
        {
            _users.Add(user);
        }

        public IEnumerable<Users> GetAllUsers()
        {
            return _users.AsReadOnly();
        }

        public Users? GetUserById(int id)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }



    }
}
