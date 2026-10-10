using DocumentSearch.Helpers;
using DocumentSearch.Models;
using DocumentSearch.Repository;
using DocumentSearch.Services.Interfaces;
using DocumentSearch.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace DocumentSearch.Services.Implementationss;

public sealed class EmployeeRegistration : IEmployeeRegistration
{
    private readonly IAuthServiceRepository _authRepository;
    private readonly IPasswordHasher<Employee> _passwordHasher;

    public EmployeeRegistration(
        IAuthServiceRepository authRepository,
        IPasswordHasher<Employee> passwordHasher)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ServiceResult<RegisterEmployeeResponse>> RegisterAsync(
        RegisterEmployee model)
    {
        var employeeCode = model.EmployeeCode.Trim();
        if (await _authRepository.ExistsByCodeAsync(employeeCode))
        {
            return ServiceResult<RegisterEmployeeResponse>.Conflict(
                "This employee code already exists.");
        }

        var employee = new Employee
        {
            EmployeeCode = employeeCode,
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            IsAdmin = false
        };
        employee.PasswordHash = _passwordHasher.HashPassword(employee, model.Password);
        await _authRepository.AddAsync(employee);

        var response = new RegisterEmployeeResponse
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            IsAdmin = employee.IsAdmin
        };

        return ServiceResult<RegisterEmployeeResponse>.Created(
            response,
            "Employee registered successfully.");
    }
}
