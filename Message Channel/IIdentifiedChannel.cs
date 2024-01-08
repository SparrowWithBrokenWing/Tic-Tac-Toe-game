using MessageConstruction;

namespace MessageChannel
{
    public interface IIdentifiedChannel<TChannelIdentifier> : IChannel
        where TChannelIdentifier : IEquatable<TChannelIdentifier>
    {
        public TChannelIdentifier ChannelIdentifier { get; }
    }
}
