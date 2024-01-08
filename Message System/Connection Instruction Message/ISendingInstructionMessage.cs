using Channel;
using MatchDataModel.Message;
namespace MessageManager.InstructionMessage
{
    public interface ISendingInstructionMessage : IMessage
    {
        public IChannel SendingChannel { get; }
    }
}