using Microsoft.AspNetCore.Mvc;
using RolesApi.Models;


namespace RolesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private static List<Role> roles = new();
        private static List<UserRole> userRoles = new();
        private static int nextRoleId = 1;

        [HttpPost]
        public IActionResult CreateRole([FromBody] Role role)
        {
            if (roles.Any(r => r.Name == role.Name))
                return BadRequest(new { message = "Role already exists." });

            role.Id = nextRoleId++;
            roles.Add(role);
            return Ok(role);
        }

        [HttpGet]
        public IActionResult GetRoles()
        {
            return Ok(roles);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateRole(int id, [FromBody] Role updatedRole)
        {
            var role = roles.FirstOrDefault(r => r.Id == id);
            if (role == null)
                return NotFound(new { message = "Role not found." });

            role.Name = updatedRole.Name;
            return Ok(role);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteRole(int id)
        {
            var role = roles.FirstOrDefault(r => r.Id == id);
            if (role == null)
                return NotFound(new { message = "Role not found." });

            roles.Remove(role);
            return NoContent();
        }

        [HttpPost("assign")]
        public IActionResult AssignRole([FromBody] UserRole userRole)
        {
            if (!roles.Any(r => r.Id == userRole.RoleId))
                return BadRequest(new { message = "Role does not exist." });

            userRoles.Add(userRole);
            return Ok(new { message = "Role assigned to user." });
        }

        [HttpGet("validate")]
        public IActionResult ValidateUserRole(int userId, string roleName)
        {
            var role = roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null)
                return NotFound(new { message = "Role not found." });

            var userHasRole = userRoles.Any(ur => ur.UserId == userId && ur.RoleId == role.Id);
            return Ok(new { hasRole = userHasRole });
        }
    }
}
