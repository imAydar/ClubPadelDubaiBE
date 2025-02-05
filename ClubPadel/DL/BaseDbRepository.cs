using LiteDB;
using System.Text.Json.Nodes;

namespace ClubPadel.DL
{
    public class BaseDbRepository<T>: IBaseRepository<T>
    {
        private readonly LiteDatabase _database;
        private readonly ILiteCollection<T> _collection;

        public BaseDbRepository(string databasePath, string collectionName)
        {
            _database = new LiteDatabase(databasePath);
            _collection = _database.GetCollection<T>(collectionName);
        }

        public IEnumerable<T> GetAll() => _collection.FindAll();

        public T GetById(Guid id)
        {
            return _collection.FindById(new BsonValue(id));
        }

        public void Save(T item)
        {
            _collection.Upsert(item);
        }

        public void Delete(Guid id)
        {
            _collection.Delete(new BsonValue(id));
        }
    }
}
