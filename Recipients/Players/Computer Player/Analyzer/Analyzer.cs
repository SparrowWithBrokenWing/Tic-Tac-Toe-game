namespace ComputerPlayer.Analyzer
{
    public interface IMoveType { }

    public interface IPossibleMove : IMoveType { }
    public interface ITacticalMove : IPossibleMove { }

    public interface IOffensiveMove : ITacticalMove { }
    public interface IForkMove : IOffensiveMove { }
    public interface IWinningMove : IOffensiveMove { }

    public interface IDefensiveMove : ITacticalMove { }
    public interface IBlockForkMove : IDefensiveMove { }
    public interface IBlockWinningMove : IDefensiveMove { }

    public interface ICategorizableMove : IMove
    {
        public ICollection<IMoveType> Categories { get; }
    }

    public interface ICategorizedMove : IMove
    {
        public IEnumerable<IMoveType> Categories { get; }
    }
{
    public interface IEvaluableMove<TValue> : IMove
        where TValue : IComparable<TValue>
    {
        public TValue NoveValue { set; }
    }

    public interface IEvaluatedMove<TValue> : IMove
        where TValue : IComparable<TValue>
    {
        public TValue EvaluateValue { get; }
    }

    public interface IAnalyzer
    {
        public Tuple<int, int> Analyze();
    }

    public abstract class AbstractAnalyzer<TTreeNode, TNodeValue> : IAnalyzer
        where TNodeValue : IComparable<TNodeValue>
        where TTreeNode : ITreeComponent, IEvaluableMove<TNodeValue>, IEvaluatedMove<TNodeValue>, ICategorizableMove, ICategorizedMove
    {
        public AbstractAnalyzer(AbstractTree<TTreeNode> tree)
        {

        }
    }
}