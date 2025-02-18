using ClubPadel.Models;

namespace ClubPadel.DL
{
    public interface IBaseRepository<T> where T : EntityBase
    {
        IEnumerable<T> GetAll();
        T GetById(Guid id);
        void Save(T item);
        void Delete(Guid id);
    }
}
