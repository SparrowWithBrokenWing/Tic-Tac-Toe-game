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
            _players = players;
            TreeComponentFactory = treeComponentFactory;
            PlayingBoard = playingBoard;
            _playedMoveRetriever = playedMoveRetriever;
            _predictedPreviousMoveRetriever = new PredictedPreviousMoveRetriever(_playedMoveRetriever);

            var lastPlayedMove = playedMoveRetriever.Last();
            if (treeComponentFactory.Produce(lastPlayedMove.Row, lastPlayedMove.Column, lastPlayedMove.Player) is ITreeRoot newRoot)
            {
                ActualRoot = newRoot;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public INextMovesPredictedMove Root => ActualRoot;

        protected ITreeRoot ActualRoot { get; set; }
        protected IMoveFacotry<TFactoryProduct> TreeComponentFactory { get; private set; }
        protected IBoard PlayingBoard { get; private set; }

        private Tuple<IPlayer, IPlayer> _players;

        protected abstract IEnumerable<Tuple<int, int>> PredictPossibleNextMovesPosition(IMove afterThisPlayedMoveFromOpponent, IMoveRetriever withThesePreviousPlayedMoves);

        protected IPlayer GetNextPlayablePlayer(IMove afterThisMoveIsPlayed) => _players.Item1.Equals(afterThisMoveIsPlayed.Player) ? _players.Item1 : _players.Item2;

    }

    partial class AbstractTree<TFactoryProduct>
    {
        // should tree be reset at this point?
        public void Growth(IMove newPlayedMove)
        {
            bool playedMoveListContainsNewPlayedMove = _playedMoveRetriever.Any((IMove playedMove) => playedMove.Equals(newPlayedMove));
            if (!playedMoveListContainsNewPlayedMove)
            {
                // should throw something here instead of return false.
                throw new NotImplementedException();
            }

            // check if the new played move is actually a predicted move from branch. If it isn't, regenerate from this. If it is, keep this branch, prune the others, and keep generate to max height.

            bool newPlayedMoveHasBeenPredicted = ActualRoot.PredictedNextMoves.Any((IMove checkedPredictedMove) => checkedPredictedMove.Equals(newPlayedMove));

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
                foreach (var predictecMove in ActualRoot.PredictableNextMoves)
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
                ActualRoot = newRoot;

                // regenerate tree from new actual root
                Generate(ActualRoot, GetMoveRetriever());
            }
            else
            {
                var node = ActualRoot.PredictedNextMoves.First((IMove checkedMove) => checkedMove.Equals(newPlayedMove));
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
                ActualRoot.PredictableNextMoves.Clear();

                // replace old root with new root
                ActualRoot = newRoot;
                ActualRoot.PredictableNextMoves.Concat(predictedNextMovesFromNewRoot);

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
                                Generate(treeComponent, GetMoveRetriever(nonRoot));
                            }
                            else
                            {
                                Generate(treeComponent, GetMoveRetriever());
                            }
                        }
                    }
                };

                generateIfNodeIsNotGenerated(ActualRoot);
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

        public void Cuttings(IMoveRetriever newSoil)
        {
            foreach (var predictedMove in ActualRoot.PredictableNextMoves)
            {
                if (predictedMove is ITreeComponent treeComponent)
                {
                    Prune(treeComponent);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            _playedMoveRetriever = newSoil;
        }

        internal void Generate(ITreeComponent node, IMoveRetriever playedMoveRetriever)
        {
            if (node is not INextMovesPredictableMove)
            {
                throw new ArgumentException();
            }

            var nextMovesPredictableMove = (INextMovesPredictableMove)node;
            var treeHeightLimitation = (PlayingBoard.WinningCondition - 2) * 2 + 1 + 2;

            if (!(node.ComponentHeight < treeHeightLimitation))
            {
                throw new NotImplementedException();
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

            var predictedNextMovePositions = PredictPossibleNextMovesPosition(nextMovesPredictableMove, moveRetriever);
            var opponentOfPlayerWhoPlayedNode = GetNextPlayablePlayer(node);
            foreach (var position in predictedNextMovePositions)
            {
                var row = position.Item1;
                var column = position.Item2;
                var player = opponentOfPlayerWhoPlayedNode;
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
                    Generate(newPredictedTreeComponent, moveRetriever);
                }
                catch (NotImplementedException)
                {

                }
            }
        }
    }

    partial class AbstractTree<TFactoryProduct>
    {
        private IMoveRetriever _playedMoveRetriever;

        internal PredictedPreviousMoveRetriever _predictedPreviousMoveRetriever;

        protected IMoveRetriever GetMoveRetriever() => _playedMoveRetriever;
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