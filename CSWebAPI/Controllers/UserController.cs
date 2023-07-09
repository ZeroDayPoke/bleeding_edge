// Controllers/UserController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: api/User
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // GET: api/User/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    // POST: api/User
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] User user)
    {
        if (user.Username == null || user.PasswordHash == null)
        {
            return BadRequest();
        }

        var newUser = await _userService.CreateUserAsync(user);
        if (newUser == null)
        {
            return BadRequest();
        }
        return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
    }

    // PUT: api/User/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] User user)
    {
        await _userService.UpdateUserAsync(user);
        return Ok(user);
    }

    // DELETE: api/User/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }

    // PUT: api/User/5/ChangePassword
    [HttpPut("{id}/ChangePassword")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordModel changePasswordModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Ensure the user is changing their own password
        if (User.Identity == null || User.Identity.Name == null || User.Identity.Name != id.ToString())
        {
            Console.WriteLine($"User.Identity.Name: {User.Identity.Name}");
            return Forbid();
        }

        var result = await _userService.ChangeUserPasswordAsync(id, changePasswordModel);
        if (!result)
        {
            return BadRequest("Invalid old password or user not found.");
        }

        return NoContent();
    }

    // POST: api/User/Login
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        if (loginModel.Username == null || loginModel.Password == null)
        {
            return BadRequest();
        }

        var token = await _userService.AuthenticateAsync(loginModel.Username, loginModel.Password);
        if (token == null)
        {
            return Unauthorized();
        }
        return Ok(new { token });
    }
}
