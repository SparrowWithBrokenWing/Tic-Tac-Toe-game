using MessageConstruction;
namespace Receiver
{
    public interface IReceiver
    {
        public void Receive(IMessage message);
    }
}