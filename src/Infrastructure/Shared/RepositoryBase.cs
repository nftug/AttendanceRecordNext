using Domain.Entities;
using Domain.Interfaces;
using LiteDB.Async;

namespace Infrastructure.Shared;

public abstract class RepositoryBase<TEntity, TDataModel> : IRepository<TEntity>
    where TDataModel : IDataModel<TEntity, TDataModel>, new()
    where TEntity : class, IEntity<TEntity>
{
    protected readonly IAppConfig _appConfig;

    protected RepositoryBase(IAppConfig appConfig)
    {
        _appConfig = appConfig;
    }

    protected LiteDbCollection<TEntity, TDataModel> Context => new(_appConfig);

    protected async Task<T> UseCollectionQuery<T>(Func<ILiteQueryableAsync<TDataModel>, Task<T>> callback)
    {
        using var context = Context;
        return await callback(GetCollectionForQuery(context));
    }

    protected virtual ILiteQueryableAsync<TDataModel> GetCollectionForQuery(LiteDbCollection<TEntity, TDataModel> db)
        => db.Collection.Query();

    public virtual async Task SaveAsync(TEntity entity)
    {
        using var db = Context;
        var data = new TDataModel().Transfer(entity);
        await db.Collection.UpsertAsync(data);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        using var db = Context;
        await db.Collection.DeleteAsync(id);
    }

    public virtual async Task<TEntity?> FindByIdAsync(Guid id)
        => await UseCollectionQuery(async query =>
        {
            var data = await query.Where(x => x.Id == id).FirstOrDefaultAsync();
            return data?.ToEntity();
        });
}
