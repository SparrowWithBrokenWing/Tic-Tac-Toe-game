using MessageConstruction;

namespace MessageDispatcher
{
    public interface IAsyncPerformer
    {
        public Task ProcessAsync(IMessage message);
    }
}
