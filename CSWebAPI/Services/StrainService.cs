using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class StrainService
{
    private readonly MyDbContext _context;

    public StrainService(MyDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Strain> CreateStrainAsync(Strain strainData, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        strainData.CultivatorId = userId;

        var strain = _context.Strains.Add(strainData);
        await _context.SaveChangesAsync();

        return strain.Entity;
    }

    public async Task<IList<Strain>> GetAllStrainsAsync()
    {
        return await _context.Strains.Include(s => s.Grower).Include(s => s.Stores).ToListAsync();
    }

    public async Task<Strain> GetStrainByIdAsync(int id)
    {
        return await _context.Strains.FindAsync(id);
    }

    public async Task<Strain> UpdateStrainAsync(int id, Strain strainData)
    {
        var strain = await _context.Strains.FindAsync(id);
        if (strain == null)
        {
            throw new Exception("Strain not found");
        }

        _context.Entry(strain).CurrentValues.SetValues(strainData);
        await _context.SaveChangesAsync();

        return strain;
    }

    public async Task DeleteStrainAsync(int id)
    {
        var strain = await _context.Strains.FindAsync(id);
        if (strain == null)
        {
            throw new Exception("Strain not found");
        }

        _context.Strains.Remove(strain);
        await _context.SaveChangesAsync();
    }

    public async Task AddStoreToStrainAsync(int strainId, int storeId)
    {
        var strain = await _context.Strains.FindAsync(strainId);
        if (strain == null)
        {
            throw new Exception("Strain not found");
        }

        var store = await _context.Stores.FindAsync(storeId);
        if (store == null)
        {
            throw new Exception("Store not found");
        }

        strain.Stores.Add(store);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveStoreFromStrainAsync(int strainId, int storeId)
    {
        var strain = await _context.Strains.FindAsync(strainId);
        if (strain == null)
        {
            throw new Exception("Strain not found");
        }

        var store = strain.Stores.FirstOrDefault(s => s.Id == storeId);
        if (store == null)
        {
            throw new Exception("Store not found");
        }

        strain.Stores.Remove(store);
        await _context.SaveChangesAsync();
    }
}
