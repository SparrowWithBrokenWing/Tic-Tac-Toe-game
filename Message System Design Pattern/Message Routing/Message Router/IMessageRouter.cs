using MessageConstruction;
namespace MessageRouter
{
    public interface IMessageRouter
    {
        public void Route(IMessage message);
    }
}