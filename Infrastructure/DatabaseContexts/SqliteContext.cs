// Ignore Spelling: Sqlite

using Core.Identity;
using Core.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseContexts;
public class SqliteContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public SqliteContext(DbContextOptions<SqliteContext> options) : base(options)
    {

    }
    public SqliteContext()
    {

    }

    public DbSet<ToDo> ToDoes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite(@"Data Source = E:\development\c#\AuthenticationExample\Database\Authentication_Example.sqlite");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ToDo>().ToTable("todos");
    }
}
