using DAL;
using Models;


namespace BLL
{
    public class Service
    {
        private readonly Repository _repository;

        public Service(Repository repository)
        {
            _repository = repository;
        }

        public List<Users> GetAllUsers()
        {
            return _repository.GetAllUsers().ToList();
        }

        public Users? GetUserById(int id)
        {
            return _repository.GetUserById(id);
        }

        public void AddUser(Users user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            _repository.AddUser(user);
        }







    }
}
