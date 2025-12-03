using CarRental_2;
using CarRental_2.Controllers;
using CarRental_2.Models;
using CarRental_2.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[TestFixture]
public class ClientsControllerTests
{
    private ClientsController _controller;
    private Mock<CarRental_2Context> _mockContext;
    private Mock<ILogger<ClientsController>> _mockLogger;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<CarRental_2Context>();
        _mockLogger = new Mock<ILogger<ClientsController>>();

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

        _controller = new ClientsController(_mockContext.Object, _mockLogger.Object)
        {
            ControllerContext = controllerContext
        };
    }

    [Test]
    public async Task Create_ValidModel_Returns_RedirectToAction()
    {
        // Arrange
        var client = new Client { FullName = "Alice", PhoneNumber = "123456" };
        _controller.ModelState.Clear(); // очищаем состояние модели

        // Act
        var result = await _controller.Create(client);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        _mockContext.Verify(x => x.Add(It.IsAny<Client>()), Times.Once);
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