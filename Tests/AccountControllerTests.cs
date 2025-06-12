using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using db_query_v1._0._0._1.Controllers;
using db_query_v1._0._0._1.Models;
using db_query_v1._0._0._1.Services;
using Microsoft.AspNetCore.Authentication;
using NuGet.ContentModel;
using Xunit;

namespace db_query_v1._0._0._1.Tests;

public class AccountControllerTests
{
    private static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
    }

    private static Mock<SignInManager<ApplicationUser>> CreateSignInManager(Mock<UserManager<ApplicationUser>>? userManager = null)
    {
        userManager ??= CreateUserManager();
        return new Mock<SignInManager<ApplicationUser>>(userManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<ApplicationUser>>().Object);
    }
    private static AccountController CreateController(
        Mock<UserManager<ApplicationUser>>? userManager = null,
        Mock<SignInManager<ApplicationUser>>? signInManager = null,
        Mock<IPlanService>? planService = null,
        Mock<IUserService>? userService = null,
        ClaimsPrincipal? principal = null)
    {
        userManager ??= CreateUserManager();
        signInManager ??= CreateSignInManager(userManager);
        planService ??= new Mock<IPlanService>();
        userService ??= new Mock<IUserService>();
        var controller = new AccountController(userManager.Object, signInManager.Object, planService.Object, userService.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal ?? new ClaimsPrincipal(new ClaimsIdentity()) }
        };
        return controller;
    }

    [Fact]
    public void Register_Get_ReturnsView()
    {
        var controller = CreateController();
        var result = controller.Register();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Register_Post_Valid_RedirectsToChat()
    {
        var um = CreateUserManager();
        var sm = CreateSignInManager(um);
        um.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
          .ReturnsAsync(IdentityResult.Success);
        sm.Setup(m => m.SignInAsync(It.IsAny<ApplicationUser>(), false, null))
          .Returns(Task.CompletedTask);
        var controller = CreateController(um, sm);

        var model = new RegisterViewModel
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "Password1!",
            ConfirmPassword = "Password1!",
            Specializations = new List<string> { "Spec" },
            Address = "Addr"
        };

        var result = await controller.Register(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ChatWithData", redirect.ActionName);
        Assert.Equal("Chat", redirect.ControllerName);
    }

    [Fact]
    public async Task Register_Post_InvalidModel_ReturnsView()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("Email", "Required");
        var model = new RegisterViewModel();

        var result = await controller.Register(model);

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Login_Get_ReturnsView()
    {
        var controller = CreateController();
        var result = controller.Login();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Login_Post_UserNotFound_ReturnsView()
    {
        var um = CreateUserManager();
        um.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        var controller = CreateController(um);
        var model = new LoginViewModel { Email = "missing@example.com", Password = "pw" };

        var result = await controller.Login(model);

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Logout_Post_RedirectsToHome()
    {
        var um = CreateUserManager();
        var sm = CreateSignInManager(um);
        sm.Setup(m => m.SignOutAsync()).Returns(Task.CompletedTask);
        var controller = CreateController(um, sm);

        var result = await controller.Logout();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }
}
