using DocumentSearch.Helpers;
using DocumentSearch.ViewModels;

namespace DocumentSearch.Services.Interfaces;

public interface IEmployeeRegistration
{
    Task<ServiceResult<RegisterEmployeeResponse>> RegisterAsync(RegisterEmployee model);
}
