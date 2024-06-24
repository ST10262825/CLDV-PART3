using JamesCrafts.Models;
using JamesCrafts;
using Microsoft.AspNetCore.Identity;
using Humanizer;

namespace JamesCrafts.Models { 
public class CartItem
{
    public int CartItemId { get; set; }
    public int Quantity { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }

    //Foreign key to link to Cart
    public int CartId { get; set; }
    public Cart Cart { get; set; }  // Navigation property

    // Navigation property to link to Product

}
}