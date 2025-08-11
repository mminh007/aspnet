using DBFristDemo.Models;
using Microsoft.Identity.Client;

namespace DBFristDemo.Services
{
    public interface IMyService
    {
        Task<Employee> GetByIdAsync(int id, bool includeNavigation = false);

        Task<Employee> GetManagerByIdAsync(int id, bool includeNavigation = false);

        Task<List<Employee>> GetAllAsync(bool includeNavigation = false);

        Task<List<Employee>> GetEmployeesByDepartmentIdAsync(int departmentId, bool includeNavigation = false);

        Task<List<Employee>> GetEmployeesByManagerIdAsync(int managerId, bool includeNavigation = false);

        Task<List<Dependent>> GetDependentsByEmployeeIdAsync(int employeeId, bool includeNavigation = false);
    }
}