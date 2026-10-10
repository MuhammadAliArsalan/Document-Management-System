using DocumentSearch.Helpers;
using DocumentSearch.Models;
using DocumentSearch.Repository;
using DocumentSearch.Services.Interfaces;
using DocumentSearch.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace DocumentSearch.Services.Implementationss;

public class EmployeeLogin : IEmployeeLogin
{
    private readonly IAuthServiceRepository _authRepository;
    private readonly IPasswordHasher<Employee> _passwordHasher;
    private readonly ITokenService _tokenService;

    public EmployeeLogin(
        IAuthServiceRepository authRepository,
        IPasswordHasher<Employee> passwordHasher,
        ITokenService tokenService)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult<LoginEmployeeResponse>> LoginAsync(LoginEmployee model)
    {
        var employee = await _authRepository.GetByCodeAsync(model.EmployeeCode.Trim());
        if (employee == null)
            return ServiceResult<LoginEmployeeResponse>.Unauthorized("Invalid credentials.");

        var verification = _passwordHasher.VerifyHashedPassword(
            employee,
            employee.PasswordHash,
            model.Password);
        if (verification == PasswordVerificationResult.Failed)
            return ServiceResult<LoginEmployeeResponse>.Unauthorized("Invalid credentials.");

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            employee.PasswordHash = _passwordHasher.HashPassword(employee, model.Password);
            await _authRepository.UpdateAsync(employee);
        }

        var roles = employee.IsAdmin ? new[] { "Admin" } : new[] { "Employee" };
        var response = new LoginEmployeeResponse
        {
            Token = _tokenService.GenerateToken(
                employee.Id.ToString(),
                employee.EmployeeCode,
                roles)
        };

        return ServiceResult<LoginEmployeeResponse>.Success(response, "Login successful.");
    }
}
