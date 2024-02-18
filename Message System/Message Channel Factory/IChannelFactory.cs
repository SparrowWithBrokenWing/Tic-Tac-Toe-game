using MessageChannel;
using MessageConstruction;
using Receiver;

namespace ChannelFactory
{
    public interface IChannelFactory<TChannel>
        where TChannel : IChannel
    {
        public TChannel Create(IReceiver receiver);
    }
}