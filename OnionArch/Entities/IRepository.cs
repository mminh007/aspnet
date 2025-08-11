using Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public interface IRepository
    {   
        Users? GetUserById(int id);
        IEnumerable<Users> GetAllUsers();
        void AddUser( Users user);

    }
}
