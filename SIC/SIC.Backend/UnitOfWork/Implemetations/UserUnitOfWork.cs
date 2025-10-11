using Microsoft.AspNetCore.Identity;
using SIC.Backend.Repositories.Implemetations;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.UnitOfWork.Implemetations
{
    public class UserUnitOfWork : IUserUnitOfWork
    {
        private readonly IUserRepository _userRepository;

        public UserUnitOfWork(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IdentityResult> AddUserAsync(User user, string password) => await _userRepository.AddUserAsync(user, password);

        public async Task<IdentityResult> UpdateUserAsync(User user) => await _userRepository.UpdateUserAsync(user);

        public async Task AddUserToRoleAsync(User user, string roleName) => await _userRepository.AddUserToRoleAsync(user, roleName);

        public async Task CheckRoleAsync(string roleName) => await _userRepository.CheckRoleAsync(roleName);

        public async Task<User> GetUserAsync(string email) => await _userRepository.GetUserAsync(email);

        public async Task<User> GetUserByAsync(string id) => await _userRepository.GetUserByAsync(id);

        public async Task<bool> IsUserInRoleAsync(User user, string roleName) => await _userRepository.IsUserInRoleAsync(user, roleName);

        public async Task<SignInResult> LogInAsync(LoginDTO model) => await _userRepository.LogInAsync(model);

        public async Task LogOutAsync() => await _userRepository.LogOutAsync();
    }
}