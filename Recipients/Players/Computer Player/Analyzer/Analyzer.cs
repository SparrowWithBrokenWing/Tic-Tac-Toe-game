namespace ComputerPlayer.Analyzer
{
    public interface ICategorizableMove : IMove
    {
        public ICollection<IMove> Categories { get; }
    }

    public interface ICategorizedMove : IMove
    {
        public IEnumerable<IMove> Categories { get; }
    }

    public interface IEvaluatableMove<TValue> : IMove
        where TValue : IComparable<TValue>
    {
        public TValue EvaluateValue { set; }
    }

    public interface IEvaluatedMove<TValue> : IMove
        where TValue : IComparable<TValue>
    {
        public TValue EvaluateValue { get; }
    }
}
