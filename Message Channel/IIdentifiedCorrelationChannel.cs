using MessageConstruction;

namespace MessageChannel
{
    public interface IIdentifiedCorrelationChannel<TChannelIdentifier> : IIdentifiedChannel<TChannelIdentifier>
        where TChannelIdentifier : IEquatable<TChannelIdentifier>
    {
        public TChannelIdentifier CorrelationIdentifier { get; }
    }
}
