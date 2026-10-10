namespace DocumentSearch.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(string userId, string username, IEnumerable<string>? roles);
    }
}
