using CarRental_2.Controllers;
using CarRental_2.Data;
using CarRental_2.Models;
using CarRental_2.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarRental_2.Tests.Controllers
{
    [TestFixture]
    public class MaintenancesControllerTests
    {
        private CarRental_2Context _context;
        private MaintenancesController _controller;
        private Mock<ILogger<MaintenancesController>> _mockLogger;
        private Mock<IConfiguration> _mockConfig;

        [SetUp]
        public async Task SetUp()
        {
            var options = new DbContextOptionsBuilder<CarRental_2Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CarRental_2Context(options);
            await _context.Database.EnsureCreatedAsync();

            _mockLogger = new Mock<ILogger<MaintenancesController>>();
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["Parameters:PageSize"]).Returns("10");

            _controller = new MaintenancesController(_context, _mockLogger.Object, _mockConfig.Object);

           
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user")
            }, "TestAuth"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [TearDown]
        public async Task TearDown()
        {
            _controller?.Dispose();
            await _context.Database.EnsureDeletedAsync();
            _context?.Dispose();
        }

        private async Task SeedTestData(int count)
        {
           
            for (int i = 1; i <= count; i++)
            {
                _context.Cars.Add(new Car
                {
                    Brand = $"Brand{i}",
                    Model = $"Model{i}",
                    LicensePlate = $"LP{i:000}",
                    Status = "Available"  
                });
            }
            await _context.SaveChangesAsync();  

           
            for (int i = 1; i <= count; i++)
            {
                decimal cost = i <= 2 ? 100m : 200m;
                _context.Maintenances.Add(new Maintenance
                {
                    
                    CarId = i,  
                    MaintenanceDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-i)),
                    Description = $"Desc{i}",
                    Cost = cost
                });
            }
            await _context.SaveChangesAsync();
        }

        [Test]
        public async Task Index_NotAuthenticated_RedirectsToHome()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext.HttpContext.User = anonymousUser;

            var result = await _controller.Index(0m) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public void Create_Get_ReturnsView()
        {
            // Act
            var result = _controller.Create() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.Null);
            Assert.That(result.Model, Is.Null);
        }


        [Test]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithErrors()
        {
            _context.Cars.Add(new Car { Brand = "Test", Model = "Test", LicensePlate = "LP001", Status = "Available" });
            await _context.SaveChangesAsync();

            var maintenance = new MaintenanceEditCreateViewModel
            {
                CarId = 1,
                MaintenanceDate = DateOnly.FromDateTime(DateTime.Now),
                Description = "Test",
                Cost = 100m
            };
            _controller.ModelState.AddModelError("Description", "Error"); // Добавляем ошибку в состояние модели

            var result = await _controller.Create(maintenance) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.SameAs(maintenance)); // Проверяем, что возвращаемая модель та же
            Assert.That(await _context.Maintenances.CountAsync(), Is.EqualTo(0)); // Не должно быть добавлений
        }

        [Test]
        public async Task Create_Post_ValidModelState_RedirectsToIndex()
        {
            // Arrange - создаем тестовые данные
            _context.Cars.Add(new Car { Brand = "Test", Model = "Test", LicensePlate = "LP001", Status = "Available" });
            await _context.SaveChangesAsync();

            var maintenance = new MaintenanceEditCreateViewModel
            {
                CarId = 1,
                MaintenanceDate = DateOnly.FromDateTime(DateTime.Now),
                Description = "Test",
                Cost = 100m
            };

            // Act
            var result = await _controller.Create(maintenance) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo(nameof(MaintenancesController.Index))); // Проверяем, что перенаправление на Index
            Assert.That(await _context.Maintenances.CountAsync(), Is.EqualTo(1)); // Проверяем, что запись добавлена
        }
        
        [Test]
        public async Task Edit_Get_NullId_ReturnsNotFound()
        {
            var result = await _controller.Edit(null) as NotFoundResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Get_NonExistentId_ReturnsNotFound()
        {
            var result = await _controller.Edit(999) as NotFoundResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Get_ValidId_ReturnsViewWithModel()
        {
            // Arrange
            await SeedTestData(1);

            // Act
            var result = await _controller.Edit(1) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Post_IdMismatch_ReturnsNotFound()
        {
            var maintenance = new MaintenanceEditCreateViewModel
            {
                Id = 1,
                CarId = 1,
                MaintenanceDate = DateOnly.FromDateTime(DateTime.Now),
                Description = "Updated",
                Cost = 150m
            };

            var result = await _controller.Edit(999, maintenance) as NotFoundResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Delete_Get_NullId_ReturnsNotFound()
        {
            var result = await _controller.Delete(null) as NotFoundResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Delete_Post_NonExistentId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Delete(999) as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Delete_Post_ValidId_RemovesItemAndRedirectsToIndex()
        {
            // Arrange
            await SeedTestData(1); // Убедитесь, что есть запись с Id = 1

            // Act
            var result = await _controller.Delete(1) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo(nameof(MaintenancesController.Index))); // Проверяем перенаправление
            Assert.That(await _context.Maintenances.CountAsync(), Is.EqualTo(0)); // Проверяем, что запись была удалена
        }


        [Test]
        public async Task DeleteConfirmed_ValidId_DeletesAndRedirects()
        {
            await SeedTestData(1);
            var result = await _controller.Delete(1) as RedirectToActionResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo(nameof(MaintenancesController.Index)));
            Assert.That(await _context.Maintenances.CountAsync(), Is.EqualTo(0));
        }
    }
}