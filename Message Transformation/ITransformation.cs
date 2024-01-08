using MessageConstruction;
namespace MessageTransformation
{
    public interface IMessageTransformation<TInput, TOutput>
        where TInput : IMessage
    {
        public TOutput Transform(TInput input);
    }
}