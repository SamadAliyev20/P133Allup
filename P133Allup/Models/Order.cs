using System.ComponentModel.DataAnnotations;

namespace P133Allup.Models
{
    public class Order : BaseEntity
    {
        public int No { get; set; }

        public string? UserId { get; set; }

        public AppUser? User { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }
        [StringLength(100)]
        public string? State { get; set; }
        [StringLength(100)]
        public string? City { get; set; }
        [StringLength(100)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? AddressLine { get; set; }

        public IEnumerable<OrderItem>? OrderItems { get; set; }
    }
}
