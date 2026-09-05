using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;
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
        private readonly IMailHelper _mailHelper;

        public AccountsController(IUserUnitOfWork userUnitOfWork, IConfiguration configuration, IUserRepository usersRepository, IMailHelper mailHelper)
        {
            _userUnitOfWork = userUnitOfWork;
            _configuration = configuration;
            _usersRepository = usersRepository;
            _mailHelper = mailHelper;
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

        [HttpPost("ChangePassword")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordDTO model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var response = await _userUnitOfWork.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
            if (response.Success)
            {
                return Ok();
            }
            return BadRequest(response.Message);
        }

        [HttpPost("RecoverPassword")]
        public async Task<IActionResult> RecoverPasswordAsync([FromBody] RecoverPasswordDTO model)
        {
            var user = await _userUnitOfWork.GetUserByPhoneAsync(model.PhoneNumber);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                return Ok(new ActionResponse<string> { Success = true, Message = "Si el número ingresado está registrado, recibirás un correo para restablecer tu contraseña." });
            }

            var tokenResponse = await _userUnitOfWork.GeneratePasswordResetTokenAsync(user);
            if (!tokenResponse.Success)
            {
                return BadRequest(tokenResponse.Message);
            }

            var baseUrl = _configuration["Mail:ResetUrlBase"] ?? "http://localhost:5124";
            var resetUrl = $"{baseUrl}/ResetPassword?phone={Uri.EscapeDataString(user.PhoneNumber!)}&token={Uri.EscapeDataString(tokenResponse.Result!)}";
            var subject = "Recuperación de contraseña — INVBOXV";
            var body = BuildResetPasswordEmail(user.FirstName, resetUrl);

            var mailResponse = await _mailHelper.SendMailGmailAsync(user.FullName, user.Email, subject, body);
            if (!mailResponse.Success)
            {
                return BadRequest(mailResponse.Message);
            }

            return Ok(new ActionResponse<string> { Success = true, Message = "Si el número ingresado está registrado, recibirás un correo para restablecer tu contraseña." });
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDTO model)
        {
            var user = await _userUnitOfWork.GetUserByPhoneAsync(model.PhoneNumber);
            if (user == null)
            {
                return BadRequest("El número de teléfono no está registrado.");
            }

            var response = await _userUnitOfWork.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }

            return Ok(new ActionResponse<string> { Success = true, Message = "Tu contraseña ha sido restablecida correctamente." });
        }

        private string BuildResetPasswordEmail(string firstName, string resetUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
</head>
<body style=""margin:0;padding:0;background-color:#F2EBD9;font-family:Arial,Helvetica,sans-serif;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#F2EBD9;padding:24px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;width:100%;background-color:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 12px rgba(0,0,0,0.08);"">
                    <tr>
                        <td style=""background-color:#3C6A79;padding:32px 24px;text-align:center;"">
                            <h1 style=""margin:0;color:#ffffff;font-size:24px;font-weight:bold;letter-spacing:1px;"">INVBOXV</h1>
                            <p style=""margin:8px 0 0;color:#CEC5B8;font-size:14px;"">Restablecer contraseña</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:32px 24px;"">
                            <p style=""color:#333333;font-size:16px;line-height:1.6;margin:0 0 16px;"">Hola <strong>{firstName}</strong>,</p>
                            <p style=""color:#555555;font-size:15px;line-height:1.6;margin:0 0 24px;"">Recibimos una solicitud para restablecer la contraseña de tu cuenta. Haz clic en el botón de abajo para elegir una nueva contraseña.</p>
                            <p style=""text-align:center;margin:0 0 24px;"">
                                <a href=""{resetUrl}"" style=""background-color:#3C6A79;color:#ffffff;text-decoration:none;padding:14px 28px;border-radius:8px;font-size:15px;font-weight:bold;display:inline-block;"">Restablecer contraseña</a>
                            </p>
                            <p style=""color:#777777;font-size:13px;line-height:1.5;margin:0 0 8px;"">Si el botón no funciona, copia y pega el siguiente enlace en tu navegador:</p>
                            <p style=""background-color:#F9F5EC;border:1px solid #E3D9C0;border-radius:8px;padding:12px;font-size:12px;word-break:break-all;color:#3C6A79;margin:0 0 20px;"">{resetUrl}</p>
                            <p style=""color:#999999;font-size:12px;line-height:1.5;margin:0;"">Este enlace es válido por 24 horas. Si no solicitaste el cambio, ignora este mensaje y tu contraseña no cambiará.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color:#F2EBD9;padding:16px 24px;text-align:center;"">
                            <p style=""color:#999999;font-size:12px;margin:0;"">&copy; {DateTime.UtcNow.Year} INVBOXV — Sistema de Invitación y Confirmación</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
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