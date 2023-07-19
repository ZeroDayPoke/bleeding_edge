using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class StoreController : ControllerBase
{
    private readonly IStoreService _storeService;

    public StoreController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    // POST: api/Store
    [HttpPost]
    public async Task<IActionResult> CreateStore([FromBody] Store store)
    {
        var newStore = await _storeService.CreateStoreAsync(store);
        return CreatedAtAction(nameof(GetStoreById), new { id = newStore.Id }, newStore);
    }

    // GET: api/Store
    [HttpGet]
    public async Task<IActionResult> GetStores()
    {
        var stores = await _storeService.GetAllStoresAsync();
        return Ok(stores);
    }

    // GET: api/Store/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStoreById(int id)
    {
        var store = await _storeService.GetStoreByIdAsync(id);
        if (store == null)
        {
            return NotFound();
        }
        return Ok(store);
    }

    // PUT: api/Store/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStore(int id, [FromBody] Store store)
    {
        await _storeService.UpdateStoreAsync(id, store);
        return Ok(store);
    }

    // DELETE: api/Store/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStore(int id)
    {
        await _storeService.DeleteStoreAsync(id);
        return NoContent();
    }

    // POST: api/Store/5/AddStrain/1
    [HttpPost("{storeId}/AddStrain/{strainId}")]
    public async Task<IActionResult> AddStrainToStore(int storeId, int strainId)
    {
        var result = await _storeService.AddStrainToStoreAsync(storeId, strainId);
        if (!result)
        {
            return BadRequest("Unable to add strain to store.");
        }
        return Ok();
    }

    // DELETE: api/Store/5/RemoveStrain/1
    [HttpDelete("{storeId}/RemoveStrain/{strainId}")]
    public async Task<IActionResult> RemoveStrainFromStore(int storeId, int strainId)
    {
        var result = await _storeService.RemoveStrainFromStoreAsync(storeId, strainId);
        if (!result)
        {
            return BadRequest("Unable to remove strain from store.");
        }
        return Ok();
    }
}
