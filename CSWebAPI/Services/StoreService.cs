using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class StoreService
{
    private readonly MyDbContext _context;

    public StoreService(MyDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Store> CreateStoreAsync(Store storeData)
    {
        var store = _context.Stores.Add(storeData);
        await _context.SaveChangesAsync();

        return store.Entity;
    }

    public async Task<IList<Store>> GetAllStoresAsync()
    {
        return await _context.Stores.Include(s => s.Strains).ToListAsync();
    }

    public async Task<Store> GetStoreByIdAsync(int id)
    {
        return await _context.Stores.FindAsync(id);
    }

    public async Task<Store> UpdateStoreAsync(int id, Store storeData)
    {
        var store = await _context.Stores.FindAsync(id);
        if (store == null)
        {
            throw new Exception("Store not found");
        }

        _context.Entry(store).CurrentValues.SetValues(storeData);
        await _context.SaveChangesAsync();

        return store;
    }

    public async Task DeleteStoreAsync(int id)
    {
        var store = await _context.Stores.FindAsync(id);
        if (store == null)
        {
            throw new Exception("Store not found");
        }

        _context.Stores.Remove(store);
        await _context.SaveChangesAsync();
    }

    public async Task AddStrainToStoreAsync(int storeId, int strainId)
    {
        var store = await _context.Stores.FindAsync(storeId);
        if (store == null)
        {
            throw new Exception("Store not found");
        }

        var strain = await _context.Strains.FindAsync(strainId);
        if (strain == null)
        {
            throw new Exception("Strain not found");
        }

        store.Strains.Add(strain);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveStrainFromStoreAsync(int storeId, int strainId)
    {
        var store = await _context.Stores.FindAsync(storeId);
        if (store == null)
        {
            throw new Exception("Store not found");
        }

        var strain = store.Strains.FirstOrDefault(s => s.Id == strainId);
        if (strain == null)
        {
            throw new Exception("Strain not found");
        }

        store.Strains.Remove(strain);
        await _context.SaveChangesAsync();
    }
}
