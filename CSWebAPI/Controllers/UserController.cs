// Controllers/UserController.cs
using Microsoft.AspNetCore.Mvc;

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
    [HttpGet("id/{id}")]
    public async Task<ActionResult<User>> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return user;
    }

    [HttpGet("username/{username}")]
    public async Task<ActionResult<User>> GetUserByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return NotFound();
        }
        return user;
    }

    // POST: api/User
    [HttpPost]
    public async Task<ActionResult<User>> Post(SignUpModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var newUser = await _userService.CreateUserAsync(new User
        {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = model.Password,
            VerificationToken = Guid.NewGuid().ToString()
        });

        if (newUser == null)
        {
            return BadRequest("An error occurred while creating the user.");
        }

        return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
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
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordModel changePasswordModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Ensure the user is changing their own password
        // if (User.Identity == null || User.Identity.Name == null || User.Identity.Name != id.ToString())
        // {
        //    return Forbid();
        // }

        var result = await _userService.ChangeUserPasswordAsync(id, changePasswordModel);
        if (!result)
        {
            return BadRequest("Invalid old password or user not found.");
        }

        return NoContent();
    }

    // POST: api/User/SignUp
    [HttpPost("SignUp")]
    public async Task<IActionResult> SignUp([FromBody] SignUpModel signUpModel)
    {
        if (signUpModel.Username == null || signUpModel.Password == null)
        {
            return BadRequest();
        }

        // Check if the username already exists
        var existingUser = await _userService.GetUserByUsernameAsync(signUpModel.Username);
        if (existingUser != null)
        {
            return Conflict("Username already exists");
        }

        // Create a new user object
        User user = new User
        {
            Username = signUpModel.Username,
            PasswordHash = signUpModel.Password,
            Email = signUpModel.Email
        };

        var newUser = await _userService.CreateUserAsync(user);
        if (newUser == null)
        {
            return BadRequest();
        }

        // Generate a JWT for the new user and return it
        var token = await _userService.AuthenticateAsync(signUpModel.Username, signUpModel.Password);
        if (token == null)
        {
            return Unauthorized();
        }
        return Ok(new { token });
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
