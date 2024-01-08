using MessageConstruction;

namespace MessageDispatcher
{
    public interface ICompositeAsyncPerformer : IAsyncPerformer
    {
        public void Add(IAsyncPerformer performer);
        public void Remove(IAsyncPerformer performer);
    }
}
