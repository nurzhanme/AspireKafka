using AspireKafka.Domain;
using Microsoft.EntityFrameworkCore;

namespace AspireKafka.Infrastructure;

public class DataContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;

}