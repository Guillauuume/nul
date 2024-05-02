using API_REST_ONLINE.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> users { get; set; }
    public DbSet<Role> roles { get; set; }
    public DbSet<Success> success { get; set; }
    public DbSet<Rank> rank { get; set; }
    public DbSet<PendingInvite> pendinginvite { get; set; }
    public DbSet<Friendship> friendship { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public ApplicationDbContext()
    {
    }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    optionsBuilder.UseSqlServer("DefaultConnection");
    //}
}

