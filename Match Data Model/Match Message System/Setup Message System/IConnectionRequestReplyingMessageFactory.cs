using MessageConstruction;
using MessageChannel;
using ChannelFactory;
using Receiver;
using MessageConstruction.IdentifiedMessage;

namespace Match
{
    public interface IConnectionRequestReplyingMessageFactory<TConnectionRequestMessage>
        where TConnectionRequestMessage : IConnectionRequestMessage
    {
        public IConnectionRequestReplyingMessage Create(TConnectionRequestMessage connectionRequestMessage);
    }

    public class AcceptedPlayerConnectionRequestReplyingMessageFactory<
        TSenderIdentifier,
        TMessageSendingChannelReceiver,
        TMessageReceivedChannelReceiver,
        TChannelIdentifier,
        TConnectionRequestMessage,
        TConnectionRequestReplyingMessage
        > : IConnectionRequestReplyingMessageFactory<TConnectionRequestMessage>
        where TSenderIdentifier : IEquatable<TSenderIdentifier>
        where TMessageSendingChannelReceiver : IReceiver
        where TMessageReceivedChannelReceiver : IReceiver, IMessageReceivingNotifier
        where TChannelIdentifier : IEquatable<TChannelIdentifier>
        where TConnectionRequestMessage : IConnectionRequestMessage, ISenderIdentifiedMessage<TSenderIdentifier>
    {
        public AcceptedPlayerConnectionRequestReplyingMessageFactory(
            IChannelFactory<IIdentifiedChannel<TChannelIdentifier>> messageSendingChannelFactory,
            IChannelFactory<IIdentifiedChannel<TChannelIdentifier>> messageReceivedChannelFactory,
            IReceiverFactory<TMessageSendingChannelReceiver> messageSendingChannelReceiverFactory,
            IReceiverFactory<TMessageReceivedChannelReceiver> messageReceivedChannelReceiverFactory,
            ChannelCorrelationTracker<TChannelIdentifier> channelCorrelationTracker,
            ConnectionTracker<TSenderIdentifier> connectionTracker)
        {
            _MessageSendingChannelFactory = messageSendingChannelFactory;
            _MessageReceivedChannelFactory = messageReceivedChannelFactory;
            _MessageSendingChannelReceiverFactory = messageSendingChannelReceiverFactory;
            _MessageReceivedChannelReceiverFactory = messageReceivedChannelReceiverFactory;
            _ChannelCorrelationTracker = channelCorrelationTracker;
            _ConnectionTracker = connectionTracker;
        }

        protected IChannelFactory<IIdentifiedChannel<TChannelIdentifier>> _MessageSendingChannelFactory { get; private set; }

        protected IChannelFactory<IIdentifiedChannel<TChannelIdentifier>> _MessageReceivedChannelFactory { get; private set; }

        protected IReceiverFactory<TMessageSendingChannelReceiver> _MessageSendingChannelReceiverFactory { get; private set; }

        protected IReceiverFactory<TMessageReceivedChannelReceiver> _MessageReceivedChannelReceiverFactory { get; private set; }

        protected ChannelCorrelationTracker<TChannelIdentifier> _ChannelCorrelationTracker { get; private set; }

        protected ConnectionTracker<TSenderIdentifier> _ConnectionTracker { get; private set; }

        public IConnectionRequestReplyingMessage Create(TConnectionRequestMessage connectionRequestMessage)
        { 
            var temporarySendingChannelReceiver = _MessageSendingChannelReceiverFactory.Create();
            var temporarySendingChannel = _MessageSendingChannelFactory.Create(temporarySendingChannelReceiver);

            var temporaryReceivedChannelReceiver = _MessageReceivedChannelReceiverFactory.Create();
            var temporaryReceivedChannel = _MessageReceivedChannelFactory.Create(temporaryReceivedChannelReceiver);

            // track new message sending channel and new message received channel
            _ChannelCorrelationTracker.Record(temporarySendingChannel.ChannelIdentifier, temporaryReceivedChannel.ChannelIdentifier);

            // track new accepted connection
            _ConnectionTracker.Record(connectionRequestMessage.SenderIdentfier);

            // Reply request
            var connectionRequestReplyingMessage = new AcceptedPlayerConnectionRequestReplyingMessage(temporarySendingChannel, temporaryReceivedChannelReceiver);

            return connectionRequestReplyingMessage;
        }

    }
}
