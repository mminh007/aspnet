using DBFristDemo.Database;
using DBFristDemo.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBFristDemo.Repository
{
    public class StaffRepository : IStaffRepository
    {

        private readonly MyDbContext _context;

        public StaffRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<Employee> AddAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
            return employee;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _context.Employees
                .Where(e => e.EmployeeId == id)
                .ExecuteDeleteAsync();
            return true;
        }

        public async Task<List<Employee>> GetAllAsync(bool includeNavigation = false)
        {
            
            if (includeNavigation)
            {
                return await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Job)
                    .Include(e => e.Manager)
                    .ToListAsync();
            }
            else
            {
                return await _context.Employees.ToListAsync();
            }

        }

        public async Task<Employee> GetByIdAsync(int id, bool includeNavigation = false)
        {
            IQueryable<Employee> query = _context.Employees.AsNoTracking();

            if (includeNavigation)
            {
                query = query
                    .Include(e => e.Department)
                    .Include(e => e.Job)
                    .Include(e => e.Manager);
                
            }

            var emp = await query
                .Where(e => e.EmployeeId == id)
                .FirstOrDefaultAsync();

            if (emp == null)
                {
                throw new KeyNotFoundException($"Employee with ID {id} not found.");
            }
            return emp;
        }

        public async Task<List<Dependent>> GetDependentsByEmployeeIdAsync(int employeeId, bool includeNavigation = false)
        {
            
            if (includeNavigation)
            {
                return await _context.Dependents
                    .Include(d => d.Employee)
                    .Where(d => d.EmployeeId == employeeId)
                    .ToListAsync();
            }
            else
            {
                return await _context.Dependents
                    .Where(d => d.EmployeeId == employeeId)
                    .ToListAsync();
            }
        }

        public async Task<List<Employee>> GetEmployeesByDepartmentIdAsync(int departmentId, bool includeNavigation = false)
        {
            if (includeNavigation)
            {
                return await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Job)
                    .Include(e => e.Manager)
                    .Where(e => e.DepartmentId == departmentId)
                    .ToListAsync();
            }
            else
            {
                return await _context.Employees
                    .Where(e => e.DepartmentId == departmentId)
                    .ToListAsync();
            }
        }

        public async Task<bool> IsManagerAsync(int id)
        {
            return await _context.Employees
                .AnyAsync(e => e.ManagerId == id);
        }

        public async Task<bool> HasManagerAsync(int id)
        {
            return await _context.Employees
                .AnyAsync(e => e.EmployeeId == id && e.ManagerId != null);
        }

        public async Task<bool> EmployeeExistsAsync(int id)
        {
             return await _context.Employees.AnyAsync(e => e.EmployeeId == id);

        }
            

        public async Task<List<Employee>> GetEmployeesByManagerIdAsync(int managerId, bool includeNavigation = false)
        {   

            if (!await EmployeeExistsAsync(managerId))
            {
                Console.WriteLine();
                Console.WriteLine(new string('-', 50));
                Console.WriteLine();
                Console.WriteLine($"Employee with ID {managerId} not found.");
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', 50));
            Console.WriteLine();

            bool isManager = await IsManagerAsync(managerId);

            Console.WriteLine();
            Console.WriteLine(new string('-', 50));
            Console.WriteLine();

            if (!isManager)
            {
                Console.WriteLine($"Manager with ID {managerId} not found.");
            }

            Console.WriteLine();
            Console.WriteLine(new string('-', 50));
            Console.WriteLine();

            if (includeNavigation)
            {
                return await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Job)
                    .Include(e => e.Manager)
                    .Where(e => e.ManagerId == managerId)
                    .ToListAsync();
            }
            else
            {
                return await _context.Employees
                    .Where(e => e.ManagerId == managerId)
                    .ToListAsync();
            }
        }

        public async Task<Employee> GetManagerByIdAsync(int id, bool includeNavigation = false)
        {
            var manager = await GetByIdAsync(id, includeNavigation);
            return manager;
        }

        public async Task<bool> SaveChangesAsync()
        {
            
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Employee> UpdateAsync(Employee employee)
        {
            return await Task.Run(() =>
            {
                _context.Employees.Update(employee);
                return employee;
            });
        }
    }
}
