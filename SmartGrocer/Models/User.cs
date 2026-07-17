using System.ComponentModel.DataAnnotations;

namespace SmartGrocer.Models
{
    public enum UserRole
    {
        Admin,
        Owner,
        Employee
    }

    public enum UserStatus
    {
        Pending,
        Approved,
        Rejected,
        Blocked
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public UserRole Role { get; set; }

        public UserStatus Status { get; set; } = UserStatus.Pending;

        public int? ShopId { get; set; }
        public Shop? Shop { get; set; }
    }
}
