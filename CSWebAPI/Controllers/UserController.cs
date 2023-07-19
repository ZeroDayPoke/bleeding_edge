using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        var existingUser = await _userService.GetUserByUsernameAsync(signUpModel.Username);
        if (existingUser != null)
        {
            return Conflict("Username already exists");
        }

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

    // PUT: api/User/{id}/Favorite/{strainId}
    [HttpPut("{id}/Favorite/{strainId}")]
    public async Task<IActionResult> AddFavoriteStrain(int id, int strainId)
    {
        await _userService.AddFavoriteStrainAsync(id, strainId);
        return Ok();
    }

    // DELETE: api/User/{id}/Favorite/{strainId}
    [HttpDelete("{id}/Favorite/{strainId}")]
    public async Task<IActionResult> RemoveFavoriteStrain(int id, int strainId)
    {
        await _userService.RemoveFavoriteStrainAsync(id, strainId);
        return NoContent();
    }

    // GET: api/User/{id}/Favorites
    [HttpGet("{id}/Favorites")]
    public async Task<ActionResult<List<int>>> GetFavoriteStrains(int id)
    {
        var strains = await _userService.GetFavoriteStrainsAsync(id);
        if (strains == null)
        {
            return NotFound();
        }
        return strains;
    }

    // POST: api/User/RequestResetPassword
    [HttpPost("RequestResetPassword")]
    public async Task<IActionResult> RequestResetPassword([FromBody] ResetPasswordRequestModel model)
    {
        await _userService.RequestPasswordResetAsync(model.Email);
        return Ok();
    }

    // PUT: api/User/ResetPassword/{token}
    [HttpPut("ResetPassword/{token}")]
    public async Task<IActionResult> ResetPassword(string token, [FromBody] ResetPasswordModel model)
    {
        await _userService.ResetPasswordAsync(token, model.Password);
        return Ok();
    }

    // POST: api/User/RequestVerificationEmail
    [HttpPost("RequestVerificationEmail")]
    public async Task<IActionResult> RequestVerificationEmail([FromBody] VerificationEmailRequestModel model)
    {
        await _userService.RequestEmailVerificationAsync(model.Email);
        return Ok();
    }

    // PUT: api/User/VerifyEmail/{token}
    [HttpPut("VerifyEmail/{token}")]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        await _userService.VerifyEmailAsync(token);
        return Ok();
    }
}
