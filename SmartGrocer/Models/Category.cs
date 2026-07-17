using System.ComponentModel.DataAnnotations;

namespace SmartGrocer.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public int ShopId { get; set; }
        public Shop Shop { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();

        public bool IsDeleted { get; set; } = false;
    }
}
