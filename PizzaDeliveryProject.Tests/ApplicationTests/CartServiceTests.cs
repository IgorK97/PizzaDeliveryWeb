using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PizzaDeliveryWeb.Application.MyExceptions;
using PizzaDeliveryWeb.Application.Services;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaDeliveryWeb.Domain.Interfaces;

namespace PizzaDeliveryProject.Tests.ApplicationTests
{
    public class CartServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IOrderRepository> _mockOrderRepo;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockOrderRepo = new Mock<IOrderRepository>();

            _mockUnitOfWork.Setup(u => u.Orders).Returns(_mockOrderRepo.Object);

            _cartService = new CartService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task SubmitCartAsync_ShouldThrow_WhenPriceIsInvalid()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _cartService.SubmitCartAsync("client1", 0, "Some Address"));
        }

        [Fact]
        public async Task SubmitCartAsync_ShouldThrow_WhenAddressIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _cartService.SubmitCartAsync("client1", 100, ""));
        }

        [Fact]
        public async Task SubmitCartAsync_ShouldThrow_WhenCartNotFound()
        {
            _mockOrderRepo.Setup(r => r.GetCartAsync("client1"))
                .ReturnsAsync((Order)null);

            await Assert.ThrowsAsync<CartNotFoundException>(() =>
                _cartService.SubmitCartAsync("client1", 100, "Address"));
        }

        [Fact]
        public async Task SubmitCartAsync_ShouldThrow_WhenCartPriceOutdated()
        {
            var cart = new Order { Price = 200, OrderLines = new List<OrderLine> { new() } };

            _mockOrderRepo.Setup(r => r.GetCartAsync("client1")).ReturnsAsync(cart);

            await Assert.ThrowsAsync<OutdatedCartException>(() =>
                _cartService.SubmitCartAsync("client1", 100, "Address"));
        }

        [Fact]
        public async Task SubmitCartAsync_ShouldThrow_WhenCartIsEmpty()
        {
            var cart = new Order { Price = 100, OrderLines = new List<OrderLine>() };

            _mockOrderRepo.Setup(r => r.GetCartAsync("client1")).ReturnsAsync(cart);

            await Assert.ThrowsAsync<EmptyCartException>(() =>
                _cartService.SubmitCartAsync("client1", 100, "Address"));
        }

        [Fact]
        public async Task SubmitCartAsync_ShouldThrow_WhenDbFails()
        {
            var cart = new Order { Price = 100, OrderLines = new List<OrderLine> { new() } };

            _mockOrderRepo.Setup(r => r.GetCartAsync("client1")).ReturnsAsync(cart);
            _mockUnitOfWork.Setup(u => u.Save()).ThrowsAsync(new DbUpdateException());

            await Assert.ThrowsAsync<MyDbException>(() =>
                _cartService.SubmitCartAsync("client1", 100, "Address"));
        }

        [Fact]
        public async Task SubmitCartAsync_ShouldSucceed_WhenCartIsValid()
        {
            var cart = new Order { Price = 100, OrderLines = new List<OrderLine> { new() } };

            _mockOrderRepo.Setup(r => r.GetCartAsync("client1")).ReturnsAsync(cart);
            _mockUnitOfWork.Setup(u => u.Save()).ReturnsAsync(1);

            var exception = await Record.ExceptionAsync(() =>
                _cartService.SubmitCartAsync("client1", 100, "Valid Address"));

            Assert.Null(exception);
            Assert.Equal((int)OrderStatusEnum.IsBeingFormed, cart.DelStatusId);
            Assert.Equal("Valid Address", cart.Address);
            Assert.True(cart.OrderTime <= DateTime.UtcNow);
        }

        [Fact]
        public async Task GetOrCreateCartAsync_ShouldReturnExistingCart()
        {
            // Arrange
            var clientId = "user1";
            var existingCart = new Order { Id = 1, ClientId = clientId, DelStatusId = (int)OrderStatusEnum.NotPlaced };
            _mockOrderRepo.Setup(r => r.GetCartAsync(clientId))
                .ReturnsAsync(existingCart);
            _mockUnitOfWork.Setup(u => u.Orders).Returns(_mockOrderRepo.Object);

            var service = new CartService(_mockUnitOfWork.Object);

            // Act
            var result = await service.GetOrCreateCartAsync(clientId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(clientId, result.ClientId);
        }

        [Fact]
        public async Task GetOrCreateCartAsync_ShouldCreateCart_WhenNotFound()
        {
            // Arrange
            var clientId = "newUser";
            var createdCart = new Order { Id = 2, ClientId = clientId, DelStatusId = (int)OrderStatusEnum.NotPlaced };

            _mockOrderRepo.SetupSequence(r => r.GetCartAsync(clientId))
                .ReturnsAsync((Order)null) // first call - not found
                .ReturnsAsync(createdCart); // second call - found after create

            _mockOrderRepo.Setup(r => r.AddOrderAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(u => u.Save())
                .ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.Orders).Returns(_mockOrderRepo.Object);

            var service = new CartService(_mockUnitOfWork.Object);

            // Act
            var result = await service.GetOrCreateCartAsync(clientId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(clientId, result.ClientId);
        }



    }
}
