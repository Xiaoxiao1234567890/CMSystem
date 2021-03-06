﻿using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace CMSystem.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public int Role { get; set; }
        public string Name { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            if ( !manager.IsInRole(this.Id, "Customer") && !manager.IsInRole(this.Id, "Member"))
            {
                manager.AddToRole(this.Id, "Customer");
                manager.AddClaim(this.Id, (new Claim("Role", "Customer")));
                manager.AddClaim(this.Id, (new Claim("Permission", "Attempt Tests")));
            }
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Announcement> Announcement { get; set; }

        public System.Data.Entity.DbSet<CMSystem.Models.Comment> Comment { get; set; }

        public System.Data.Entity.DbSet<CMSystem.Models.Event> Event { get; set; }

        public System.Data.Entity.DbSet<CMSystem.Models.EventSignUp> EventSignUp { get; set; }
    }
    
}