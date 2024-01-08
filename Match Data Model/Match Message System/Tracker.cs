namespace Match
{
    public class ChannelCorrelationTracker<TChannelIdentifier> 
        where TChannelIdentifier : notnull
    {
        public ChannelCorrelationTracker(IDictionary<TChannelIdentifier, TChannelIdentifier> trackedCorrelationChannelDictionary)
        {
            _TrackingCorrelationChannelDictionary = trackedCorrelationChannelDictionary;
        }

        private IDictionary<TChannelIdentifier, TChannelIdentifier> _TrackingCorrelationChannelDictionary { get; set; }

        public void Record(TChannelIdentifier channelIdentifier, TChannelIdentifier correlationChannelIdentifier)
        {
            _TrackingCorrelationChannelDictionary.Add(channelIdentifier, correlationChannelIdentifier);
        }

        public TChannelIdentifier Retrieve(TChannelIdentifier channelIdentifer)
        {
            return _TrackingCorrelationChannelDictionary[channelIdentifer];
        }
    }

    public interface IConnectionConfirmation : IDisposable { }

    public class ConnectionTracker<TConnectorIdentifier>
        where TConnectorIdentifier : IEquatable<TConnectorIdentifier>
    {
        protected IDictionary<TConnectorIdentifier, IConnectionConfirmation> _TrackingConnection { get; private set; }

        public ConnectionTracker(IDictionary<TConnectorIdentifier, IConnectionConfirmation> trackedConnection)
        {
            _TrackingConnection = trackedConnection;
        }

        public void Record(TConnectorIdentifier connectorIdentifier)
        {
            var connectionConfirmation = new ConnectionConfirmation<TConnectorIdentifier, IConnectionConfirmation>(connectorIdentifier, _TrackingConnection);
            _TrackingConnection.Add(connectorIdentifier, connectionConfirmation);
        }

        public IConnectionConfirmation Retrieve(TConnectorIdentifier connectorIdentifier)
        {
            return _TrackingConnection[connectorIdentifier];
        }

        public void Remove(TConnectorIdentifier connectorIdentifier)
        {
            _TrackingConnection.Remove(connectorIdentifier);
        }

        public uint ConnectionAmount => (uint)_TrackingConnection.Count;

        protected class ConnectionConfirmation<TIdentifier, TConfirmation> : IConnectionConfirmation
            where TIdentifier : IEquatable<TIdentifier>
            where TConfirmation : IConnectionConfirmation
        {
            public ConnectionConfirmation(TIdentifier connectionIdentifier, IDictionary<TIdentifier, TConfirmation> trackingConnectionConfirmation)
            {
                _ConfirmedConnectionIdentifier = connectionIdentifier;
                _TrackingConnectionConfirmation = trackingConnectionConfirmation;
            }

            protected TIdentifier _ConfirmedConnectionIdentifier { get; private set; }
            protected IDictionary<TIdentifier, TConfirmation> _TrackingConnectionConfirmation { get; private set; }

            public void Dispose()
            {
                _TrackingConnectionConfirmation.Remove(_ConfirmedConnectionIdentifier);
            }
        }
    }

    public class MessageChannelSourceTracker<TChannelIdentifier, TMessageIdentifier>
        where TChannelIdentifier : IEquatable<TChannelIdentifier>
        where TMessageIdentifier : IEquatable<TMessageIdentifier>
    {
        public MessageChannelSourceTracker(IDictionary<TMessageIdentifier, TChannelIdentifier> trackedMessageDictionary)
        {
            _TrackingMessageDictionary = trackedMessageDictionary;
        }

        private IDictionary<TMessageIdentifier, TChannelIdentifier> _TrackingMessageDictionary
        { get; set; }

        public void Record(TChannelIdentifier channelIdentifier, TMessageIdentifier messageIdentifier)
        {
            _TrackingMessageDictionary.Add(new KeyValuePair<TMessageIdentifier, TChannelIdentifier>(messageIdentifier, channelIdentifier));
        }

        public TChannelIdentifier Retrieve(TMessageIdentifier messageIdentifier)
        {
            return _TrackingMessageDictionary[messageIdentifier];
        }
    }

    public class MessageCorrelationTracker<TMessageCorrelationIdentifier, TMessageIdentifier>
        where TMessageIdentifier : IEquatable<TMessageIdentifier>
        where TMessageCorrelationIdentifier : IEquatable<TMessageCorrelationIdentifier>
    {
        public MessageCorrelationTracker(IDictionary<TMessageCorrelationIdentifier, TMessageIdentifier> trackedMessageCorrelationDictionary)
        {
            _TrackingMessageCorrelationDictianry = trackedMessageCorrelationDictionary;
        }

        private IDictionary<TMessageCorrelationIdentifier, TMessageIdentifier> _TrackingMessageCorrelationDictianry { get; set; }

        public void Record(TMessageCorrelationIdentifier messageCorrelationIdentifier, TMessageIdentifier messageIdentifier)
        {
            _TrackingMessageCorrelationDictianry.Add(new KeyValuePair<TMessageCorrelationIdentifier, TMessageIdentifier>(messageCorrelationIdentifier, messageIdentifier));
        }

        public TMessageIdentifier Retrieve(TMessageCorrelationIdentifier messageCorrelationIdentifier)
        {
            return _TrackingMessageCorrelationDictianry[messageCorrelationIdentifier];
        }
    }
}
