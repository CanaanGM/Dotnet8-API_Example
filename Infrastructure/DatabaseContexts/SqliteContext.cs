using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseContexts;
public class SqliteContext : DbContext
{
    public SqliteContext(DbContextOptions<SqliteContext> options) : base(options)
    {

    }
    public SqliteContext()
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite(@"E:\development\c#\AuthenticationExample\Database\Authentication_Example.sqlite");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
