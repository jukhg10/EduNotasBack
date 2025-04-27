using System.ComponentModel.DataAnnotations;
namespace RolesApi.Dtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}