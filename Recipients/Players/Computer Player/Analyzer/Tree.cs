using Moq;
using System.Collections;

namespace ComputerPlayer
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

    // need to define an definition of IMoveRetriever that work base on another move retriever. 
    public class Tree
    {
        public Tree(IBoard board, IMatch match, IMoveRetriever moveRetriever, ITurnLog turnLog, IMoveCategorizer moveCategorizer, IOpponentNextMovePossibilityChecker opponentNextMovePossibilityChecker)
        {
            _Board = board;
            _Match = match;
            _MoveRetriever = new PreviousMoveRetriever(moveRetriever);
            _TurnLog = turnLog;
            _MoveCategorizer = moveCategorizer;
            _OpponentNextMovePossibilityChecker = opponentNextMovePossibilityChecker;

            var lastTurn = _TurnLog[_TurnLog.Count - 1];
            var lastMove = lastTurn[lastTurn.Count - 1];
            if (lastMove is not null)
            {
                root = new RootMove(lastMove);
            }
            else
            {
                throw new ArgumentException("Cannot find the last played move.");
            }
            UpdateTree(root);
        }

        private RootMove root;
        public INextMovesPredictedMove Root => root;
        protected INextMovesPredictableMove _Root => root;

        protected IBoard _Board { get; private set; }

        protected IMatch _Match { get; private set; }

        protected PreviousMoveRetriever _MoveRetriever { get; private set; }

        protected ITurnLog _TurnLog { get; private set; }

        protected IMoveCategorizer _MoveCategorizer { get; private set; }

        protected IOpponentNextMovePossibilityChecker _OpponentNextMovePossibilityChecker { get; private set; }

        private int GetBranchHeight(IPreviousMoveAccessibleMove referenceablePreviousMoveMove)
        {
            IMove currentCheckingMove = referenceablePreviousMoveMove;
            int result = 0;
            while (currentCheckingMove is IPreviousMoveAccessibleMove previousMoveReferenceableMove)
            {
                result++;
                currentCheckingMove = previousMoveReferenceableMove;
            }
            return result;
        }

        protected class PreviousMoveRetriever : IMoveRetriever
        {
            public PreviousMoveRetriever(IMoveRetriever moveRetriever)
            {
                _MoveRetriever = moveRetriever;
            }

            protected IMoveRetriever _MoveRetriever { get; private set; }
            public IPreviousMoveAccessibleMove? CurrentPreviousMoveAccessibleMove { protected get; set; }

            public IMove? Retrieve(int row, int column)
            {
                IMove? result = _MoveRetriever.Retrieve(row, column);
                if (result is null)
                {
                    var current = CurrentPreviousMoveAccessibleMove;
                    while (current is IPreviousMoveAccessibleMove)
                    {
                        if (current.Row == row && current.Column == column)
                        {
                            result = current;
                            break;
                        }
                    }
                }
                return result;
            }

            public IEnumerator<IMove> GetEnumerator()
            {
                return CurrentPreviousMoveAccessibleMove is not null
                    ? new CustomEnumerator(_MoveRetriever.GetEnumerator(), CurrentPreviousMoveAccessibleMove)
                    : _MoveRetriever.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return CurrentPreviousMoveAccessibleMove is not null
                    ? new CustomEnumerator(_MoveRetriever.GetEnumerator(), CurrentPreviousMoveAccessibleMove)
                    : _MoveRetriever.GetEnumerator();
            }

            protected struct CustomEnumerator : IEnumerator, IEnumerator<IMove>
            {
                public CustomEnumerator(IEnumerator<IMove> enumerator, IPreviousMoveAccessibleMove previousMoveAccessibleMove)
                {
                    _enumerator = enumerator;
                    _lastPreviousMoveAccessibleMove = previousMoveAccessibleMove;
                    _hasChanged = false;
                    _current = _enumerator.Current;
                }

                private IEnumerator<IMove> _enumerator { get; set; }
                private IPreviousMoveAccessibleMove _lastPreviousMoveAccessibleMove;
                private bool _hasChanged;
                private IMove? _current;

                public IMove Current => _current is not null ? _current : throw new NotImplementedException();
                object IEnumerator.Current => _current is not null ? _current : throw new NotImplementedException();

                public bool MoveNext()
                {
                    if (_enumerator.MoveNext())
                    {
                        _current = _enumerator.Current;
                        return true;
                    }
                    else
                    {
                        if (!_hasChanged)
                        {
                            _current = _lastPreviousMoveAccessibleMove;
                            _hasChanged = true;
                        }

                        if (_current is IPreviousMoveAccessibleMove previousMoveAccessibleMove)
                        {
                            _current = previousMoveAccessibleMove.PreviousMove;
                            return true;
                        }
                        else
                        {
                            _hasChanged = false;
                            return false;
                        }
                    }
                }

                public void Reset()
                {
                    _enumerator.Reset();
                    _current = _enumerator.Current;
                }

                public void Dispose()
                {
                    _enumerator.Dispose();
                    _current = null;
                }
            }
        }

        protected class RootMove : INextMovesPredictableMove, INextMovesPredictedMove, ICategorizableMove, ICategorizedMove
        {
            public RootMove(int row, int column, IPlayer player, IEnumerable<IMove> predictedNextmoves, IEnumerable<IMoveType> moveTypes)
            {
                this.Row = row;
                this.Column = column;
                this.Player = player;
                this._nextMoves = new List<IMove>(predictedNextmoves);
                this._moveTypes = new List<IMoveType>(moveTypes);
            }

            public RootMove(IMove otherMove, IEnumerable<IMove> predictedNextMoves, IEnumerable<IMoveType> moveTypes)
                : this(otherMove.Row, otherMove.Column, otherMove.Player, predictedNextMoves, moveTypes)
            {
            }

            public RootMove(int row, int column, IPlayer player)
                : this(row, column, player, Enumerable.Empty<IMove>(), Enumerable.Empty<IMoveType>())
            {

            }

            public RootMove(IMove otherMove)
                : this(otherMove, Enumerable.Empty<IMove>(), Enumerable.Empty<IMoveType>())
            {

            }

            public int Row { get; private set; }
            public int Column { get; private set; }
            public IPlayer Player { get; private set; }

            private IList<IMove> _nextMoves;
            public ICollection<IMove> PredictableNextMoves => _nextMoves;
            public IEnumerable<IMove> PredictedNextMoves => _nextMoves;

            private IList<IMoveType> _moveTypes;
            public ICollection<IMoveType> CategorizableMoveTypes => _moveTypes;
            public IEnumerable<IMoveType> CategorizedMoveTypes => _moveTypes;

            public bool Equals(IMove? other)
            {
                return other is not null
                    && this.Row.Equals(other.Row)
                    && this.Column.Equals(other.Column)
                    && this.Player.Equals(other.Player);
            }
        }

        protected class TrunkMove : IPreviousMoveAccessibleMove, IPreviousMoveSpecifiableMove, INextMovesPredictedMove, INextMovesPredictableMove, ICategorizableMove, ICategorizedMove
        {
            public TrunkMove(int row, int column, IPlayer player, IMove previousMove, IEnumerable<IMove> predictedNextMoves, IEnumerable<IMoveType> moveTypes)
            {
                this.Row = row;
                this.Column = column;
                this.Player = player;
                this.PreviousMove = previousMove;
                this._nextMoves = new List<IMove>(predictedNextMoves);
                this._moveTypes = new List<IMoveType>(moveTypes);
            }

            public TrunkMove(IMove otherMove, IMove previousMove, IEnumerable<IMove> predictedNextMoves, IEnumerable<IMoveType> moveTypes)
                : this(otherMove.Row, otherMove.Column, otherMove.Player, previousMove, predictedNextMoves, moveTypes)
            {
            }

            public TrunkMove(int row, int column, IPlayer player, IMove previousMove)
                : this(row, column, player, previousMove, Enumerable.Empty<IMove>(), Enumerable.Empty<IMoveType>())
            {

            }

            public TrunkMove(IMove otherMove, IMove previousMove)
                : this(otherMove, previousMove, Enumerable.Empty<IMove>(), Enumerable.Empty<IMoveType>())
            {

            }

            public int Row { get; private set; }
            public int Column { get; private set; }
            public IPlayer Player { get; private set; }

            public IMove PreviousMove { get; set; }

            private IList<IMove> _nextMoves;
            public ICollection<IMove> PredictableNextMoves => _nextMoves;
            public IEnumerable<IMove> PredictedNextMoves => _nextMoves;

            private IList<IMoveType> _moveTypes;
            public ICollection<IMoveType> CategorizableMoveTypes => _moveTypes;
            public IEnumerable<IMoveType> CategorizedMoveTypes => _moveTypes;

            public bool Equals(IMove? other)
            {
                return other is not null
                    && this.Row.Equals(other.Row)
                    && this.Column.Equals(other.Column)
                    && this.Player.Equals(other.Player);
            }
        }

        protected class LeafMove : IPreviousMoveAccessibleMove, IPreviousMoveSpecifiableMove, ICategorizableMove, ICategorizedMove
        {
            public LeafMove(int row, int column, IPlayer player, IMove previousMove, IEnumerable<IMoveType> moveTypes)
            {
                this.Row = row;
                this.Column = column;
                this.Player = player;
                this.PreviousMove = previousMove;
                this._moveTypes = new List<IMoveType>(moveTypes);
            }

            public LeafMove(IMove otherMove, IMove previousMove, IEnumerable<IMoveType> moveTypes)
                : this(otherMove.Row, otherMove.Column, otherMove.Player, previousMove, moveTypes)
            {
            }

            public LeafMove(int row, int column, IPlayer player, IMove previousMove)
                : this(row, column, player, previousMove, Enumerable.Empty<IMoveType>())
            {

            }

            public LeafMove(IMove otherMove, IMove previousMove)
                : this(otherMove, previousMove, Enumerable.Empty<IMoveType>())
            {

            }

            public int Row { get; private set; }
            public int Column { get; private set; }
            public IPlayer Player { get; private set; }

            public IMove PreviousMove { get; set; }

            private IList<IMoveType> _moveTypes;
            public ICollection<IMoveType> CategorizableMoveTypes => _moveTypes;
            public IEnumerable<IMoveType> CategorizedMoveTypes => _moveTypes;

            public bool Equals(IMove? other)
            {
                return other is not null
                    && this.Row.Equals(other.Row)
                    && this.Column.Equals(other.Column)
                    && this.Player.Equals(other.Player);
            }
        }

        protected IPlayer GetOpponentOf(IPlayer player)
        {
            using (var enumerator = _Match.Players.GetEnumerator())
            {
                enumerator.Reset();
                IPlayer opponent = enumerator.Current;
                while (true)
                {
                    var current = enumerator.Current;
                    if (player.Equals(current))
                    {
                        try
                        {
                            enumerator.MoveNext();
                            opponent = enumerator.Current;
                        }
                        catch (InvalidOperationException)
                        {
                            enumerator.Reset();
                            opponent = enumerator.Current;
                        }
                        break;
                    }
                }
                return opponent;
            }
        }
        // I have tried to generate the next possible moves in a correct way, but I cannot do that. There are so many things need to ensure, and whatever how did I sort the played moves, I'd have to find the other moves that in range to with each checked move, which make it useless to continue to find a better way.
        protected Task PredictOpponentsNextMoves(INextMovesPredictableMove nextMovesPredictableMove, IMoveRetriever playedMoveRetriever, IBoard playingBoard)
        {
            if (nextMovesPredictableMove is LeafMove)
            {
                return Task.CompletedTask;
            }

            var winningCondition = playingBoard.WinningCondition;
            var numberOfRows = playingBoard.NumberOfRows;
            var numberOfColumns = playingBoard.NumberOfColumns;
            var opponent = GetOpponentOf(nextMovesPredictableMove.Player);

            var mockOfIEqualityComparer = new Mock<IEqualityComparer<IMove>>();
            mockOfIEqualityComparer
                .Setup((comparer) => comparer.Equals(It.IsAny<IMove>(), It.IsAny<IMove>()))
                .Returns<IMove, IMove>((move, otherMove) =>
                {
                    return move.Row == otherMove.Row
                        && move.Column == otherMove.Column;
                });

            var possibleMeaningMoves = new HashSet<IMove>(mockOfIEqualityComparer.Object);

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
                        var mockOfMove = new Mock<IMove>();
                        mockOfMove
                            .Setup((move) => move.Row)
                            .Returns(row);
                        mockOfMove
                            .Setup((move) => move.Column)
                            .Returns(column);
                        mockOfMove
                            .Setup((move) => move.Player)
                            .Returns(opponent);


                        if (nextMovesPredictableMove is ICategorizableMove categorizableMove)
                        {
                            var predictedMoveTypes = _MoveCategorizer.Categorize(mockOfMove.Object, playedMoveRetriever, playingBoard);
                            mockOfMove.As<ICategorizedMove>().Setup((move) => move.CategorizedMoveTypes).Returns(predictedMoveTypes);
                            if (_OpponentNextMovePossibilityChecker.IsPossible(mockOfMove.As<ICategorizedMove>().Object))
                            {
                                possibleMeaningMoves.Add(new TrunkMove(mockOfMove.Object, nextMovesPredictableMove, Enumerable.Empty<IMove>(), predictedMoveTypes));
                            }
                            else
                            {
                                possibleMeaningMoves.Add(new LeafMove(mockOfMove.Object, nextMovesPredictableMove, predictedMoveTypes));
                            }
                        }
                    }
                }
            }

            foreach (var possibleMeaningMove in possibleMeaningMoves)
            {
                nextMovesPredictableMove.PredictableNextMoves.Add(possibleMeaningMove);
            }

            return Task.CompletedTask;
        }

        public void UpdateTree(IMove newPlayedMove)
        {
            // remove all branches that is uncovered anymore, assume that the new played move has been predicted earlier.
            foreach (var move in _Root.PredictableNextMoves)
            {
                if (move.Equals(newPlayedMove))
                {
                    var temp = move;
                    _Root.PredictableNextMoves.Clear();
                    _Root.PredictableNextMoves.Add(temp);
                    break;
                }
            }

            // predict next opponent possible moves
            var current = _Root;
            Func<INextMovesPredictableMove, Task>? process = null;
            process = async (INextMovesPredictableMove nextMovesPredictableMove) =>
            {
                if (nextMovesPredictableMove is IPreviousMoveAccessibleMove previousMoveAccesibleMove)
                {
                    if (GetBranchHeight(previousMoveAccesibleMove) < (_Board.WinningCondition - 1) * 2 + 1)
                    {
                        if (nextMovesPredictableMove.PredictableNextMoves.Count == 0)
                        {
                            _MoveRetriever.CurrentPreviousMoveAccessibleMove = previousMoveAccesibleMove;
                            await PredictOpponentsNextMoves(nextMovesPredictableMove, _MoveRetriever, _Board);
                        }
                        var tasks = new List<Task>();
                        foreach (var subMove in nextMovesPredictableMove.PredictableNextMoves)
                        {
                            if (subMove is INextMovesPredictableMove nextMovesPredictableSubMove)
                            {
                                if (process is not null)
                                {
                                    tasks.Add(process(nextMovesPredictableSubMove));
                                }
                            }
                        }
                        await Task.WhenAll(tasks);
                    }
                    else
                    {
                        await Task.CompletedTask;
                    }
                }
                else
                {
                    throw new ArgumentException();
                }
            };

            if (process is not null)
            {
                Task.WaitAll(process(root));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void ResetTree()
        {
            var lastTurn = _TurnLog[_TurnLog.Count - 1];
            var lastMove = lastTurn[lastTurn.Count - 1];
            if (lastMove is not null)
            {
                root = new RootMove(lastMove);
            }
            else
            {
                throw new InvalidOperationException();
            }
            UpdateTree(root);
        }
    }
}
