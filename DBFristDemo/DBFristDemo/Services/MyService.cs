using DBFristDemo.Models;
using DBFristDemo.Repository;

namespace DBFristDemo.Services
{
    public class MyService : IMyService
    {
        private readonly IStaffRepository _staffRepository;

        public MyService(IStaffRepository staffRepository)
        {
            _staffRepository = staffRepository;
        }

        public async Task<List<Employee>> GetAllAsync(bool includeNavigation = false)
        {
            var employees = await _staffRepository.GetAllAsync(includeNavigation);

            return employees;
        }

        public async Task<Employee> GetByIdAsync(int id, bool includeNavigation = false)
        {
            var employee = await _staffRepository.GetByIdAsync(id, includeNavigation);
            return employee;
        }

        public async Task<List<Dependent>> GetDependentsByEmployeeIdAsync(int employeeId, bool includeNavigation = false)
        {
            var dependents = await _staffRepository.GetDependentsByEmployeeIdAsync(employeeId, includeNavigation);
            return dependents;
        }

        public Task<List<Employee>> GetEmployeesByDepartmentIdAsync(int departmentId, bool includeNavigation = false)
        {
            var employees = _staffRepository.GetEmployeesByDepartmentIdAsync(departmentId, includeNavigation);
            return employees;

        }

        public Task<List<Employee>> GetEmployeesByManagerIdAsync(int managerId, bool includeNavigation = false)
        {
            var employees = _staffRepository.GetEmployeesByManagerIdAsync(managerId, includeNavigation);
            return employees;
        }

        public Task<Employee> GetManagerByIdAsync(int id, bool includeNavigation = false)
        {
            var manager = _staffRepository.GetManagerByIdAsync(id, includeNavigation);
            return manager;
        }
    }
}