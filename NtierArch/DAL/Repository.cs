using Models;


namespace DAL
{
    public class Repository
    {
        private readonly List<Users> _users;
        public Repository()
        {
            _users = new List<Users>();
        }
        public void AddUser(Users user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
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
