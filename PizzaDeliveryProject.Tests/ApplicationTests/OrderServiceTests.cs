using System;
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

    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<IOrderRepository> _mockOrderRepository = new();
        private readonly Mock<IDeliveryRepository> _mockDeliveryRepo = new Mock<IDeliveryRepository>();
        private readonly OrderService _orderService;
        public OrderServiceTests()
        {
            _mockUnitOfWork.Setup(u => u.Orders).Returns(_mockOrderRepository.Object);
            _mockUnitOfWork.Setup(u => u.Deliveries).Returns(_mockDeliveryRepo.Object);
            _orderService = new OrderService(_mockUnitOfWork.Object);
            
        }

        [Fact]
        public async Task AcceptOrderAsync_ShouldAcceptOrder_WhenValid()
        {
            // Arrange
            var orderId = 1;
            var managerId = "manager1";
            var order = new Order
            {
                Id = orderId,
                DelStatusId = (int)OrderStatusEnum.IsBeingFormed
            };

            _mockOrderRepository.Setup(r => r.GetOrderByIdAsync(orderId))
                .ReturnsAsync(order);

            _mockUnitOfWork.Setup(u => u.Save())
                .ReturnsAsync(1);

            // Act
            await _orderService.AcceptOrderAsync(orderId, managerId);

            // Assert
            Assert.Equal(managerId, order.ManagerId);
            Assert.Equal((int)OrderStatusEnum.IsBeingPrepared, order.DelStatusId);
            Assert.True(order.AcceptedTime <= DateTime.UtcNow);
        }

        [Fact]
        public async Task AcceptOrderAsync_ShouldThrowInvalidOperationException_WhenOrderNotInFormedState()
        {
            // Arrange
            var orderId = 1;
            var managerId = "manager1";
            var order = new Order
            {
                Id = orderId,
                DelStatusId = (int)OrderStatusEnum.NotPlaced
            };

            _mockOrderRepository.Setup(r => r.GetOrderByIdAsync(orderId))
                .ReturnsAsync(order);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _orderService.AcceptOrderAsync(orderId, managerId));

            Assert.Equal("Невозможно принять заказ в текущем статусе", ex.Message);
        }

        [Fact]
        public async Task TakeOrder_ShouldAssignDelivery_WhenOrderStatusIsBeingTransferred()
        {
            // Arrange
            var orderId = 1;
            var courierId = "courier1";
            var order = new Order
            {
                Id = orderId,
                DelStatusId = (int)OrderStatusEnum.IsBeingTransferred
            };

            _mockOrderRepository.Setup(r => r.GetOrderWithDeliveryAsync(orderId))
                .ReturnsAsync(order);

            _mockDeliveryRepo.Setup(r => r.GetDeliveryByOrderIdAsync(orderId))
                .ReturnsAsync((Delivery)null);

            _mockDeliveryRepo.Setup(r => r.AddDeliveryAsync(It.IsAny<Delivery>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(u => u.Save())
                .ReturnsAsync(1);

            // Act
            await _orderService.TakeOrder(orderId, courierId);

            // Assert
            Assert.Equal((int)OrderStatusEnum.HasBeenTransferred, order.DelStatusId);
            _mockDeliveryRepo.Verify(r => r.AddDeliveryAsync(It.Is<Delivery>(d =>
                d.OrderId == orderId && d.CourierId == courierId)), Times.Once);
        }

        [Fact]
        public async Task TakeOrder_ShouldThrowInvalidOperationException_WhenOrderStatusIsInvalid()
        {
            // Arrange
            var orderId = 1;
            var courierId = "courier1";
            var order = new Order
            {
                Id = orderId,
                DelStatusId = (int)OrderStatusEnum.IsBeingPrepared // неправильный статус
            };

            _mockOrderRepository.Setup(r => r.GetOrderWithDeliveryAsync(orderId))
                .ReturnsAsync(order);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _orderService.TakeOrder(orderId, courierId));

            Assert.Equal("Невозможно начать доставку", exception.Message);
        }



    }
}
