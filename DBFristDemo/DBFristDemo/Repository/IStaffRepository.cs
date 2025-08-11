using DBFristDemo.Models;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBFristDemo.Repository
{
    public interface IStaffRepository
    {
        Task<Employee> GetByIdAsync(int id, bool includeNavigation = false);

        Task<Employee> GetManagerByIdAsync(int id, bool includeNavigation = false);

        Task<List<Employee>> GetAllAsync(bool includeNavigation = false);

        Task<bool> IsManagerAsync(int id);

        Task<bool> HasManagerAsync(int id);

        // Relationship

        Task<List<Employee>> GetEmployeesByDepartmentIdAsync(int departmentId, bool includeNavigation = false);
        
        Task<List<Employee>> GetEmployeesByManagerIdAsync(int managerId, bool includeNavigation = false);

        Task<List<Dependent>> GetDependentsByEmployeeIdAsync(int employeeId, bool includeNavigation = false);


        // Write 
        Task<Employee> AddAsync(Employee employee);

        Task<Employee> UpdateAsync(Employee employee);

        Task<bool> DeleteAsync(int id);

        Task<bool> SaveChangesAsync();


    }
}
