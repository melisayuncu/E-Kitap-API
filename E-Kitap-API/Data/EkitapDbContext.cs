using Microsoft.EntityFrameworkCore;
using E_Kitap_API.Models;

namespace E_Kitap_API.Data
{
	public class EkitapDbContext : DbContext
	{
		public EkitapDbContext(DbContextOptions<EkitapDbContext> options) : base(options) { }

		public DbSet<Book> Books => Set<Book>();
		public DbSet<Submission> Submissions => Set<Submission>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Book>()
				.HasMany(b => b.Submissions)
				.WithOne(s => s.Book)
				.HasForeignKey(s => s.BookId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Book>()
				.Property(b => b.Status)
				.HasConversion<string>(); //Store the enum as readable text in the database.

			base.OnModelCreating(modelBuilder);
		}
	}
}