using MessageConstruction;
using MessageTransformation.CanonicalDataModel.Message.DecisionMessage;
using Receiver;

namespace Match
{
    public interface IMessageReceivingNotifier
    {
        public event EventHandler<IMessage> OnReceivingMessage;
    }

    public class DecisionMessageReceivingNotifier : IMessageReceivingNotifier, IReceiver
    {
        public event EventHandler<IMessage>? OnReceivingMessage;

        public void Receive(IMessage message)
        {
            if (message is IDecisionMessage decisionMessage)
            {
                OnReceivingMessage?.Invoke(this, message);
            }
        }
    }

    public class InstructionMessageReceivingNotifier: IReceiver, IMessageReceivingNotifier
    {
        public InstructionMessageReceivingNotifier()
        {

        }

        public event EventHandler<IMessage>? OnReceivingMessage;

        public void Receive(IMessage message)
        {
            if (message is IInstructionMessage instructionMessage)
            {
                OnReceivingMessage?.Invoke(this, message);
            }
        }
    }
}
