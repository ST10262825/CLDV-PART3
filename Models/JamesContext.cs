using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JamesCrafts;
namespace JamesCrafts.Models
{
    public partial class JamesContext : IdentityDbContext<IdentityUser>
    {
        public JamesContext(DbContextOptions<JamesContext> options) : base(options)
        {
        }

        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; } // DbSet for CartItems
        public DbSet<PurchaseHistory> PurchaseHistories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("MyConnectionStringDev"));
              //  optionsBuilder.UseSqlServer(configuration.GetConnectionString("MyConnectionStringAZURE"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // This is important to call Identity configurations

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAFFD02610B");

                entity.Property(e => e.OrderId).HasColumnName("OrderID");
                entity.Property(e => e.OrderDate).HasColumnType("datetime");
                entity.Property(e => e.OrderStatus).HasMaxLength(50);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User).WithMany(p => p.Orders)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Orders__UserID__4D94879B");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D30CC0ABDC30");

                entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");
                entity.Property(e => e.OrderId).HasColumnName("OrderID");
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__OrderDeta__Order__5070F446");

                entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__OrderDeta__Produ__5165187F");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6ED69DB9485");

                entity.Property(e => e.ProductId).HasColumnName("ProductID");
                entity.Property(e => e.Availability).HasMaxLength(50);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(200)
                    .HasColumnName("ImageURL");
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACA3069970");

                entity.Property(e => e.UserId).HasColumnName("UserID");
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Password).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.UserRole).HasMaxLength(50);
            });

            modelBuilder.Entity<Cart>()
            .HasMany(c => c.CartItems)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId);

            modelBuilder.Entity<PurchaseHistory>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PurchaseHistories");

                entity.Property(e => e.Id)
                      .HasColumnName("Id")
                      .HasColumnType("int")
                      .UseIdentityColumn();

                entity.Property(e => e.UserId)
                      .HasColumnName("UserId")
                      .HasColumnType("nvarchar(max)")
                      .IsRequired();

                entity.Property(e => e.ProductName)
                      .HasColumnName("ProductName")
                      .HasColumnType("nvarchar(max)")
                      .IsRequired();

                entity.Property(e => e.ProductDescription)
                      .HasColumnName("ProductDescription")
                      .HasColumnType("nvarchar(max)")
                      .IsRequired();

                entity.Property(e => e.ProductPrice)
                      .HasColumnName("ProductPrice")
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.Property(e => e.Quantity)
                      .HasColumnName("Quantity")
                      .HasColumnType("int")
                      .IsRequired();

                entity.Property(e => e.PurchaseDate)
                      .HasColumnName("PurchaseDate")
                      .HasColumnType("datetime2")
                      .IsRequired();

                entity.Property(e => e.IsConfirmed)
                      .HasColumnName("IsConfirmed")
                      .HasColumnType("bit")
                      .IsRequired();

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
