using MessageChannel;
using MessageConstruction;
using MessageConstruction.IdentifiedMessage;

namespace Match
{
    public class FeatureIntegrableChannel : IChannel
    {
        public FeatureIntegrableChannel(IMessageSendingFeature channelFeature, IChannel adaptedChannel)
        {
            _Feature = channelFeature;
            _AdaptedChannel = adaptedChannel;
        }

        private IChannel _AdaptedChannel { get; set; }

        private IMessageSendingFeature _Feature { get; set; }

        public void Send(IMessage message)
        {
            _Feature.Work(_AdaptedChannel, message);
            _AdaptedChannel.Send(message);
        }
    }

    public interface IMessageSendingFeature
    {
        public void Work(IChannel channel, IMessage message);
    }

    public class MessageTrackingFeature<
        TIdentifiedChannel,
        TChannelIdentifier,
        TIdentifiedMessage,
        TMessageIdentifier
        > : IMessageSendingFeature

        where TIdentifiedChannel : IIdentifiedChannel<TChannelIdentifier>
        where TChannelIdentifier : IEquatable<TChannelIdentifier>
        where TIdentifiedMessage : IIdentifiedMessage<TMessageIdentifier>
        where TMessageIdentifier : IEquatable<TMessageIdentifier>
    {
        public MessageTrackingFeature(MessageChannelSourceTracker<TChannelIdentifier, TMessageIdentifier> messageTracker)
        {
            _MessageTracker = messageTracker;
        }

        protected MessageChannelSourceTracker<TChannelIdentifier, TMessageIdentifier> _MessageTracker { get; private set; }

        public void Work(IChannel channel, IMessage message)
        {
            if (channel is TIdentifiedChannel identifiedChannel
                && message is TIdentifiedMessage identifiedMessage)
            {
                _MessageTracker.Record(identifiedChannel.ChannelIdentifier, identifiedMessage.MessageIdentifier);
            }
        }
    }
}
