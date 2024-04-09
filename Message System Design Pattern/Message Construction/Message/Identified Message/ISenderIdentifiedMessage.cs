namespace MessageConstruction.IdentifiedMessage
{
    public interface ISenderIdentifiedMessage<TSenderIdentifier> : IMessage
        where TSenderIdentifier : IEquatable<TSenderIdentifier>
    {
        public TSenderIdentifier SenderIdentfier { get; }
    }
}
