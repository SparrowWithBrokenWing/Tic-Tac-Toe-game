using MessageConstruction;

namespace MessageDispatcher
{
    public interface IAsyncDispatcher
    {
        public Task PerformAsync(IMessage message);
    }
}
