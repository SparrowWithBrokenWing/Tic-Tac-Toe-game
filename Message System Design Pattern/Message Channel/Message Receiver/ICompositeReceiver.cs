using MessageConstruction;

namespace Receiver
{
    public interface ICompositeReceiver : IReceiver
    {
        public void Add(IReceiver receiver);
        public void Remove(IReceiver receiver);
    }
}