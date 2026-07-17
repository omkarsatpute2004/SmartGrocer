using System.ComponentModel.DataAnnotations;

namespace SmartGrocer.Models
{
    public class Shop
    {
        [Key]
        public int ShopId { get; set; }

        [Required]
        [StringLength(100)]
        public string ShopName { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }

        public UserStatus Status { get; set; } = UserStatus.Pending;

        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<User> Employees { get; set; } = new List<User>();
    }
}
