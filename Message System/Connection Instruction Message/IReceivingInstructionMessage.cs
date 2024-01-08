using Channel;
using MatchDataModel.Message;
namespace MessageManager.InstructionMessage
{
    public interface IReceivingInstructionMessage : IMessage
    {
        public IChannel ReceivingChannel { get; }
    }
}