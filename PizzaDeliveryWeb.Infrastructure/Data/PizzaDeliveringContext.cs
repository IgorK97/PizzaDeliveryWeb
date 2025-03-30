using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PizzaDeliveryWeb.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace PizzaDeliveryWeb.Infrastructure.Data
{
    public partial class PizzaDeliveringContext : IdentityDbContext<User>
    {
        //public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Pizza> Pizzas { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<DelStatus> DelStatuses { get; set; }
        public DbSet<PizzaSize> PizzaSizes { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Review> Reviews { get; set; }

        public PizzaDeliveringContext(DbContextOptions<PizzaDeliveringContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().HasIndex(u => u.PhoneNumber);
            builder.Entity<User>().HasIndex(u => u.Email);

            builder.Entity<Pizza>()
                .HasMany(p => p.Ingredients)
                .WithMany(i => i.Pizzas)
                .UsingEntity(j => j.ToTable("PizzaIngredients"));

            builder.Entity<Pizza>().Property(p => p.Image).IsRequired();

            builder.Entity<Pizza>().Property(p => p.Id).ValueGeneratedOnAdd();

            builder.Entity<Ingredient>().Property(i => i.Image).IsRequired();

            builder.Entity<OrderLine>()
                .HasMany(ol => ol.Ingredients)
                .WithMany(i => i.OrderLines)
                .UsingEntity(j => j.ToTable("OrderLineIngredients"));

            builder.Entity<OrderLine>()
                .HasOne(ol => ol.Order)
                .WithMany(o => o.OrderLines)
                .HasForeignKey(ol => ol.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderLine>()
                .HasOne(ol => ol.Pizza)
                .WithMany(p => p.OrderLines)
                .HasForeignKey(ol => ol.PizzaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderLine>()
                .HasOne(ol => ol.PizzaSize)
                .WithMany(ps => ps.OrderLines)
                .HasForeignKey(ol => ol.PizzaSizeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(t => t.Client)
                .WithMany(c=>c.Orders)
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(t => t.Manager)
                .WithMany()
                .HasForeignKey(t => t.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.DelStatus)
                .WithMany(ds => ds.Orders)
                .HasForeignKey(o => o.DelStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasIndex(o => o.OrderTime);

            builder.Entity<Delivery>()
                .HasOne(d => d.Order)
                .WithMany(o => o.Deliveries)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Delivery>()
                .HasOne(d => d.Courier)
                .WithMany(c => c.Deliveries)
                .HasForeignKey(d => d.CourierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Delivery>()
                .HasIndex(d => d.DeliveryTime);

            builder.Entity<Review>()
                .HasOne(r => r.Order)
                .WithMany(o => o.Reviews)
                .HasForeignKey(r => r.OrderId);

            builder.Entity<Review>()
                .HasOne(r => r.Client)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.ClientId);
        }

        
    }
}
