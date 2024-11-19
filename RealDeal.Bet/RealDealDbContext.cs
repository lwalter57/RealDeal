using Microsoft.EntityFrameworkCore;
using RealDeal.Models;
namespace RealDeal;

public class RealDealDbContext : DbContext
{
	public RealDealDbContext(DbContextOptions<RealDealDbContext> options) : base(options)
	{

    }

    public DbSet<Bett> Bets { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bett>().HasKey(b => b.Id);
		base.OnModelCreating(modelBuilder);
	}
}
