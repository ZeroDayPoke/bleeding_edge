// Controllers/UserController.cs

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IDatabaseOperations _databaseOperations;

    public UserController(IDatabaseOperations databaseOperations)
    {
        _databaseOperations = databaseOperations;
    }

    // GET: api/User
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        await _databaseOperations.AllAsync(new string[] { "User" });
        return Ok();
    }

    // GET: api/User/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        await _databaseOperations.ShowAsync(new string[] { "User", id.ToString() });
        return Ok();
    }

    // POST: api/User
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] User user)
    {
        if (user.Username == null || user.PasswordHash == null)
        {
            return BadRequest();
        }

        await _databaseOperations.RegisterUserAsync(user.Username, user.PasswordHash);
        return CreatedAtAction("Get", new { id = user.Id }, user);
    }

    // PUT: api/User/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] User user)
    {
        if (user.Username == null || user.PasswordHash == null)
        {
            return BadRequest();
        }

        await _databaseOperations.UpdateAsync(new string[] { "User", id.ToString(), $"Username={user.Username}", $"PasswordHash={user.PasswordHash}" });
        return NoContent();
    }

    // DELETE: api/User/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _databaseOperations.DestroyAsync(new string[] { "User", id.ToString() });
        return NoContent();
    }
}
