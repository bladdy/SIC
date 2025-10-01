namespace SIC.Frontend.Services
{
    public interface ILoginService
    {
        Task LoginAsync(string token);

        Task LogOutAsync();
    }
}