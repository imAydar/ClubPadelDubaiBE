namespace ClubPadel.DL
{
    public interface IBaseRepository<T>
    {
        IEnumerable<T> GetAll();
        T GetById(Guid id);
        void Save(T item);
        void Delete(Guid id);
    }
}
