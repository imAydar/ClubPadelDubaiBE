using ClubPadel.DL.EfCore;
using ClubPadel.Models;
using LiteDB;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json.Nodes;

namespace ClubPadel.DL
{
    public class BaseLiteDbRepository<T>: IBaseRepository<T> where T : EntityBase
    {
        private readonly LiteDatabase _database;
        protected readonly ILiteCollection<T> _collection;

        public BaseLiteDbRepository(string databasePath, string collectionName)
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

    public class EfCoreRepository<T> : IBaseRepository<T> where T : EntityBase
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public EfCoreRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public void Delete(Guid id)
        {
            var item = GetById(id);

            _dbSet.Remove(item);
            _context.SaveChanges();
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public T GetById(Guid id)
        {
            return _dbSet.Find(id);
        }

        public void Save(T item)
        {
            var entity = GetById(item.Id);

            if (entity != null)
            {
                entity = item;
                _dbSet.Update(item);
            }
            else
            {
                _dbSet.Add(item);
            }

            _context.SaveChanges();
        }
    }

}
