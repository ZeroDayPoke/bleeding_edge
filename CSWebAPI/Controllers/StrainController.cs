using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class StrainController : ControllerBase
{
    private readonly IStrainService _strainService;

    public StrainController(IStrainService strainService)
    {
        _strainService = strainService;
    }

    // POST: api/Strain
    [HttpPost]
    public async Task<IActionResult> CreateStrain([FromBody] Strain strain)
    {
        var newStrain = await _strainService.CreateStrainAsync(strain);
        return CreatedAtAction(nameof(GetStrainById), new { id = newStrain.Id }, newStrain);
    }

    // PUT: api/Strain/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStrain(int id, [FromBody] Strain strain)
    {
        await _strainService.UpdateStrainAsync(id, strain);
        return Ok(strain);
    }

    // GET: api/Strain
    [HttpGet]
    public async Task<IActionResult> GetStrains()
    {
        var strains = await _strainService.GetAllStrainsAsync();
        return Ok(strains);
    }

    // GET: api/Strain/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStrainById(int id)
    {
        var strain = await _strainService.GetStrainByIdAsync(id);
        if (strain == null)
        {
            return NotFound();
        }
        return Ok(strain);
    }

    // DELETE: api/Strain/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStrain(int id)
    {
        await _strainService.DeleteStrainAsync(id);
        return NoContent();
    }

    // POST: api/Strain/5/AddStore/1
    [HttpPost("{strainId}/AddStore/{storeId}")]
    public async Task<IActionResult> AddStoreToStrain(int strainId, int storeId)
    {
        var result = await _strainService.AddStoreToStrainAsync(strainId, storeId);
        if (!result)
        {
            return BadRequest("Unable to add store to strain.");
        }
        return Ok();
    }

    // DELETE: api/Strain/5/RemoveStore/1
    [HttpDelete("{strainId}/RemoveStore/{storeId}")]
    public async Task<IActionResult> RemoveStoreFromStrain(int strainId, int storeId)
    {
        var result = await _strainService.RemoveStoreFromStrainAsync(strainId, storeId);
        if (!result)
        {
            return BadRequest("Unable to remove store from strain.");
        }
        return Ok();
    }
}
