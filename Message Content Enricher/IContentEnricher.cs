using MessageConstruction;
namespace MessageTransformation.ContentEnricher
{
    public interface IContentEnricher<TBasicMessage, TEnrichedMessage> : IMessageTransformation<TBasicMessage, TEnrichedMessage>
        where TBasicMessage : IMessage
        where TEnrichedMessage : IMessage
    {

    }
}
