using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SIC.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IUserUnitOfWork _userUnitOfWork;
        private readonly IUserRepository _usersRepository;
        private readonly IConfiguration _configuration;

        public AccountsController(IUserUnitOfWork userUnitOfWork, IConfiguration configuration, IUserRepository usersRepository)
        {
            _userUnitOfWork = userUnitOfWork;
            _configuration = configuration;
            _usersRepository = usersRepository;
        }

        [HttpGet("UserById/{id}")]
        public async Task<IActionResult> GetAsync(string id)
        {
            return Ok(await _userUnitOfWork.GetUserByAsync(id));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAsync([FromQuery] PaginationDTO pagination)
        {
            var response = await _usersRepository.GetAsync(pagination);
            if (response.Success)
            {
                return Ok(response.Result);
            }
            return BadRequest();
        }

        [HttpGet("totalPages")]
        public async Task<IActionResult> GetPagesAsync([FromQuery] PaginationDTO pagination)
        {
            var action = await _usersRepository.GetTotalPagesAsync(pagination);
            if (action.Success)
            {
                return Ok(action.Result);
            }
            return BadRequest();
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserDTO model)
        {
            User user = model;
            var result = await _userUnitOfWork.AddUserAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userUnitOfWork.AddUserToRoleAsync(user, user.UserType.ToString());
                return Ok(BuildToken(user));
            }
            return BadRequest(result.Errors.FirstOrDefault());
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> PutAsync([FromBody] User model)
        {
            User user = model;
            var result = await _userUnitOfWork.UpdateUserAsync(user);
            if (result.Succeeded)
            {
                return Ok(BuildToken(user));
            }
            return BadRequest(result.Errors.FirstOrDefault());
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LogInAsync([FromBody] LoginDTO model)
        {
            var result = await _userUnitOfWork.LogInAsync(model);
            if (result.Succeeded)
            {
                var user = await _userUnitOfWork.GetUserAsync(model.Email);
                if (user == null) return BadRequest("Usuario no encontrado.");
                return Ok(BuildToken(user));
            }
            return BadRequest("Email o Contraseña incorrectos.");
        }

        private TokenDTO? BuildToken(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.FullName!),
                new Claim(ClaimTypes.Role, user.UserType.ToString()),
                new Claim("Document", user.Document),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim("Address", user.Address),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwtKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddDays(30);
            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
                );
            return new TokenDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }
}