using MessageChannel;
using MessageConstruction;

namespace MessageRouter.DynamicRouter
{
    public interface IDynamicRouter : IMessageRouter
    {
        public void Add(IChannel outputChannel, Func<IMessage, bool> messageRequirement);
    }
}
