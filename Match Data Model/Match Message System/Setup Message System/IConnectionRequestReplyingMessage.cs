using MessageChannel;
using MessageConstruction;
using MessageTransformation.CanonicalDataModel.Message.DecisionMessage;

namespace Match
{
    public interface IConnectionRequestReplyingMessage : IMessage { }

    public class AcceptedPlayerConnectionRequestReplyingMessage : IAcceptedConnectionRequestReplyingMessage, IMessageSendingInstruction, IMessageReceivingInstruction
    {
        public AcceptedPlayerConnectionRequestReplyingMessage(IChannel messageSendingChannel, IMessageReceivingNotifier messageReceivingNotifier)
        {
            MessageSendingChannel = messageSendingChannel;
            MessageReceivingNotifier = messageReceivingNotifier;
        }

        public IChannel MessageSendingChannel { get; private set; }

        public IMessageReceivingNotifier MessageReceivingNotifier { get; private set; }
    }

    public class AcceptedViewerConnectionRequestReplyingMessage : IAcceptedConnectionRequestReplyingMessage, IMessageReceivingInstruction
    {
        public AcceptedViewerConnectionRequestReplyingMessage(IMessageReceivingNotifier messageReceivingNotifier)
        {
            MessageReceivingNotifier = messageReceivingNotifier;
        }

        public IMessageReceivingNotifier MessageReceivingNotifier { get; private set; }
    }

    public class AcceptedArbiterConnectionRequestReplyingMessage : IAcceptedConnectionRequestReplyingMessage, IMessageSendingInstruction, IMessageReceivingInstruction
    {
        public AcceptedArbiterConnectionRequestReplyingMessage(IChannel messageSendingChannel, IMessageReceivingNotifier messageReceivingNotifier)
        {
            MessageSendingChannel = messageSendingChannel;
            MessageReceivingNotifier = messageReceivingNotifier;
        }

        public IChannel MessageSendingChannel { get; private set; }

        public IMessageReceivingNotifier MessageReceivingNotifier { get; private set; }
    }

    public interface IDeniedConnectionRequestReplyingMessage : IConnectionRequestReplyingMessage
    {

    }

    public interface IAcceptedConnectionRequestReplyingMessage : IConnectionRequestReplyingMessage
    {

    }
}

