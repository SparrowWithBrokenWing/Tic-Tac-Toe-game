using MessageConstruction.IdentifiedMessage;

namespace Match
{
    public interface IConnectionRequestMessageAllower<TConnectionRequestMessage>
        where TConnectionRequestMessage : IConnectionRequestMessage
    {
        public bool IsAllowed(TConnectionRequestMessage connectionRequestMessage);
    }

    public class LimitedConnectionRequestMessageAllower<TConnectionRequestMessage,TConnectorIdentifier> : IConnectionRequestMessageAllower<TConnectionRequestMessage>
       where TConnectionRequestMessage : IConnectionRequestMessage, ISenderIdentifiedMessage<TConnectorIdentifier>
       where TConnectorIdentifier : IEquatable<TConnectorIdentifier>
    {
        public LimitedConnectionRequestMessageAllower(uint connectionLimitation, ConnectionTracker<TConnectorIdentifier> connectionTracker)
        {
            _ConnectionLimitation = connectionLimitation;
            _ConnectionTracker = connectionTracker;
        }

        protected uint _ConnectionLimitation { get; private set; }
        protected ConnectionTracker<TConnectorIdentifier> _ConnectionTracker { get; private set; }

        public bool IsAllowed(TConnectionRequestMessage connectionRequestMessage)
        {
            return _ConnectionLimitation <_ConnectionTracker.ConnectionAmount;
        }
    }

    public class DisconnectionRequestMessageAllower<TConnectionRequestMessage,TConnectorIdentifier> : IConnectionRequestMessageAllower<TConnectionRequestMessage>
        where TConnectionRequestMessage : IConnectionRequestMessage, ISenderIdentifiedMessage<TConnectorIdentifier>
        where TConnectorIdentifier : IEquatable<TConnectorIdentifier>
    {
        public DisconnectionRequestMessageAllower(ConnectionTracker<TConnectorIdentifier> trackingConnection)
        {
            _TrackingConnection = trackingConnection;
        }

        protected ConnectionTracker<TConnectorIdentifier> _TrackingConnection { get; private set; }

        public bool IsAllowed(TConnectionRequestMessage connectionRequestMessage)
        {
            IConnectionConfirmation? connectionConfirmationOfWantingToDisconectConnector = null;
            try
            {
                connectionConfirmationOfWantingToDisconectConnector = _TrackingConnection.Retrieve(connectionRequestMessage.SenderIdentfier);
            }
            // should handle throwed exception when trying to get connection confirmation from tracker.
            catch
            {

            }
            return connectionConfirmationOfWantingToDisconectConnector is not null;
        }
    }

    public class UnconnecedConnectorConnectionRequestMessageAllower<
        TConnectionRequestMessage,
        TConnectorIdentifier
        > : IConnectionRequestMessageAllower<TConnectionRequestMessage>
        where TConnectionRequestMessage : IConnectionRequestMessage, ISenderIdentifiedMessage<TConnectorIdentifier>
        where TConnectorIdentifier : IEquatable<TConnectorIdentifier>
    {
        public UnconnecedConnectorConnectionRequestMessageAllower(ConnectionTracker<TConnectorIdentifier> trackingConnection)
        {
            _TrackingConnection = trackingConnection;
        }

        protected ConnectionTracker<TConnectorIdentifier> _TrackingConnection { get; private set; }

        public bool IsAllowed(TConnectionRequestMessage connectionRequestMessage)
        {
            IConnectionConfirmation? connectionConfirmation = null;
            try
            {
                connectionConfirmation = _TrackingConnection.Retrieve(connectionRequestMessage.SenderIdentfier);
            }
            catch
            {

            }
            return connectionConfirmation is null;
        }
    }
}

