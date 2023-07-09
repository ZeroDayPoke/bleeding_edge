using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Xunit;

public class UserControllerTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;

    public UserControllerTests(WebApplicationFactory<Startup> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Put_EndpointUpdatesUser()
    {
        // Arrange
        var client = _factory.CreateClient();
        var testUserId = 1;
        var testUser = new User
        {
            Username = "Lok'Tar",
            Email = "fluffy@meow.purrr",
            PasswordHash = "$2a$11$c6B.9vMahtL7Vh0IhcCzBe6N8i3.JU0PQNOZ2hARRRgaRgLNTJmje"
        };
        var content = new StringContent(JsonConvert.SerializeObject(testUser), Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));

        // Act
        var response = await client.PutAsync($"/api/User/{testUserId}", content);

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
