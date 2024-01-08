using MessageChannel;
using MessageConstruction;

namespace Match
{
    public interface IInstruction
    {

    }

    public interface IMessageReceivingInstruction : IInstruction
    {
        public IMessageReceivingNotifier MessageReceivingNotifier { get; }
    }
    public interface IMessageSendingInstruction : IInstruction
    {
        public IChannel MessageSendingChannel { get; }
    }
}
