namespace MessageConstruction.IdentifiedMessage
{
    public interface IIdentifiedMessage<TMessageIdentifier> : IMessage
        where TMessageIdentifier : IEquatable<TMessageIdentifier>
    {
        public TMessageIdentifier MessageIdentifier { get; }
    }
}
