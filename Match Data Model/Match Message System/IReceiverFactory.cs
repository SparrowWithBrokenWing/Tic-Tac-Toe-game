using MessageConstruction;
using Receiver;

namespace Match
{
    public interface IReceiverFactory<TReceiver>
        where TReceiver : IReceiver
    {
        public TReceiver Create();
    }
}
