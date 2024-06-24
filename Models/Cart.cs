using Microsoft.AspNetCore.Identity;

namespace JamesCrafts.Models
{
    public class Cart
    {
        public int CartId { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }  // Navigation property
        public ICollection<CartItem> CartItems { get; set; }
    }

}
