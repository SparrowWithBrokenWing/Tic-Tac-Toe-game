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
    public interface ITreeRoot : ITreeComponent, INextMovesPredictableMove, INextMovesPredictedMove { }
    public interface ITreeBranch : ITreeComponent, IPreviousMoveSpecifiableMove, IPreviousMoveAccessibleMove, INextMovesPredictableMove, INextMovesPredictedMove { }
    public interface ITreeLeaf : ITreeComponent, IPreviousMoveAccessibleMove, IPreviousMoveSpecifiableMove { }
    public interface ITreeFlower : ITreeComponent, IPreviousMoveSpecifiableMove, IPreviousMoveAccessibleMove, INextMovesPredictableMove, INextMovesPredictedMove { }
    public interface ITreeFruit : ITreeComponent, IPreviousMoveAccessibleMove, IPreviousMoveSpecifiableMove { }

    public interface ITree
    {
        public IMoveRetriever PlayedMoves { get; }
        public INextMovesPredictedMove LastPlayedMove { get; }
        public void Cuttings(IMove newPlayedMove);
        public void Prune(ITreeComponent nextMovesPredictableMove);
        public void Sowing(IMoveRetriever playedMoves, IMove lastPlayedMove);
    }

    // because the next possible possition can be predicted base on 
    public abstract partial class AbstractTree<TTreeRootFactoryProduct, TTreeBranchFactoryProduct, TTreeLeafFactoryProduct, TTreeFlowerFactoryProduct, TTreeFruitFactoryProduct> : ITree
        where TTreeRootFactoryProduct : ITreeRoot, ICategorizableMove, ICategorizedMove
        where TTreeBranchFactoryProduct : ITreeBranch, ICategorizableMove, ICategorizedMove
        where TTreeLeafFactoryProduct : ITreeLeaf, ICategorizableMove, ICategorizedMove
        where TTreeFlowerFactoryProduct : ITreeFlower, ICategorizableMove, ICategorizedMove
        where TTreeFruitFactoryProduct : ITreeFruit, ICategorizableMove, ICategorizedMove
    {
        public AbstractTree(
            Tuple<IPlayer, IPlayer> players,
            IBoard playingBoard,
            IMove lastPlayedMove,
            IMoveRetriever playedMoveRetriever,
            IMoveCategorizer moveCategorizer,
            IMoveFacotry<TTreeRootFactoryProduct> treeRootFactory,
            IMoveFacotry<TTreeBranchFactoryProduct> treeBranchFactory,
            IMoveFacotry<TTreeLeafFactoryProduct> treeLeafFactory,
            IMoveFacotry<TTreeFlowerFactoryProduct> treeFlowerFactory,
            IMoveFacotry<TTreeFruitFactoryProduct> treeFruitFactory)
        {
            _players = players;
            PlayingBoard = playingBoard;
            _playedMoveLog = new PlayedMoveLog(playingBoard, playedMoveRetriever);
            _predictedPreviousMoveRetriever = new PredictedPreviousMoveRetriever(_playedMoveLog);

            TreeRootFactory = treeRootFactory;
            TreeBranchFactory = treeBranchFactory;
            TreeLeafFactory = treeLeafFactory;
            TreeFlowerFactory = treeFlowerFactory;
            TreeFruitFactory = treeFruitFactory;

            Root = TreeRootFactory.Produce(lastPlayedMove.Row, lastPlayedMove.Column, lastPlayedMove.Player);
            Growth(Root, GetMoveRetriever());
        }

        // currently I find one of predicted move as new root, but should it be? everytime tree need to be growthed, the old design require to make new instance of root, setup its state become the state of nonroot node also remove reference to the old one, but should it work like that? it is more efficient if the old branch become root, or should it be no root at all? No. Because the tree represent as state of game, if ... No, even in the case where root is removed or the root now will only be instance of IMove, I think I still missing the way to retrieve played move. analyzer should work on tree, bound with tree. Then should tree hold reference to the played moves? should analyzer get it as played moves retriever?
        public INextMovesPredictedMove LastPlayedMove => Root;

        protected ITreeRoot Root { get; set; }
        protected IMoveFacotry<TTreeRootFactoryProduct> TreeRootFactory { get; private set; }
        protected IMoveFacotry<TTreeBranchFactoryProduct> TreeBranchFactory { get; private set; }
        protected IMoveFacotry<TTreeLeafFactoryProduct> TreeLeafFactory { get; private set; }
        protected IMoveFacotry<TTreeFlowerFactoryProduct> TreeFlowerFactory { get; private set; }
        protected IMoveFacotry<TTreeFruitFactoryProduct> TreeFruitFactory { get; private set; }

        public IMoveRetriever PlayedMoves => GetMoveRetriever();
        protected IBoard PlayingBoard { get; private set; }

        private Tuple<IPlayer, IPlayer> _players;

        // cannot sure if the last move in move retriever is last played move, but the move order is not important to predict next possible moves after a move is played so I let the design of this method like this.
        protected abstract IEnumerable<Tuple<int, int>> PredictPossibleNextMovesPosition(IMove lastPlayedMove, IMoveRetriever boardStateBeforeLastMovePlayed);

        protected IPlayer GetNextPlayablePlayer(IMove afterThisMoveIsPlayed) => _players.Item1.Equals(afterThisMoveIsPlayed.Player) ? _players.Item1 : _players.Item2;
    }

    partial class AbstractTree<TTreeRootFactoryProduct, TTreeBranchFactoryProduct, TTreeLeafFactoryProduct, TTreeFlowerFactoryProduct, TTreeFruitFactoryProduct>
    {
        // should tree be reset at this point?
        public void Cuttings(IMove newPlayedMove)
        {
            bool playedMoveListContainsNewPlayedMove = _playedMoveLog.Any((IMove playedMove) => playedMove.Equals(newPlayedMove));
            if (!playedMoveListContainsNewPlayedMove)
            {
                // should throw something here instead of return false.
                throw new NotImplementedException();
            }

            // check if the new played move is actually a predicted move from branch. If it isn't, regenerate from this. If it is, keep this branch, prune the others, and keep generate to max height.

            bool newPlayedMoveHasBeenPredicted = Root.PredictedNextMoves.Any((IMove checkedPredictedMove) => checkedPredictedMove.Equals(newPlayedMove));

            // create new root, but hasn't remove the old root yet
            var currentPlayablePlayer = GetNextPlayablePlayer(newPlayedMove);
            var factoryProduct = TreeComponentFactory.Produce(newPlayedMove.Row, newPlayedMove.Column, currentPlayablePlayer);
            if (factoryProduct is not ITreeRoot)
            {
                throw new NotImplementedException();
            }
            var newRoot = (ITreeRoot)factoryProduct;
            var treeHeightLimitation = (PlayingBoard.WinningCondition - 2) * 2 + 1 + 2;
            if (newPlayedMoveHasBeenPredicted is false)
            {
                // remove all predicted branches from current actual root
                foreach (var predictecMove in Root.PredictableNextMoves)
                {
                    if (predictecMove is ITreeComponent treeComponent)
                    {
                        Prune(treeComponent);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                // make new played move become new root
                Root = newRoot;

                // regenerate tree from new actual root
                Growth(Root, GetMoveRetriever());
            }
            else
            {
                var node = Root.PredictedNextMoves.First((IMove checkedMove) => checkedMove.Equals(newPlayedMove));
                if (node is not ITreeComponent)
                {
                    throw new ArgumentException();
                }

                if (node is not INextMovesPredictedMove)
                {
                    throw new ArgumentException();
                }
                var predictedNextMovesFromNewRoot = ((INextMovesPredictedMove)node).PredictedNextMoves;

                // remove all other branches that is not from predicted branch
                Root.PredictableNextMoves.Clear();

                // replace old root with new root
                Root = newRoot;
                Root.PredictableNextMoves.Concat(predictedNextMovesFromNewRoot);

                // regenerate from leaf of tree (supposing that all predicted move is correct)
                Action<ITreeComponent>? generateIfNodeIsNotGenerated = null;

                generateIfNodeIsNotGenerated = (ITreeComponent treeComponent) =>
                {
                    if (treeComponent is INextMovesPredictableMove nextMovesPredictableMove
                    && treeComponent.ComponentHeight < treeHeightLimitation)
                    {
                        if (nextMovesPredictableMove.PredictableNextMoves.Count > 0)
                        {
                            foreach (var predictedMove in nextMovesPredictableMove.PredictableNextMoves)
                            {
                                if (predictedMove is ITreeComponent subNode)
                                {
                                    if (generateIfNodeIsNotGenerated is not null)
                                    {
                                        generateIfNodeIsNotGenerated(subNode);
                                    }
                                    else
                                    {
                                        throw new NotImplementedException();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (treeComponent is IPreviousMoveAccessibleMove nonRoot)
                            {
                                Growth(treeComponent, GetMoveRetriever(nonRoot));
                            }
                            else
                            {
                                Growth(treeComponent, GetMoveRetriever());
                            }
                        }
                    }
                };

                generateIfNodeIsNotGenerated(Root);
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

        public void Sowing(IMoveRetriever newSoil, IMove newSeed)
        {
            // remove the old tree
            foreach (var predictedMove in Root.PredictableNextMoves)
            {
                if (predictedMove is ITreeComponent branch)
                {
                    Prune(branch);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            // move seed to new soil
            _playedMoveLog = new PlayedMoveLog(PlayingBoard, newSoil);
            // make seed become root
            Root = TreeRootFactory.Produce(newSeed.Row, newSeed.Column, newSeed.Player);
            // make tree growth from root
            Growth(Root);
        }

        public class NodeHeightException : Exception { }
        public class UngrowthableNodeException : Exception { }
        
        internal void Growth(ITreeComponent node)
        {
            if (node is not INextMovesPredictableMove)
            {
                throw new UngrowthableNodeException();
            }

            var nextMovesPredictableMove = (INextMovesPredictableMove)node;
            var treeHeightLimitation = (PlayingBoard.WinningCondition - 2) * 2 + 1 + 2;

            if (!(node.ComponentHeight < treeHeightLimitation))
            {
                throw new NodeHeightException();
            }
            IMoveRetriever moveRetriever;

            if (node is IPreviousMoveAccessibleMove previousMoveAccessibleMove)
            {
                moveRetriever = GetMoveRetriever(previousMoveAccessibleMove);
            }
            else
            {
                moveRetriever = GetMoveRetriever();
            }

            var predictedNextPossibleMovePositions = PredictPossibleNextMovesPosition(nextMovesPredictableMove, moveRetriever);
            var nextPlayablePlayer = GetNextPlayablePlayer(node);
            foreach (var position in predictedNextPossibleMovePositions)
            {
                var row = position.Item1;
                var column = position.Item2;
                var player = nextPlayablePlayer;
                var newPredictedMove = TreeComponentFactory.Produce(row, column, player);
                newPredictedMove.ComponentHeight = node.ComponentHeight + 1;
                nextMovesPredictableMove.PredictableNextMoves.Add(newPredictedMove);
            }
            foreach (var predictedMove in nextMovesPredictableMove.PredictableNextMoves)
            {
                if (predictedMove is not ITreeComponent)
                {
                    throw new NotImplementedException();
                }

                var newPredictedTreeComponent = (ITreeComponent)predictedMove;

                try
                {
                    Growth(newPredictedTreeComponent);
                }
                catch (NodeHeightException)
                {

                }
                catch (UngrowthableNodeException)
                {

                }
            }
        }
    }

    partial class AbstractTree<TTreeRootFactoryProduct, TTreeBranchFactoryProduct, TTreeLeafFactoryProduct, TTreeFlowerFactoryProduct, TTreeFruitFactoryProduct>
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
                throw new NotImplementedException();
            }
        }

        internal class PredictedPreviousMoveEnumerator : IEnumerator<IMove>
        {
            public PredictedPreviousMoveEnumerator(IEnumerator<IMove> playedMoveEnumerator, IPreviousMoveAccessibleMove predictedPreviousMoveAccessibleMove)
            {
                _playedMoveEnumerator = playedMoveEnumerator;
                _originalPredictedPreviousMoveAccessibleMove = predictedPreviousMoveAccessibleMove;
                _currentPredictedMove = _originalPredictedPreviousMoveAccessibleMove;
                _hasTraveled = false;
            }

            private IPreviousMoveAccessibleMove _originalPredictedPreviousMoveAccessibleMove;
            private IMove _currentPredictedMove;
            private bool _hasTraveled;
            private IEnumerator<IMove> _playedMoveEnumerator;

            private bool disposedValue;

            public IMove Current => _hasTraveled ? _currentPredictedMove : _playedMoveEnumerator.Current;

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (!_hasTraveled
                    || _playedMoveEnumerator.MoveNext())
                {
                    if (_currentPredictedMove is IPreviousMoveAccessibleMove previousMoveAccessibleMove)
                    {
                        _currentPredictedMove = previousMoveAccessibleMove.PreviousMove;
                        _hasTraveled = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }

            public void Reset()
            {
                _hasTraveled = false;
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

    public class UnoptimizedTree1<TTreeComponent> : AbstractTree<TTreeComponent>
        where TTreeComponent : ITreeComponent
    {
        public UnoptimizedTree1(Tuple<IPlayer, IPlayer> players, IMoveFacotry<TTreeComponent> treeComponentFactory, IBoard playingBoard, IMoveRetriever playedMoveRetriever) : base(players, treeComponentFactory, playingBoard, playedMoveRetriever)
        {
        }

        protected override IEnumerable<Tuple<int, int>> PredictPossibleNextMovesPosition(IMove afterThisPlayedMoveFromOpponent, IMoveRetriever withThosePreviousPlayedMoves)
        {
            var winningCondition = PlayingBoard.WinningCondition;
            var numberOfRows = PlayingBoard.NumberOfRows;
            var numberOfColumns = PlayingBoard.NumberOfColumns;
            var opponent = GetNextPlayablePlayer(afterThisPlayedMoveFromOpponent);
            var playedMoveRetriever = afterThisPlayedMoveFromOpponent is IPreviousMoveAccessibleMove previousMoveAccessible ? GetMoveRetriever(previousMoveAccessible) : GetMoveRetriever();

            var mockOfIEqualityComparer = new Mock<IEqualityComparer<Tuple<int, int>>>();
            mockOfIEqualityComparer
                .Setup((comparer) => comparer.Equals(It.IsAny<Tuple<int, int>>(), It.IsAny<Tuple<int, int>>()))
                .Returns<Tuple<int, int>, Tuple<int, int>>((position, otherPosition) =>
                {
                    return position.Item1 == otherPosition.Item1
                        && position.Item2 == otherPosition.Item2;
                });

            var possibleMeaningLocations = new HashSet<Tuple<int, int>>(mockOfIEqualityComparer.Object);
           
            foreach (var playedMove in playedMoveRetriever)
            {
                var minRow = playedMove.Row - (winningCondition - 1);
                minRow = minRow >= 0 ? minRow : 0;
                var maxRow = playedMove.Row + (winningCondition - 1);
                maxRow = maxRow <= numberOfRows ? maxRow : numberOfRows;

                var minColumn = playedMove.Column - (winningCondition - 1);
                minColumn = minColumn >= 0 ? minColumn : 0;
                var maxColumn = playedMove.Column + (winningCondition - 1);
                maxColumn = maxColumn <= numberOfColumns ? maxColumn : numberOfColumns;

                for (int row = minRow; row <= maxRow; row++)
                {
                    for (int column = minColumn; column <= maxColumn; column++)
                    {
                        possibleMeaningLocations.Add(new Tuple<int, int>(row, column));
                    }
                }
            }
            return possibleMeaningLocations;
        }
    }
}