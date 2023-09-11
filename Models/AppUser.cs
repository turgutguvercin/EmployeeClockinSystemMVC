using Microsoft.AspNetCore.Identity;

namespace EmployeeClockinSystem.Models
{
    public class AppUser : IdentityUser
    {
        public virtual Employee? Employee { get; set; }
        
    }
}