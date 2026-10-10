using System.Linq;
using System.Threading.Tasks;
using DocumentSearch.Data;
using DocumentSearch.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentSearch.Repository.Implementations
{
    public class AuthServiceRepository : IAuthServiceRepository
    {
        private readonly ApplicationDbContext _context;
        public AuthServiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByCodeAsync(string employeeCode)
        {
            return await _context.Employees
                .AnyAsync(e => e.EmployeeCode == employeeCode);
        }

        public async Task AddAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task<Employee?> GetByCodeAsync(string employeeCode)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
        }

        public async Task UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }
    }
}
