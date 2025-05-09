// File: Controllers/AuthController.cs
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using RolesApi.Data;
using RolesApi.Dtos;
using RolesApi.Models;

namespace RolesApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IConfiguration _cfg;

        public AuthController(
            AppDbContext db,
            IPasswordHasher<User> hasher,
            IConfiguration configuration)
        {
            _db = db;
            _hasher = hasher;
            _cfg = configuration;
        }

        /// <summary>Registra un nuevo usuario.</summary>
        [HttpPost("register")]
        [SwaggerResponse(201, "Creado", typeof(UserResponseDto))]
        [SwaggerResponse(400, "Datos inválidos")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "El correo ya está registrado." });

            var user = new User
            {
                FirstName = dto.FirstName,
                SecondName = dto.SecondName,
                FirstLastName = dto.FirstLastName,
                SecondLastName = dto.SecondLastName,
                Email = dto.Email
            };
            user.Password = _hasher.HashPassword(user, dto.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                SecondName = user.SecondName,
                FirstLastName = user.FirstLastName,
                SecondLastName = user.SecondLastName,
                Email = user.Email
            };

            return CreatedAtAction(nameof(UsersController.GetById),
                                   new { id = user.UserId },
                                   response);
        }

        /// <summary>Valida credenciales y devuelve JWT + cookie HttpOnly.</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Unauthorized(new { message = "Credenciales inválidas" });

            var pwCheck = _hasher.VerifyHashedPassword(user, user.Password, dto.Password);
            if (pwCheck != PasswordVerificationResult.Success)
                return Unauthorized(new { message = "Credenciales inválidas" });

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(double.Parse(_cfg["Jwt:ExpireHours"]!));

            var jwt = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(jwt);

            Response.Cookies.Append("AuthToken", tokenStr, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expires
            });

            return Ok(new LoginResponseDto
            {
                Token = tokenStr,
                Expires = expires
            });
        }
    }
}
