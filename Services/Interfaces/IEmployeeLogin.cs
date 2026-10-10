using DocumentSearch.Helpers;
using DocumentSearch.ViewModels;

namespace DocumentSearch.Services.Interfaces;

public interface IEmployeeLogin
{
    Task<ServiceResult<LoginEmployeeResponse>> LoginAsync(LoginEmployee model);
}
