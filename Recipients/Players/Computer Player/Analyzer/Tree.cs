using Moq;
using System.Collections;

namespace ComputerPlayer.Analyzer
{

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

    // I understand that there is a case where tree or game reach to the state so that root will be unpredictable. Maybe that root is a winning move. But the tree root in general case is predictable. should I change the design here?
    //public interface ITreeRoot : ITreeComponent, INextMovesPredictableMove, INextMovesPredictedMove { }
    public interface IGrowingTreeComponent : ITreeComponent, INextMovesPredictableMove, INextMovesPredictedMove { }
    public interface ILivingTreeComponent : ITreeComponent, IPreviousMoveAccessibleMove, IPreviousMoveSpecifiableMove { }
    public interface ITreeBranch : IGrowingTreeComponent, ILivingTreeComponent { }
    public interface ITreeLeaf : ILivingTreeComponent { }
    public interface ITreeFlower : IGrowingTreeComponent, ILivingTreeComponent { }
    public interface ITreeFruit : ILivingTreeComponent { }

    public interface ITree
    {
        public IEnumerable<IMove> PredictedNextMoves { get; }
        public IMoveRetriever PlayedMoves { get; }
        public void Cuttings(IMove newPlayedMove);
        public void Prune(ITreeComponent nextMovesPredictableMove);
        public void Sowing(IMove lastPlayedMove, IMoveRetriever playedMoves);
    }

    // because the next possible possition can be predicted base on 
    public abstract partial class AbstractTree<TTreeComponent> : ITree
        where TTreeComponent : ITreeComponent
    {
        public AbstractTree(
            Tuple<IPlayer, IPlayer> players,
            IBoard playingBoard,
            IMove lastPlayedMove,
            IMoveRetriever playedMoveRetriever)
        {
            Players = players;
            PlayingBoard = playingBoard;

            _playedMoveLog = new PlayedMoveLog(playingBoard, playedMoveRetriever);
            _predictedPreviousMoveRetriever = new PredictedPreviousMoveRetriever(_playedMoveLog);

            _lastPlayedMove = lastPlayedMove;
            _predictedNextMoves = null;
            _hasBeenInitialized = false;
        }

        public IMoveRetriever PlayedMoves => GetMoveRetriever();
        protected Tuple<IPlayer, IPlayer> Players { get; private set; }
        protected IBoard PlayingBoard { get; private set; }

        private bool _hasBeenInitialized;
        private ITreeComponent? _root;
        protected ITreeComponent Root
        {
            get
            {
                if (_root is null)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    return _root;
                }
            }
            private set
            {
                _root = value;
            }
        }

        private IMove? _lastPlayedMove;
        private IEnumerable<IMove>? _predictedNextMoves;
        public IEnumerable<IMove> PredictedNextMoves
        {
            get
            {
                if (_hasBeenInitialized)
                {
                    return _predictedNextMoves is not null ? _predictedNextMoves : throw new NotImplementedException("The predicted next moves has seemed like incomplete intialized.");
                }
                else
                {
                    _hasBeenInitialized = true;
                    if (_predictedNextMoves is not null)
                    {
                        return _predictedNextMoves;
                    }
                    else
                    {
                        if (_lastPlayedMove is not null)
                        {
                            Sowing(_lastPlayedMove, GetMoveRetriever());
                            if (_predictedNextMoves is not null)
                            {
                                return _predictedNextMoves;
                            }
                            else
                            {
                                throw new NotImplementedException("When trying to predict the next moves with new played moves and new last played move, it seem like the Sowing method don't work as expected, PredictedNextMoves set method hasn't called which mean _predictedNextMoves field stay as null.");
                            }
                        }
                        else
                        {
                            throw new NotImplementedException("When trying to predict the next moves with new played moves and new last played move, it seem like last palyed move is missing.");
                        }
                    }
                }
            }
            set => _predictedNextMoves = value;
        }

        // cannot sure if the last move in move retriever is last played move, but the move order is not important to predict next possible moves after a move is played so I let the design of this method like this.
        protected abstract IEnumerable<TTreeComponent> Generate(IMove node, IMoveRetriever nutrition);
    }

    partial class AbstractTree<TTreeComponent>
    {
        public void Cuttings(IMove newPlayedMove)
        {
            var temp = PredictedNextMoves.FirstOrDefault((move) => move.Equals(newPlayedMove));

            if (temp is not null)
            {
                if (temp is ITreeComponent component)
                {
                    Root = component;
                    if (Root is IGrowingTreeComponent growingRoot)
                    {
                        Growth(growingRoot);
                        PredictedNextMoves = growingRoot.PredictedNextMoves;
                    }
                    _playedMoveLog.Record(newPlayedMove);
                    return;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void Prune(ITreeComponent treeComponent)
        {
            if (treeComponent is IPreviousMoveAccessibleMove previousMoveAccessible)
            {
                if (previousMoveAccessible.PreviousMove is INextMovesPredictableMove nextMovesPredictableMove)
                {
                    nextMovesPredictableMove.PredictableNextMoves.Remove(previousMoveAccessible);
                }
            }
        }

        public void Sowing(IMove newSeed, IMoveRetriever newSoil)
        {
            // remove everything unless the root
            PredictedNextMoves = Enumerable.Empty<IMove>();
            // move root to new soil
            _playedMoveLog = new PlayedMoveLog(PlayingBoard, newSoil);
            // regrowth root
            var mockOfGrowingTreeRoot = new Mock<IGrowingTreeComponent>();
            mockOfGrowingTreeRoot.Setup((treeComponent) => treeComponent.Row).Returns(newSeed.Row);
            mockOfGrowingTreeRoot.Setup((treeComponent) => treeComponent.Column).Returns(newSeed.Column);
            mockOfGrowingTreeRoot.Setup((treeComponent) => treeComponent.Player).Returns(newSeed.Player);
            uint componentHeight = 0;
            mockOfGrowingTreeRoot.Setup((treeComponent) => treeComponent.ComponentHeight).Returns(componentHeight);
            var predictedMoves = new List<IMove>();
            mockOfGrowingTreeRoot.Setup((growingTreeComponent) => growingTreeComponent.PredictableNextMoves).Returns(predictedMoves);
            mockOfGrowingTreeRoot.Setup((growingTreeComponent) => growingTreeComponent.PredictedNextMoves).Returns(predictedMoves);
            Growth(mockOfGrowingTreeRoot.Object);
            PredictedNextMoves = predictedMoves;
            Root = mockOfGrowingTreeRoot.Object;
        }

        public class NodeHeightLimitationException : Exception { }
        public class UngrowthableNodeException : Exception { }

        internal void Growth(IGrowingTreeComponent node)
        {
            var treeHeightLimitation = ((PlayingBoard.WinningCondition - 2) * 2 + 1 + 2) * 2;

            if (!(node.ComponentHeight < treeHeightLimitation))
            {
                throw new NodeHeightLimitationException();
            }
            IMoveRetriever moveRetriever;

            // will the last previous move is really some played move? I don't know either. Let's trust that everything is predicted correctly.
            if (node is IPreviousMoveAccessibleMove previousMoveAccessibleMove)
            {
                moveRetriever = GetMoveRetriever(previousMoveAccessibleMove);
            }
            else
            {
                moveRetriever = GetMoveRetriever();
            }

            IEnumerable<TTreeComponent> nextPossibleMoves;
            if (node is INextMovesPredictedMove nextNodesGeneratedNode
                && nextNodesGeneratedNode.PredictedNextMoves.Any())
            {
                // I don't expect there is move that different type of TTreeComponent in thesse next predicted moves.
                nextPossibleMoves = nextNodesGeneratedNode.PredictedNextMoves.OfType<TTreeComponent>();
            }
            else
            {
                nextPossibleMoves = Generate(node, moveRetriever);
            }

            foreach (var predictedMoves in nextPossibleMoves)
            {
                node.PredictableNextMoves.Add(predictedMoves);
                predictedMoves.ComponentHeight = (node.ComponentHeight + 1);
                try
                {
                    if (predictedMoves is IGrowingTreeComponent growingTreeComponent)
                    {
                        Growth(growingTreeComponent);
                    }
                }
                catch (NodeHeightLimitationException)
                {

                }
                catch (UngrowthableNodeException)
                {

                }
            }
        }
    }

    partial class AbstractTree<TTreeComponent>
    {
        private PlayedMoveLog _playedMoveLog;

        internal PredictedPreviousMoveRetriever _predictedPreviousMoveRetriever;

        protected IMoveRetriever GetMoveRetriever() => _playedMoveLog;
        protected IMoveRetriever GetMoveRetriever(IPreviousMoveAccessibleMove previousMoveAccessibleMove)
        {
            _predictedPreviousMoveRetriever.CurrentPreviousMoveAccessibleMove = previousMoveAccessibleMove;
            return _predictedPreviousMoveRetriever;
        }

        internal class PredictedPreviousMoveRetriever : IMoveRetriever
        {
            public PredictedPreviousMoveRetriever(IMoveRetriever playedMoveRetriever)
            {
                _playedMoveRetriever = playedMoveRetriever;
            }

            private IMoveRetriever _playedMoveRetriever;
            public IPreviousMoveAccessibleMove? CurrentPreviousMoveAccessibleMove { get; set; }

            public IEnumerator<IMove> GetEnumerator()
                => CurrentPreviousMoveAccessibleMove is not null
                ? new PredictedPreviousMoveEnumerator(_playedMoveRetriever.GetEnumerator(), CurrentPreviousMoveAccessibleMove)
                : _playedMoveRetriever.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public IMove? Retrieve(int row, int column)
            {
                var move = _playedMoveRetriever.Retrieve(row, column);
                if (move is null)
                {
                    while (move is IPreviousMoveAccessibleMove previousMoveAccessible)
                    {
                        move = previousMoveAccessible.PreviousMove;
                    }
                }
                return move;
            }
        }

        internal class PredictedPreviousMoveEnumerator : IEnumerator<IMove>
        {
            public PredictedPreviousMoveEnumerator(IEnumerator<IMove> playedMoveEnumerator, IPreviousMoveAccessibleMove predictedPreviousMoveAccessibleMove)
            {
                _playedMoveEnumerator = playedMoveEnumerator;
                _originalPredictedPreviousMoveAccessibleMove = predictedPreviousMoveAccessibleMove;
                _currentPredictedMove = _originalPredictedPreviousMoveAccessibleMove;
                _hasMovedToPreviousMove = false;
            }

            private IPreviousMoveAccessibleMove _originalPredictedPreviousMoveAccessibleMove;
            private IMove _currentPredictedMove;
            private bool _hasMovedToPreviousMove;
            private IEnumerator<IMove> _playedMoveEnumerator;

            private bool disposedValue;

            public IMove Current => _hasMovedToPreviousMove ? _currentPredictedMove : _playedMoveEnumerator.Current;

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (_playedMoveEnumerator.MoveNext() is true)
                {
                    return true;
                }
                else
                {
                    if (_currentPredictedMove is IPreviousMoveAccessibleMove previousMoveAccessibleMove)
                    {
                        _currentPredictedMove = previousMoveAccessibleMove.PreviousMove;
                        //_hasMovedToPreviousMove = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            public void Reset()
            {
                _hasMovedToPreviousMove = false;
                _currentPredictedMove = _originalPredictedPreviousMoveAccessibleMove;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {

                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }

    // generate base on the move type categories
    public class TreeImplement1
        <TTreeComponent,
        TTreeBranchFactoryProduct,
        TTreeLeafFactoryProduct,
        TTreeFlowerFactoryProduct,
        TTreeFruitFactoryProduct>
        : AbstractTree<TTreeComponent>
        where TTreeComponent : ITreeComponent, ICategorizableMove, ICategorizedMove
        where TTreeBranchFactoryProduct : ITreeBranch, TTreeComponent
        where TTreeLeafFactoryProduct : ITreeLeaf, TTreeComponent
        where TTreeFlowerFactoryProduct : ITreeFlower, TTreeComponent
        where TTreeFruitFactoryProduct : ITreeFruit, TTreeComponent
    {
        public TreeImplement1(
            Tuple<IPlayer, IPlayer> players,
            IBoard playingBoard,
            IMove lastPlayedMove,
            IMoveRetriever playedMoveRetriever,
            IMoveCategorizer moveCategorizer,
            IMoveFacotry<TTreeBranchFactoryProduct> treeBranchFactory,
            IMoveFacotry<TTreeLeafFactoryProduct> treeLeafFactory,
            IMoveFacotry<TTreeFlowerFactoryProduct> treeFlowerFactory,
            IMoveFacotry<TTreeFruitFactoryProduct> treeFruitFactory)
            : base(players, playingBoard, lastPlayedMove, playedMoveRetriever)
        {
            MoveCategorizer = moveCategorizer;

            TreeBranchFactory = treeBranchFactory;
            TreeLeafFactory = treeLeafFactory;
            TreeFlowerFactory = treeFlowerFactory;
            TreeFruitFactory = treeFruitFactory;
        }

        protected IMoveFacotry<TTreeBranchFactoryProduct> TreeBranchFactory { get; private set; }
        protected IMoveFacotry<TTreeLeafFactoryProduct> TreeLeafFactory { get; private set; }
        protected IMoveFacotry<TTreeFlowerFactoryProduct> TreeFlowerFactory { get; private set; }
        protected IMoveFacotry<TTreeFruitFactoryProduct> TreeFruitFactory { get; private set; }
        protected IMoveCategorizer MoveCategorizer { get; private set; }

        protected override IEnumerable<TTreeComponent> Generate(IMove node, IMoveRetriever nutrition)
        {
            // prepare
            IPlayer nextPlayablePlayer = base.Players.Item1.Equals(node.Player) ? base.Players.Item2 : base.Players.Item2;

            // predict next possible move locations
            IEnumerable<Tuple<int, int>> nextPossibleLocations = Enumerable.Empty<Tuple<int, int>>();

            var playedMoves = nutrition.ToList();
            //if (node is IPreviousMoveAccessibleMove previousMoveAccessibleNode
            //    && previousMoveAccessibleNode.PreviousMove is ICategorizableMove previousMoveCategorizedNode
            //    && previousMoveCategorizedNode.Categories.OfType<IForkMoveType>().Any()
            //    && node is ICategorizedMove categorizedNode
            //    && !categorizedNode.Categories.OfType<IBlockForkMoveType>().Any()
            //    )
            //{

            //}
            //else
            //if (node is IPreviousMoveAccessibleMove previousNode
            //    && previousNode.PreviousMove is IPreviousMoveAccessibleMove previousOfPreviousNode
            //    && previousOfPreviousNode.PreviousMove is INextMovesPredictedMove previousOfPreviousNextMovesPredictedNode)
            //{
            //    var sameRankGeneratedNodes = previousOfPreviousNextMovesPredictedNode.PredictedNextMoves.Where((move) => !move.Equals(node));
            //    nextPossibleLocations.Concat(sameRankGeneratedNodes.Select((move) => new Tuple<int, int>(move.Row, move.Column)));

            //    var minimumRow = node.Row - base.PlayingBoard.WinningCondition;
            //    if (minimumRow < 0) { minimumRow = 0; }
            //    var maximumRow = node.Row + base.PlayingBoard.WinningCondition;
            //    if (maximumRow > base.PlayingBoard.NumberOfColumns) { maximumRow = base.PlayingBoard.NumberOfColumns; }

            //    var minimumColumn = node.Column - base.PlayingBoard.WinningCondition;
            //    if (minimumColumn < 0) { minimumColumn = 0; }
            //    var maximumColumn = node.Column + base.PlayingBoard.WinningCondition;
            //    if (maximumColumn > base.PlayingBoard.NumberOfColumns) { maximumColumn = base.PlayingBoard.NumberOfColumns; }

            //    for (int row = minimumRow; row <= maximumRow; row++)
            //    {
            //        for (int column = minimumColumn; column <= maximumColumn; column++)
            //        {
            //            if (nutrition.Retrieve(row, column) is null)
            //            {
            //                nextPossibleLocations = nextPossibleLocations.Append(new Tuple<int, int>(row, column));
            //            }
            //        }
            //    }
            //}
            //else
            {
                var mockOfIEqualityComparer = new Mock<IEqualityComparer<Tuple<int, int>>>();
                mockOfIEqualityComparer
                    .Setup((comparer) => comparer.Equals(It.IsAny<Tuple<int, int>>(), It.IsAny<Tuple<int, int>>()))
                    .Returns<Tuple<int, int>, Tuple<int, int>>((position, otherPosition) =>
                    {
                        return position.Item1 == otherPosition.Item1
                            && position.Item2 == otherPosition.Item2;
                    });
                var possibleLocations = new HashSet<Tuple<int, int>>(mockOfIEqualityComparer.Object);
                foreach (var playedMove in nutrition)
                {

                    var minimumRow = node.Row - base.PlayingBoard.WinningCondition;
                    if (minimumRow < 0) { minimumRow = 0; }
                    var maximumRow = node.Row + base.PlayingBoard.WinningCondition;
                    if (maximumRow > base.PlayingBoard.NumberOfColumns - 1) { maximumRow = base.PlayingBoard.NumberOfColumns - 1; }

                    var minimumColumn = node.Column - base.PlayingBoard.WinningCondition;
                    if (minimumColumn < 0) { minimumColumn = 0; }
                    var maximumColumn = node.Column + base.PlayingBoard.WinningCondition;
                    if (maximumColumn > base.PlayingBoard.NumberOfColumns - 1) { maximumColumn = base.PlayingBoard.NumberOfColumns - 1; }

                    for (int row = minimumRow; row <= maximumRow; row++)
                    {
                        for (int column = minimumColumn; column <= maximumColumn; column++)
                        {
                            possibleLocations.Add(new Tuple<int, int>(row, column));
                        }
                    }
                }
                possibleLocations.RemoveWhere((location) => nutrition.Any((playedMove) => playedMove.Row == location.Item1 && playedMove.Column == location.Item2));
                nextPossibleLocations = nextPossibleLocations.Concat(possibleLocations);
            }

            if (!nextPossibleLocations.Any())
            {
                throw new UngrowthableNodeException();
            }

            // categorize those location and generate move base on those location
            bool canOnlyPlayOneMoreMove = nutrition.Count() == (base.PlayingBoard.NumberOfRows * base.PlayingBoard.NumberOfColumns) - 1;

            var result = new LinkedList<TTreeComponent>();
            foreach (var predictedLocation in nextPossibleLocations)
            {
                var mockOfMove = new Mock<IMove>();
                mockOfMove.Setup((move) => move.Row).Returns(predictedLocation.Item1);
                mockOfMove.Setup((move) => move.Column).Returns(predictedLocation.Item2);
                mockOfMove.Setup((move) => move.Player).Returns(nextPlayablePlayer);
                var categories = MoveCategorizer.Categorize(mockOfMove.Object, nutrition, base.PlayingBoard);

                if (categories.OfType<IWinningMoveType>().Any())
                {
                    var product = TreeFruitFactory.Produce(predictedLocation.Item1, predictedLocation.Item2, nextPlayablePlayer);
                    ((IPreviousMoveSpecifiableMove)product).PreviousMove = node;
                    ((ICategorizableMove)product).Categories.Concat(categories);
                    result.AddFirst(product);
                } 
                else
                // check if this move is last playable move?
                if (canOnlyPlayOneMoreMove)
                {
                    var product = TreeLeafFactory.Produce(predictedLocation.Item1, predictedLocation.Item2, nextPlayablePlayer);
                    ((IPreviousMoveSpecifiableMove)product).PreviousMove = node;
                    ((ICategorizableMove)product).Categories.Concat(categories);
                    result.AddFirst(product);
                    continue;
                }
                else
                if (categories.OfType<IForkMoveType>().Any()
                    || categories.OfType<IBlockWinningMoveType>().Any()
                    || categories.OfType<IBlockForkMoveType>().Any())
                {
                    var product = TreeFlowerFactory.Produce(predictedLocation.Item1, predictedLocation.Item2, nextPlayablePlayer);
                    ((IPreviousMoveSpecifiableMove)product).PreviousMove = node;
                    ((ICategorizableMove)product).Categories.Concat(categories);
                    result.AddFirst(product);
                }
                else
                if (categories.OfType<IOffensiveMoveType>().Any()
                    || categories.OfType<IDefensiveMoveType>().Any())
                {
                    var product = TreeBranchFactory.Produce(predictedLocation.Item1, predictedLocation.Item2, nextPlayablePlayer);
                    ((IPreviousMoveSpecifiableMove)product).PreviousMove = node;
                    ((ICategorizableMove)product).Categories.Concat(categories);
                    result.AddFirst(product);
                }
            }

            return result;
        }
    }
}