using FTask.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Task = System.Threading.Tasks.Task;

namespace FTask.Repository.Data;

public interface IBaseRepository<T, TKey> where T : class
{
    Task<T?> FindAsync(TKey id);
    IQueryable<T> FindAll();
    IQueryable<T> Get(Expression<Func<T, bool>> where);
    IQueryable<T> Get(Expression<Func<T, bool>> where, params Expression<Func<T, object>>[] includes);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
}

public class BaseRepository<T, TKey> : IBaseRepository<T, TKey> where T : class
{
    protected ApplicationDbContext _applicationDbContext;
    protected DbSet<T> dbSet;
    //protected readonly ILogger _logger;

    public BaseRepository(ApplicationDbContext applicationDbContext
        //,ILoggerFactory logFactory
        )
    {
        _applicationDbContext = applicationDbContext;
        dbSet = _applicationDbContext.Set<T>();
        //_logger = logFactory.CreateLogger("logs");
    }

    public virtual async Task AddAsync(T entity)
    {
        await dbSet.AddAsync(entity);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await dbSet.AddRangeAsync(entities);
    }

    public virtual IQueryable<T> FindAll()
    {
        return dbSet.AsNoTracking();
    }

    public virtual async Task<T?> FindAsync(TKey id)
    {
        return await dbSet.FindAsync(id);
    }

    public virtual IQueryable<T> Get(Expression<Func<T, bool>> where)
    {
        return dbSet.Where(where);
    }

    public virtual IQueryable<T> Get(Expression<Func<T, bool>> where, params Expression<Func<T, object>>[] includes)
    {
        var result = dbSet.Where(where);
        foreach (var include in includes)
        {
            result = result.Include(include);
        }
        return result;
    }

    public virtual void Remove(T entity)
    {
        dbSet.Remove(entity);
    }

    public virtual void Update(T entity)
    {
        _applicationDbContext.Entry<T>(entity).State = EntityState.Modified;
    }
}
