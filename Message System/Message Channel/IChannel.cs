using MessageConstruction;
namespace MessageChannel
{
    public interface IChannel
    {
        public void Send(IMessage message);
    }
}