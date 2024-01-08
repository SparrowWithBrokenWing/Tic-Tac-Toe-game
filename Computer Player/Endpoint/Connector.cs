using MessageConstruction;
using MessageChannel;

namespace ComputerPlayer.Endpoint
{
    public interface IConnector<TInstruction>
    {
        public TInstruction Connect();
    }

    public class MatchConnectingInstructionMessage : IMessage
    {
        public IChannel MessageSendingChannel { get; }
        public IChannel MessageReceivingChannel { get; }

        public MatchConnectingInstructionMessage(IChannel messageSendingChannel, IChannel messsageReceivingChannel)
        {
            MessageSendingChannel = messageSendingChannel;
            MessageReceivingChannel = messsageReceivingChannel;
        }
    }

    // get the message instruction, set the thing that gateway use to send message to message system follow the received instruction so that it can be used to send message to message system.
    //public class MatchConnector : IConnector<MatchConnectingInstructionMessage>
    //{
    //    protected IChannel<IMessage> ConnectionRequestChannel { get; private set; }

    //    public MatchConnector(IChannel<IMessage> connectionRequestChannel)
    //    {
    //        ConnectionRequestChannel = connectionRequestChannel;
    //    }

    //    public MatchConnectingInstructionMessage Connect()
    //    {
    //        ConnectionRequestChannel.Send()
    //    }
    //}
}
