using Domain.Entities;
using LiteDB.Async;

namespace Infrastructure.Shared;

public class LiteDbCollection<TEntity, TDataModel> : IDisposable
    where TDataModel : IDataModel<TEntity, TDataModel>, new()
    where TEntity : class, IEntity<TEntity>
{
    private readonly IAppInfo _appConfig;
    private readonly ILiteDatabaseAsync _db;
    private readonly ILiteCollectionAsync<TDataModel> _collection;

    public ILiteCollectionAsync<TDataModel> Collection => _collection;

    private bool disposedValue;

    public LiteDbCollection(IAppInfo appConfig)
    {
        _appConfig = appConfig;

        string dbPath = Path.Combine(_appConfig.AppDataPath, "attendance.db");
        _db = new LiteDatabaseAsync(dbPath);

        string tableName = typeof(TEntity).Name;
        _collection = _db.GetCollection<TDataModel>(tableName);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _db.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
