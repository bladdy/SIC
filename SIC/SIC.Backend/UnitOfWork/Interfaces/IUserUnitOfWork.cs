using Microsoft.AspNetCore.Identity;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.UnitOfWork.Interfaces
{
    public interface IUserUnitOfWork
    {
        Task<User> GetUserAsync(string email);

        Task<User> GetUserByAsync(string id);

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task<IdentityResult> UpdateUserAsync(User user);

        Task CheckRoleAsync(string roleName);

        Task AddUserToRoleAsync(User user, string roleName);

        Task<bool> IsUserInRoleAsync(User user, string roleName);

        Task<SignInResult> LogInAsync(LoginDTO model);

        Task LogOutAsync();
    }
}