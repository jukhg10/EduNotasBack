using System.ComponentModel.DataAnnotations;    // ← add this
namespace RolesApi.Models
{
    public class UserRole
    {
        [Key]                                   // ← decorate the PK
        public int Id { get; set; }

        public int UserId { get; set; }
        public int RoleId { get; set; }
    }
}
