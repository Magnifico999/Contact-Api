using MainAssignment.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MainAssignment.Data
{
    public class ContactDbContext : IdentityDbContext<AppUser>
    {
        public ContactDbContext(DbContextOptions<ContactDbContext> options) : base(options)
        {
        }

        public DbSet<Contact> Contact { get; set; }
        public DbSet<AppUser> UserTable { get; set; }

    }

}
