using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Infrastructure.Data;
using Xunit;

namespace PizzaDelivery.Tests.InfrastructureTests
{
    public class PizzaDeliveringContextTests
    {
        private readonly DbContextOptions<PizzaDeliveringContext> _options;

        public PizzaDeliveringContextTests()
        {
            _options = new DbContextOptionsBuilder<PizzaDeliveringContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
        }

        [Fact]
        public async Task AddPizzaAsync_ShouldSavePizza()
        {
            using (var context = new PizzaDeliveringContext(_options))
            {
                var pizza = new Pizza { Name = "Test Pizza", Description = "Test Description", Image="testimage.jpg" };
                context.Pizzas.Add(pizza);
                await context.SaveChangesAsync();
            }

            using (var context = new PizzaDeliveringContext(_options))
            {
                var pizza = await context.Pizzas.FirstOrDefaultAsync(p => p.Name == "Test Pizza");
                Assert.NotNull(pizza);
                Assert.Equal("Test Description", pizza.Description);
            }
        }
    }
}
