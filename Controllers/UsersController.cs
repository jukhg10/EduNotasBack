// File: Controllers/UsersController.cs
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RolesApi.Data;
using RolesApi.Dtos;
using RolesApi.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace RolesApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>Devuelve todos los usuarios.</summary>
        [HttpGet]
        [SwaggerResponse(200, "OK", typeof(UserResponseDto[]))]
        public async Task<IActionResult> GetAll()
        {
            var users = await _db.Users
                .Select(u => new UserResponseDto
                {
                    UserId = u.UserId,
                    FirstName = u.FirstName,
                    SecondName = u.SecondName,
                    FirstLastName = u.FirstLastName,
                    SecondLastName = u.SecondLastName,
                    Email = u.Email
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>Devuelve un usuario por ID.</summary>
        [HttpGet("{id:int}")]
        [SwaggerResponse(200, "OK", typeof(UserResponseDto))]
        [SwaggerResponse(404, "No encontrado")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado." });

            var dto = new UserResponseDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                SecondName = user.SecondName,
                FirstLastName = user.FirstLastName,
                SecondLastName = user.SecondLastName,
                Email = user.Email
            };
            return Ok(dto);
        }

        /// <summary>Actualiza datos de un usuario.</summary>
        [HttpPut("{id:int}")]
        [SwaggerResponse(200, "OK", typeof(UserResponseDto))]
        [SwaggerResponse(400, "Datos inválidos")]
        [SwaggerResponse(404, "No encontrado")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado." });

            user.FirstName = dto.FirstName;
            user.SecondName = dto.SecondName;
            user.FirstLastName = dto.FirstLastName;
            user.SecondLastName = dto.SecondLastName;
            user.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = _hasher.HashPassword(user, dto.Password);

            _db.Users.Update(user);
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
            return Ok(response);
        }

        /// <summary>Elimina un usuario por ID.</summary>
        [HttpDelete("{id:int}")]
        [SwaggerResponse(204, "No Content")]
        [SwaggerResponse(404, "No encontrado")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado." });

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
