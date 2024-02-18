using ComputerPlayer.Endpoint;
using Match;
using MessageChannel;
using MessageConstruction;
using MessageTransformation.CanonicalDataModel.Decision;
using Receiver;

namespace ComputerPlayer
{
    public class Player : IReceiver
    {
        public Player(IChannel setupChannel)
        {
            _SetupChannel = setupChannel;

            _ReplyingConnectionRequestMessaageChannel = new PointToPointChannel(this);

            var joinGameRequest = new ConnectionRequestMessage(_ReplyingConnectionRequestMessaageChannel);

            //var analyzer = new Analyzer();
            //Task analyzeTask = analyzer.Analyze();
        }

        protected IChannel _SetupChannel { get; private set; }

        protected IChannel _ReplyingConnectionRequestMessaageChannel;

        public void Receive(IMessage message)
        {
            if (message is IMessageSendingInstruction messageSendingInstruction
                && message is IMessageReceivingInstruction messageReceivingInstruction)
            {
                _DecisionMesssageSendingChannel = messageSendingInstruction.MessageSendingChannel;
                _DecisionMessageReceivingNotifier = messageReceivingInstruction.MessageReceivingNotifier;
            }
            else
            {
                // resend a message to organizer that it cannot understand received instruction message
            }
        }

        protected IChannel? _DecisionMesssageSendingChannel { get; set; }
        protected IMessageReceivingNotifier? _DecisionMessageReceivingNotifier { get; set; }
    }

    //public class Decider
    //{
    //    public void SendDecidedDecision() { }
    //    public void ReceiveOpponentDecision() { }

    //    public IDecision GetSuggestedDecision() { }
    //}
}
