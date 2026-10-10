using DocumentSearch.Models;

namespace DocumentSearch.Repository
{
    public interface IAuthServiceRepository
    {
        // Registration
        Task<bool> ExistsByCodeAsync(string employeeCode);
        Task AddAsync(Employee employee);

        // Login
        Task<Employee?> GetByCodeAsync(string employeeCode);
        Task UpdateAsync(Employee employee);

    }
}
