using MessageConstruction;
using MessageDispatcher;
using MessageChannel;
using Receiver;
using ChannelFactory;

namespace Match
{
    public interface IConnectionRequestMessageReplier<TConnectionRequestMessage> : IAsyncPerformer
        where TConnectionRequestMessage : IConnectionRequestMessage
    {

    }

    public class ConnectionRequesMessageReplier<TConnectionRequestMessage> : IConnectionRequestMessageReplier<TConnectionRequestMessage>
        where TConnectionRequestMessage : IConnectionRequestMessage
    {
        public ConnectionRequesMessageReplier(IConnectionRequestReplyingMessageFactory<TConnectionRequestMessage> connectionRequestReplyingMessageFactory)
        {
            _ConnectionRequestReplyingMessageFactory = connectionRequestReplyingMessageFactory;
        }

        protected IConnectionRequestReplyingMessageFactory<TConnectionRequestMessage> _ConnectionRequestReplyingMessageFactory { get; private set; }

        public async Task ProcessAsync(IMessage message)
        {
            try
            {
                if (message is TConnectionRequestMessage connectionRequestMessage)
                {
                    var connectionRequestReplyingMessage = _ConnectionRequestReplyingMessageFactory.Create(connectionRequestMessage);
                    connectionRequestMessage.ReplyingConnectionRequestMessageChanel.Send(connectionRequestReplyingMessage);
                }

                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                await Task.FromException(e);
            }
        }
    }

    public class CompositeConnectionRequestMessageReplier<TConnectionRequestMessage> : IConnectionRequestMessageReplier<TConnectionRequestMessage>
        where TConnectionRequestMessage : IConnectionRequestMessage
    {
        public CompositeConnectionRequestMessageReplier(ICollection<IConnectionRequestMessageReplier<TConnectionRequestMessage>> connectionRequestMessageReplier)
        {
            _ConnectionRequestMessageReplierCollection = connectionRequestMessageReplier;
        }

        protected ICollection<IConnectionRequestMessageReplier<TConnectionRequestMessage>> _ConnectionRequestMessageReplierCollection { get; private set; }

        public void Add(IConnectionRequestMessageReplier<TConnectionRequestMessage> connectionRequestMessageReplier)
        {
            _ConnectionRequestMessageReplierCollection.Add(connectionRequestMessageReplier);
        }

        public void Remove(IConnectionRequestMessageReplier<TConnectionRequestMessage> connectionRequestMessageReplier)
        {
            _ConnectionRequestMessageReplierCollection.Remove(connectionRequestMessageReplier);
        }

        public async Task ProcessAsync(IMessage message)
        {
            if (message is TConnectionRequestMessage connectionRequestMessage)
            {
                List<Task> replyingTasks = new List<Task>();

                foreach (var replier in _ConnectionRequestMessageReplierCollection)
                {
                    Task replyingTask = new Task(() => replier.ProcessAsync(message));
                    replyingTasks.Add(replyingTask);
                }

                await Task.WhenAll(replyingTasks);
            }
            else
            {
                await Task.CompletedTask;
            }
        }
    }

    public class SelectiveConnectionRequestMessageReplier<TConnectionRequestMessage> : IConnectionRequestMessageReplier<TConnectionRequestMessage>
        where TConnectionRequestMessage : IConnectionRequestMessage
    {
        public SelectiveConnectionRequestMessageReplier(
            IConnectionRequestMessageAllower<TConnectionRequestMessage> connectionRequestMessageAllower,
            IConnectionRequestMessageReplier<TConnectionRequestMessage> connectionRequestMessageReplier)
        {
            _ConnectionRequestMessageAllower = connectionRequestMessageAllower;
            _ConnectionRequestMessageReplier = connectionRequestMessageReplier;
        }

        protected IConnectionRequestMessageAllower<TConnectionRequestMessage> _ConnectionRequestMessageAllower { get; private set; }

        protected IConnectionRequestMessageReplier<TConnectionRequestMessage> _ConnectionRequestMessageReplier { get; private set; }

        public async Task ProcessAsync(IMessage message)
        {
            if (message is TConnectionRequestMessage connectionRequestMessage
                && _ConnectionRequestMessageAllower.IsAllowed(connectionRequestMessage))
            {
                await _ConnectionRequestMessageReplier.ProcessAsync(connectionRequestMessage);
            }
            else
            {
                await Task.CompletedTask;
            }
        }
    }
}
