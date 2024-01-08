using MessageConstruction.IdentifiedMessage;
namespace MessageConstruction.CorrelationIdentifier
{
    public interface IIdentifiedCorrelationMessage<TMessageIdentifier> : IIdentifiedMessage<TMessageIdentifier>
        where TMessageIdentifier : IEquatable<TMessageIdentifier>
    {
        public TMessageIdentifier CorrelationIdentifier { get; }
    }
}
