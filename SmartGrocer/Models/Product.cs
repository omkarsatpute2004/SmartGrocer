using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartGrocer.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int ShopId { get; set; }
        public Shop Shop { get; set; }

        public ICollection<Sale> Sales { get; set; } = new List<Sale>();

        public bool IsDeleted { get; set; } = false;
    }
}
