﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using PizzaDeliveryWeb.Application.Services;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;

namespace PizzaDeliveryProject.Tests.ApplicationTests
{
    public class PizzaServiceTests
    {
        private readonly Mock<IPizzaRepository> _mockPizzaRepository;
        private readonly Mock<IIngredientRepository> _mockIngredientRepository;
        private readonly Mock<IPizzaSizeRepository> _mockPizzaSizeRepository;
        private readonly PizzaService _pizzaService;

        public PizzaServiceTests()
        {
            _mockPizzaRepository = new Mock<IPizzaRepository>();
            _mockIngredientRepository = new Mock<IIngredientRepository>();
            _mockPizzaSizeRepository = new Mock<IPizzaSizeRepository>();
            _pizzaService = new PizzaService(_mockPizzaRepository.Object, _mockIngredientRepository.Object,
                _mockPizzaSizeRepository.Object);
        }

        [Fact]
        public async Task GetPizzasAsync_ShouldReturnMappedPizzaDtos()
        {
            //// Arrange
            //var pizzas = new List<Pizza>
            //{
            //    new Pizza { Id = 1, Name = "Pizza 1", Description = "Description 1", },
            //    new Pizza { Id = 2, Name = "Pizza 2", Description = "Description 2" }
            //};

            //_mockPizzaRepository.Setup(repo => repo.GetPizzasAsync(0, 10, null)).ReturnsAsync(pizzas);

            //// Act
            //var result = await _pizzaService.GetPizzasAsync();

            //// Assert
            //Assert.Equal(2, result.Count());
            //Assert.Equal("Pizza 1", result.First().Name);


            var pizzas = new List<Pizza>
    {
        new Pizza { Id = 1, Name = "Pizza 1", Description = "Description 1" },
        new Pizza { Id = 2, Name = "Pizza 2", Description = "Description 2" }
    };

            var pizzaSizes = new List<PizzaSize>
    {
        new PizzaSize { Id = 1, Name = "Small", Price = 10 },
        new PizzaSize { Id = 2, Name = "Medium", Price = 15 }
    };

            _mockPizzaRepository
                .Setup(repo => repo.GetPizzasAsync(0, 10, true))
                .ReturnsAsync(pizzas);

            _mockPizzaSizeRepository
                .Setup(repo => repo.GetPizzaSizesAsync())
                .ReturnsAsync(pizzaSizes);

            // Act
            var result = await _pizzaService.GetPizzasAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.Name == "Pizza 1");
            Assert.Contains(result, p => p.Name == "Pizza 2");


        }
    }
}
