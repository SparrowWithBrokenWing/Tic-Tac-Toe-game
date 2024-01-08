using MessageConstruction;
using MessageDispatcher;
using MessageTransformation.CanonicalDataModel.Message.DecisionMessage;
using Receiver;

namespace Match
{
    public partial class Communicator<TDecisionMessage> : IAsyncDispatcher, IReceiver
        where TDecisionMessage : IDecisionMessage
    {
        public Communicator(IAsyncPerformer asyncPerformer)
        {
            _AsyncPerformer = asyncPerformer;
        }

        protected IAsyncPerformer _AsyncPerformer { get; private set; }

        public async Task PerformAsync(IMessage message)
        {
            if (message is IDecisionMessage decisionMessage)
            {
                await _AsyncPerformer.ProcessAsync(decisionMessage);
            }
            else
            {
                await Task.CompletedTask;
            }
        }

        public void Receive(IMessage message)
        {
            if (message is TDecisionMessage decisionMessage)
            {
                Task.Run(() => this.PerformAsync(decisionMessage));
            }
        }
    }
}
