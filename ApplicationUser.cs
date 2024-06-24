using JamesCrafts.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace JamesCrafts
{
   

    public class ApplicationUser : IdentityUser
    {
        public Cart Cart { get; set; }
    }

}
