using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSo_WebBanMoHinh.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<FavoriteProduct> FavoriteProducts { get; set; }
        public DbSet<CompareProduct> CompareProducts { get; set; }
        public DbSet<Coupons> Coupons { get; set; }
        public DbSet<RedeemedCoupon> RedeemedCoupons { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<RedeemedCoupon>()
                .HasKey(rc => rc.Id);
            builder.Entity<RedeemedCoupon>()
                .HasOne(rc => rc.User) 
                .WithMany()            
                .HasForeignKey(rc => rc.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Coupons>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.RedeemedBy)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<Coupons>()
                .HasOne(c => c.Order)
                .WithMany(o => o.Coupons)
                .HasForeignKey(c => c.OrderId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
