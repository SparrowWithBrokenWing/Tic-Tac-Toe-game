using SharedDataModel.Message;
namespace MessageProcessor
{
    public interface IMessageProcessor<TMessage>
        where TMessage : IMessage
    {
        public void Process(TMessage message);
    }
}