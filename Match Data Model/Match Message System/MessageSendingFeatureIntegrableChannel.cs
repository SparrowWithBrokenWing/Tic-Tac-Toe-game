using MessageChannel;
using MessageConstruction;
using MessageConstruction.IdentifiedMessage;

namespace Match
{
    public class FeatureIntegrableChannel : IChannel
    {
        public FeatureIntegrableChannel(IMessageSendingFeature channelFeature, IChannel adaptedChannel)
        {
            _feature = channelFeature;
            _adaptedChannel = adaptedChannel;
        }

        private IChannel _adaptedChannel { get; set; }

        private IMessageSendingFeature _feature { get; set; }

        public void Send(IMessage message)
        {
            _feature.Work(_adaptedChannel, message);
            _adaptedChannel.Send(message);
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
