using MessageChannel;
using MessageConstruction;

namespace MessageRouter.DynamicRouter
{
    public class DynamicRouter : IDynamicRouter
    {
        private ICollection<IChannel> _OutputChannels;

        public DynamicRouter()
        {
            _OutputChannels = new List<IChannel>();
        }

        // this method act like a control channel to add new output channel.
        public void Add(IChannel outputChannel, Func<IMessage, bool> messageRequirement)
        {
            _OutputChannels.Add(new ConditionChannel(outputChannel, messageRequirement));
        }

        public void Route(IMessage message)
        {
            var messageExceptionComposite = new MessageExceptionComposite();
            bool isThereAnyExeptionThrown = false;
            foreach (IChannel channel in _OutputChannels)
            {
                try
                {
                    channel.Send(message);
                }
                catch (ConditionChannel.UnsatisfiedRequirementException exception)
                {
                    isThereAnyExeptionThrown = true;
                    messageExceptionComposite.Add(exception);
                }
            }

            if (isThereAnyExeptionThrown)
            {
                throw messageExceptionComposite;
            }
        }

        protected class MessageException : Exception { }

        protected class MessageExceptionComposite : MessageException
        {
            private ICollection<MessageException> _MessageExceptions = new List<MessageException>();
            public void Add(MessageException newMessageException)
            {
                _MessageExceptions.Add(newMessageException);
            }
            public void Remove(MessageException compositedMessageException)
            {
                _MessageExceptions.Remove(compositedMessageException);
            }
        }

        protected class ConditionChannel : IChannel
        {
            protected IChannel Channel { get; set; }
            protected Func<IMessage, bool> MessageRequirement { get; set; }

            public ConditionChannel(IChannel channel, Func<IMessage, bool> messageRequirement)
            {
                MessageRequirement = messageRequirement;
                Channel = channel;
            }

            public void Send(IMessage message)
            {
                if (MessageRequirement(message))
                {
                    Channel.Send(message);
                }
                else
                {
                    throw new UnsatisfiedRequirementException();
                }
            }

            protected internal class UnsatisfiedRequirementException : MessageException { }
        }

    }
}
