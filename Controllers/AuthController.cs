using DocumentSearch.Helpers;
using DocumentSearch.Services.Interfaces;
using DocumentSearch.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentSearch.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IEmployeeRegistration _employeeRegistration;
    private readonly IEmployeeLogin _employeeLogin;

    public AuthController(
        IEmployeeRegistration employeeRegistration,
        IEmployeeLogin employeeLogin)
    {
        _employeeRegistration = employeeRegistration;
        _employeeLogin = employeeLogin;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterEmployee model)
    {
        var result = await _employeeRegistration.RegisterAsync(model);
        return ApiHelper.ToActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginEmployee model)
    {
        var result = await _employeeLogin.LoginAsync(model);
        return ApiHelper.ToActionResult(result);
    }
}
