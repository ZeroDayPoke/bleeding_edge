// SignUpTest.cs

using Microsoft.AspNetCore.Mvc;


public class SignUpTest
{
    [Fact]
    public async Task SignUp_ReturnsBadRequest_WhenModelIsInvalid()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var controller = new UserController(mockUserService.Object);
        controller.ModelState.AddModelError("error", "some error");

        // Act
        var result = await controller.SignUp(null);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }
}
