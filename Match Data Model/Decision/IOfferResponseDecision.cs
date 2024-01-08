namespace MessageTransformation.CanonicalDataModel.Decision
{
    public interface IOfferResponseDecision : IDecision
    {
        IOffer Offer { get; }
    }
}
