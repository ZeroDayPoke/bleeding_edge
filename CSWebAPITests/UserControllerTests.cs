using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net.Http.Headers;

public class UserControllerTests : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
{
    private readonly WebApplicationFactory<Startup> _factory;
    private HttpClient _client;
    private User? _testUser;

    public UserControllerTests(WebApplicationFactory<Startup> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _client = _factory.CreateClient();
        InitializeAsync().Wait();
    }

    public async Task InitializeAsync()
    {
        _testUser = new User
        {
            Username = "pawsoffury",
            Email = "testingsucks@lame.boo",
            PasswordHash = "saltyhash"
        };
        var content = new StringContent(JsonConvert.SerializeObject(_testUser), Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        var response = await _client.PostAsync("/api/User", content);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to create test user: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        _testUser = JsonConvert.DeserializeObject<User>(responseContent) ?? throw new Exception("Failed to deserialize test user.");
    }


    public async void Dispose()
    {
        if (_testUser != null)
        {
            var response = await _client.GetAsync($"/api/User/{_testUser.Id}");
            Console.WriteLine(response.Content);
            if (response.IsSuccessStatusCode)
            {
                response = await _client.DeleteAsync($"/api/User/{_testUser.Id}");
            }
            Console.WriteLine("NO USER NO MORE");
        }
    }

    [Fact]
    public async Task Post_EndpointCreatesUser()
    {
        // Arrange
        var client = _factory.CreateClient();
        var testUser = new User
        {
            Username = "tatanka",
            Email = "largeone@buffalo.tech",
            PasswordHash = "graze4life"
        };
        var content = new StringContent(JsonConvert.SerializeObject(testUser), Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));

        // Act
        var response = await client.PostAsync("/api/User", content);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetAll_EndpointReturnsUsers()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/User");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var users = JsonConvert.DeserializeObject<List<User>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(users);
        if (users != null)
        {
            Assert.True(users.Count > 0);
        }
    }

    [Fact]
    public async Task Get_EndpointReturnsUser()
    {
        // Arrange
        var client = _factory.CreateClient();
        var testUserId = 9001;

        // Act
        var response = await client.GetAsync("/api/User/9001");

        // Assert
        var user = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(user);
        if (user != null)
        {
            Assert.Equal(testUserId, user.Id);
        }
    }

    [Fact]
    public async Task Put_EndpointUpdatesUser()
    {
        // Arrange
        var client = _factory.CreateClient();
        var testUserId = 9001;
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

    [Fact]
    public async Task Delete_EndpointDeletesUser()
    {
        // Arrange
        var client = _factory.CreateClient();
        var testUserId = 9001;

        // Act
        var response = await client.DeleteAsync($"/api/User/{testUserId}");

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
