using PortalOgloszeniowy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;
using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Identity;

namespace PortalOgloszeniowy.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Post> Posts { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

    }
}
