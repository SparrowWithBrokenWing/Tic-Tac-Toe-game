using MessageConstruction;
using Receiver;

namespace MessageChannel
{
    public class PubSubChannel : PointToPointChannel
    {
        public PubSubChannel(ICompositeReceiver compositeReceiver) : base(compositeReceiver)
        {

        }
    }
}
