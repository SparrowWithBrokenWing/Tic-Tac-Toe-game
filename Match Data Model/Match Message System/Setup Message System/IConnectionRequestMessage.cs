using MessageConstruction;
using MessageChannel;
using MessageConstruction.IdentifiedMessage;
using MessageTransformation.CanonicalDataModel.Message.DecisionMessage;

namespace Match
{
    public interface IRequestMessage : IMessage { }

    public interface IConnectionRequestMessage : IRequestMessage
    {
        public IChannel ReplyingConnectionRequestMessageChanel { get; }
    }

    public class ConnectionRequestMessage : IConnectionRequestMessage
    {
        public ConnectionRequestMessage(IChannel replyingConnectionRequestMessageChannel)
        {
            ReplyingConnectionRequestMessageChanel = replyingConnectionRequestMessageChannel;
        }

        public IChannel ReplyingConnectionRequestMessageChanel { get; private set; }
    }

    public interface IDisconnectionRequestMessage<TSenderIdentifier> : IRequestMessage, ISenderIdentifiedMessage<TSenderIdentifier>
        where TSenderIdentifier : IEquatable<TSenderIdentifier>
    {

    }

    public class DisconnectionRequestMessage<TSenderIdentifier> : IDisconnectionRequestMessage<TSenderIdentifier>
        where TSenderIdentifier : IEquatable<TSenderIdentifier>
    {
        public DisconnectionRequestMessage(TSenderIdentifier senderIdentifier)
        {
            SenderIdentfier = senderIdentifier;
        }

        public TSenderIdentifier SenderIdentfier { get; private set; }
    }
}
