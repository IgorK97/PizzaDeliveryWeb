using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using PizzaDeliveryWeb.Application.DTOs;
using PizzaDeliveryWeb.Application.Services;
using PizzaDeliveryWeb.Domain.Interfaces;

namespace PizzaDeliveryProject.Tests.ApplicationTests
{
    public class IngredientServiceTests
    {
        private readonly Mock<IIngredientRepository> _mockIngrRepository;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IPizzaSizeRepository> _mockPizzaSizeRepository;
        private readonly Mock<IOrderLineRepository> _mockOrderLineRepository;
        private readonly IngredientService _ingrService;

        public IngredientServiceTests()
        {
            _mockIngrRepository = new Mock<IIngredientRepository>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockPizzaSizeRepository = new Mock<IPizzaSizeRepository>();
            _mockOrderLineRepository = new Mock<IOrderLineRepository>();

            _ingrService = new IngredientService(_mockIngrRepository.Object, _mockOrderRepository.Object,
                _mockPizzaSizeRepository.Object, _mockOrderLineRepository.Object);
        }

        [Fact]
        public async Task GetIngredientsAsync_ShouldReturnIngredients()
        {
            // Arrange
            var ingrs = new List<PizzaDeliveryWeb.Domain.Entities.Ingredient>
            {
                new PizzaDeliveryWeb.Domain.Entities.Ingredient { Id = 1, Name = "Ingr 1", Description = "Description 1", Small=10, Medium=20, Big=30, PricePerGram=5 },
                new PizzaDeliveryWeb.Domain.Entities.Ingredient { Id = 2, Name = "Ingr 2", Description = "Description 2", Small=15, Medium=30, Big=45, PricePerGram=10 }
            };

            _mockIngrRepository.Setup(repo => repo.GetIngredientsAsync()).ReturnsAsync(ingrs);

            // Act
            var result = await _ingrService.GetIngredientsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal("Ingr 1", result.First().Name);
        }

        [Fact]
        public async Task AddIngredientAsync_ShouldAddIngredient()
        {
            // Arrange
            var IngrDto = new CreateIngredientDto { Name = "New Ingredient", Image="", IsAvailable=true, Description = "New Description", Small = 10, Medium = 20, Big = 30, PricePerGram = 5 };

            // Act
            await _ingrService.AddIngredientAsync(IngrDto);

            // Assert
            _mockIngrRepository.Verify(repo => repo.AddIngredientAsync(It.Is<PizzaDeliveryWeb.Domain.Entities.Ingredient>(t => t.Name == "New Ingredient")), Times.Once);
        }
    }
}
