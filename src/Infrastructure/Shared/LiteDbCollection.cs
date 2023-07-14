using LiteDB.Async;

namespace Infrastructure.Shared;

internal class LiteDbCollection<T> : IDisposable
    where T : IDataModel, new()
{
    private readonly ILiteDatabaseAsync _db;
    private readonly ILiteCollectionAsync<T> _collection;

    public ILiteCollectionAsync<T> Collection => _collection;
    public static readonly string DbFileName = "attendance.db";

    private bool disposedValue;

    public LiteDbCollection()
    {
        _db = new LiteDatabaseAsync(DbFileName);
        _collection = _db.GetCollection<T>(new T().TableName);
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
