using Microsoft.AspNetCore.Identity;

namespace Balosar.Server.Models;

public class ApplicationUser : IdentityUser
{
       public string FirstName { get; set; }
        public string LastName { get; set; }
}
