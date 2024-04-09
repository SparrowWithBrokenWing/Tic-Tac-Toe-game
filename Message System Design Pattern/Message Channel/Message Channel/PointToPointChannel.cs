using MessageConstruction;
using Receiver;
namespace MessageChannel
{
    public class PointToPointChannel : IChannel
    {
        protected IReceiver _Receiver { get; private set; }

        public PointToPointChannel(IReceiver receiver)
        {
            _Receiver = receiver;
        }

        public void Send(IMessage message)
        {
            _Receiver.Receive(message);
        }
    }
}