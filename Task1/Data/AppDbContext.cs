using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Task1.Models;

namespace Task1.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<StringData> StringDatas { get; set; }
        public DbSet<StringProperty> StringProperties { get; set; }
        public DbSet<CharacterItem> CharacterItems { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            /*builder.Entity<BaseModel>()
                .Property(p => p.Id)
                .HasColumnName("id")
              .UseIdentityByDefaultColumn();*/
            base.OnModelCreating(builder);

        }
    }
}
