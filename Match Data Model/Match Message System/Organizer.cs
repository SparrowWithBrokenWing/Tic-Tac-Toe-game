using MessageConstruction;
using MessageDispatcher;
using Receiver;

namespace Match
{
    public class Organizer<TConnectionRequestMessage> : IAsyncDispatcher, IReceiver
        where TConnectionRequestMessage : IConnectionRequestMessage
    {
        public Organizer(IAsyncPerformer asyncPerformer)
        {
            _AsyncPerformer = asyncPerformer;
        }

        protected IAsyncPerformer _AsyncPerformer { get; private set; }

        public async Task PerformAsync(IMessage message)
        {
            if (message is TConnectionRequestMessage connectionRequestMessage)
            {
                await _AsyncPerformer.ProcessAsync(connectionRequestMessage);
            }
            else
            {
                await Task.CompletedTask;
            }
        }

        public void Receive(IMessage message)
        {
            if (message is TConnectionRequestMessage connectionRequestMessage)
            {
                Task.Run(() => _AsyncPerformer.ProcessAsync(connectionRequestMessage));
            }
        }
    }
}

