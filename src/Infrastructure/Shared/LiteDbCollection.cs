using Domain.Entities;
using LiteDB.Async;

namespace Infrastructure.Shared;

public class LiteDbCollection<TEntity, TDataModel> : IDisposable
    where TDataModel : IDataModel<TEntity, TDataModel>, new()
    where TEntity : class, IEntity<TEntity>
{
    private readonly ILiteDatabaseAsync _db;
    private readonly ILiteCollectionAsync<TDataModel> _collection;

    public ILiteCollectionAsync<TDataModel> Collection => _collection;

    public static readonly string DbPath = Path.Combine(AppConfig.AppDataPath, "attendance.db");

    private bool disposedValue;

    public LiteDbCollection()
    {
        _db = new LiteDatabaseAsync(DbPath);
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
