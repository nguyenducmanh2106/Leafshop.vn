using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Models.Models.DataModels
{
    public partial class MobileShopContext : DbContext
    {
        public MobileShopContext()
            : base("name=MobileShopContext")
        {
        }

        public virtual DbSet<C__MigrationHistory> C__MigrationHistory { get; set; }
        public virtual DbSet<AddToCart> AddToCarts { get; set; }
        public virtual DbSet<Attribute> Attributes { get; set; }
        public virtual DbSet<Banner> Banners { get; set; }
        public virtual DbSet<Business> Businesses { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Feedback> Feedbacks { get; set; }
        public virtual DbSet<GroupRole> GroupRoles { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<News> News { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Provider> Providers { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<TypeAttr> TypeAttrs { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Attribute>()
                .HasMany(e => e.Products)
                .WithMany(e => e.Attributes)
                .Map(m => m.ToTable("ProductAttrs").MapLeftKey("AttrId").MapRightKey("ProductId"));

            modelBuilder.Entity<Category>()
                .HasMany(e => e.Categories1)
                .WithOptional(e => e.Category1)
                .HasForeignKey(e => e.ParentId);

            modelBuilder.Entity<Contact>()
                .Property(e => e.Sdt)
                .IsUnicode(false);

            modelBuilder.Entity<Feedback>()
                .Property(e => e.Sdt)
                .IsUnicode(false);

            modelBuilder.Entity<Product>()
                .HasMany(e => e.AddToCarts)
                .WithOptional(e => e.Product)
                .HasForeignKey(e => e.product_ProductId);
        }
    }
}
