using System.ComponentModel.DataAnnotations;  
namespace RolesApi.Dtos
{
	public class RegisterUserDto
	{
		[Required] public string FirstName { get; set; }
		public string? SecondName { get; set; }

		[Required] public string FirstLastName { get; set; }
		[Required] public string SecondLastName { get; set; }

		[Required, EmailAddress]
		public string Email { get; set; }

		[Required, MinLength(6)]
		public string Password { get; set; }
	}
}