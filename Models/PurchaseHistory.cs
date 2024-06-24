using Microsoft.AspNetCore.Identity;

namespace JamesCrafts.Models
{
    //public class PurchaseHistory
    //{
    //    public int PurchaseHistoryId { get; set; }
    //    public string UserId { get; set; }
    //    public DateTime PurchaseDate { get; set; }
    //    // Add more properties as needed, e.g., products, total amount, etc.
    //}

    public class PurchaseHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
        public DateTime PurchaseDate { get; set; }
        public bool IsConfirmed { get; set; }

        // Navigation property to User
        public IdentityUser User { get; set; }
    }


}
