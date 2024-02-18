using MessageChannel;
using MessageConstruction;
using MessageConstruction.IdentifiedMessage;
using MessageConstruction.CorrelationIdentifier;
using MessageRouter;
using MessageDispatcher;

namespace Match
{
    public class MessageReplyingPerformer<
        TReplyMessageDestinationChannel,
        TChannelIdentifier,
        TMessageIdentifier,
        TReplyMessage
        > : IAsyncPerformer

        where TReplyMessageDestinationChannel : IIdentifiedChannel<TChannelIdentifier>
        where TChannelIdentifier : IEquatable<TChannelIdentifier>
        where TMessageIdentifier : IEquatable<TMessageIdentifier>
        where TReplyMessage : IIdentifiedCorrelationMessage<TMessageIdentifier>
    {
        public MessageReplyingPerformer(
            MessageChannelSourceTracker<TChannelIdentifier, TMessageIdentifier> messageSourceTracker,
            ChannelCorrelationTracker<TChannelIdentifier> channelCorrelationTracker,
            IdentifiedChannelDictionary<TReplyMessageDestinationChannel, TChannelIdentifier> destinationChannelDictionary)
        {
            _MessageSourceTracker = messageSourceTracker;
            _ChannelCorrelationTracker = channelCorrelationTracker;
            _DestinationChannelDictionary = destinationChannelDictionary;
        }

        private MessageChannelSourceTracker<TChannelIdentifier, TMessageIdentifier> _MessageSourceTracker { get; set; }

        private ChannelCorrelationTracker<TChannelIdentifier> _ChannelCorrelationTracker { get; set; }

        private IdentifiedChannelDictionary<TReplyMessageDestinationChannel, TChannelIdentifier> _DestinationChannelDictionary { get; set; }

        public async Task ProcessAsync(IMessage message)
        {
            if (message is TReplyMessage replyMessage)
            {
                TMessageIdentifier repliedMessageIdentifier = replyMessage.CorrelationIdentifier;
                TChannelIdentifier sourceChannelIdentifier = _MessageSourceTracker.Retrieve(repliedMessageIdentifier);
                TChannelIdentifier destinationChannelIdentifier = _ChannelCorrelationTracker.Retrieve(sourceChannelIdentifier);
                TReplyMessageDestinationChannel destinationChannel = _DestinationChannelDictionary[destinationChannelIdentifier];
                destinationChannel.Send(message);
            }
            
            await Task.CompletedTask;
        }
    }

    public class IdentifiedChannelDictionary<TIdentifiedChannel, TChannelIdentifier> : Dictionary<TChannelIdentifier, TIdentifiedChannel>
        where TIdentifiedChannel : IIdentifiedChannel<TChannelIdentifier>
        where TChannelIdentifier : notnull, IEquatable<TChannelIdentifier>
    {

    }
}
