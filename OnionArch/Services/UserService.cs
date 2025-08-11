using Entities;
using Entities.User;

namespace Application
{
    public class UserService
    {
        private readonly IRepository _repository;
        public UserService(IRepository repository)
        {
            _repository = repository;
        }
       
        public void AddUser(Users user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }
            _repository.AddUser(user);
        }

        public List<Users> GetAllUsers()
        {

            return _repository.GetAllUsers().ToList();
        }

        public Users? GetUserById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "ID must be greater than zero");
            }
            return _repository.GetUserById(id);
        }
    }
}
