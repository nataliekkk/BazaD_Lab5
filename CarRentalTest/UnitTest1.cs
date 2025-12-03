using CarRental_2;
using CarRental_2.Controllers;
using CarRental_2.Data;
using CarRental_2.Models;
using CarRental_2.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarRentalTest
{
    [TestFixture]
    public class CarClassControllerTests
    {
        private CarsClassController _controller;
        private Mock<CarRental_2Context> _mockContext;
        private Mock<ILogger<CarsClassController>> _mockLogger;
        private Mock<IConfiguration> _mockConfiguration;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<CarRental_2Context>();
            _mockLogger = new Mock<ILogger<CarsClassController>>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Настройка HttpContext
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "User")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = user };
            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _controller = new CarsClassController(_mockContext.Object, _mockLogger.Object, _mockConfiguration.Object)
            {
                ControllerContext = controllerContext
            };
        }

        private Mock<DbSet<T>> MockDbSet<T>(IQueryable<T> source) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(source.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(source.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(source.GetEnumerator());

            return mockSet;
        }

        [Test]
        public async Task Create_ValidModel_Returns_RedirectToAction()
        {
            // Arrange
            var cars = new List<CarClass>
            {
                new CarClass { Name = "Class A", Description = "Description" }
            }.AsQueryable();

            var mockCarClasses = MockDbSet(cars);
            _mockContext.Setup(x => x.CarClasses).Returns(mockCarClasses.Object);

            _controller.ModelState.Clear(); // очищаем состояние модели

            // Act
            var result = await _controller.Create(new CarClass { Name = "Class A", Description = "Description" });

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            _mockContext.Verify(x => x.Add(It.IsAny<CarClass>()), Times.Once);
        }


        [Test]
        public async Task Edit_Returns_NotFound_When_Id_Is_Null()
        {
            // Act
            var result = await _controller.Edit(null);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }
    }
    

}