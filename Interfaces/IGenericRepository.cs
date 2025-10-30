namespace Luley_Integracion_Net.Interfaces;

public interface IGenericRepository<TEntity>
    where TEntity : class
{
    TEntity? GetById(int id);
    void Add(TEntity entity);
    void AddRange(List<TEntity> entities);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    void DeleteRange(List<TEntity> entities);

    Task<TEntity?> GetByIdAsync(int id);
    Task AddAsync(TEntity entity);
    Task AddRangeAsync(List<TEntity> entities);
}
