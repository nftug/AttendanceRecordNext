using Domain.Entities;
using Domain.Interfaces;
using LiteDB.Async;

namespace Infrastructure.Shared;

public abstract class RepositoryBase<TEntity, TDataModel> : IRepository<TEntity>
    where TDataModel : IDataModel<TEntity, TDataModel>, new()
    where TEntity : class, IEntity<TEntity>
{
    protected static LiteDbCollection<TEntity, TDataModel> Context => new();

    protected Task<T> UseCollectionQuery<T>(Func<ILiteQueryableAsync<TDataModel>, Task<T>> callback)
    {
        using var context = Context;
        return callback(GetCollectionForQuery(context));
    }

    protected virtual ILiteQueryableAsync<TDataModel> GetCollectionForQuery(LiteDbCollection<TEntity, TDataModel> db)
        => db.Collection.Query();

    public virtual async Task CreateAsync(TEntity entity)
    {
        using var db = Context;
        var data = new TDataModel().Transfer(entity);
        await db.Collection.InsertAsync(data);
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        using var db = Context;
        var data = new TDataModel().Transfer(entity);
        await db.Collection.UpdateAsync(data);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        using var db = Context;
        await db.Collection.DeleteAsync(id);
    }

    public virtual Task<TEntity?> FindByIdAsync(Guid id)
        => UseCollectionQuery(async query =>
        {
            var data = await query.Where(x => x.Id == id).FirstOrDefaultAsync();
            return data?.ToEntity();
        });
}
