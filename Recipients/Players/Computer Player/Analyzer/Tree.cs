namespace ComputerPlayer
{
    public interface IMoveFacotry<TProduct>
        where TProduct : IMove
    {
        public TProduct Produce(int row, int column, IPlayer player);
    }

    public interface IPreviousMoveSpecifiableMove : IMove
    {
        public IMove PreviousMove { set; }
    }

    public interface IPreviousMoveAccessibleMove : IMove
    {
        public IMove PreviousMove { get; }
    }

    public interface INextMovesPredictableMove : IMove
    {
        public ICollection<IMove> PredictableNextMoves { get; }
    }

    public interface INextMovesPredictedMove : IMove
    {
        public IEnumerable<IMove> PredictedNextMoves { get; }
    }

    public interface ITreeComponent : IMove
    {
        // I want the height of tree should only be set by the tree only, but there is no way that tree can do that when do not know exactly what kind of tree component will be produced by factory
        public uint ComponentHeight { get; set; }
    }

    public interface ITreeRoot : ITreeComponent, INextMovesPredictableMove, INextMovesPredictedMove { }
    public interface ITreeBranch : ITreeComponent, IPreviousMoveSpecifiableMove, IPreviousMoveAccessibleMove, INextMovesPredictableMove, INextMovesPredictedMove { }
    public interface ITreeLeaf : ITreeComponent, IPreviousMoveAccessibleMove, IPreviousMoveSpecifiableMove { }
    public interface ITreeFlower : ITreeComponent, IPreviousMoveSpecifiableMove, IPreviousMoveAccessibleMove, INextMovesPredictableMove, INextMovesPredictedMove { }
    public interface ITreeFruit : ITreeComponent, IPreviousMoveAccessibleMove, IPreviousMoveSpecifiableMove { }

    public interface ITree
    {
        public INextMovesPredictedMove Root { get; }
        public void Growth(IMove newPlayedMove);
        public void Prune(ITreeComponent nextMovesPredictableMove);
        public void Cuttings(IMoveRetriever playedMoves);
    }

    public abstract partial class AbstractTree<TFactoryProduct> : ITree
        where TFactoryProduct : ITreeComponent
    {
        public AbstractTree(Tuple<IPlayer, IPlayer> players, IMoveFacotry<TFactoryProduct> treeComponentFactory, IBoard playingBoard, IMoveRetriever playedMoveRetriever)
        {

        }

        public INextMovesPredictedMove Root => ActualRoot;

        protected ITreeRoot ActualRoot { get; set; }
        protected IMoveFacotry<TFactoryProduct> TreeComponentFactory { get; private set; }
        protected IBoard PlayingBoard { get; private set; }

        private Tuple<IPlayer, IPlayer> _players;

        protected abstract IEnumerable<Tuple<int, int>> PredictPossibleNextMovesPosition(IMove afterThisPlayedMoveFromOpponent, IMoveRetriever withThesePreviousPlayedMoves);

    }
}