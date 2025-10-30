using Luley_Integracion_Net.Data;
using Luley_Integracion_Net.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Luley_Integracion_Net.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : class
{
    protected readonly LuleyDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(LuleyDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public TEntity? GetById(int id)
    {
        return _dbSet.Find(id);
    }

    public void Add(TEntity entity)
    {
        _dbSet.Add(entity);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public void DeleteRange(List<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public void AddRange(List<TEntity> entities)
    {
        _dbSet.AddRange(entities);
    }

    // Async

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task AddRangeAsync(List<TEntity> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }
}
