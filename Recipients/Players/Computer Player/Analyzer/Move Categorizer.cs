namespace ComputerPlayer.Analyzer
{
    public interface IMoveType { }

    public interface IPossibleMoveType : IMoveType { }
    public interface ITacticalMoveType : IPossibleMoveType { }

    public interface IOffensiveMoveType  : ITacticalMoveType { }
    public interface IForkMoveType  : IOffensiveMoveType  { }
    public interface IWinningMoveType  : IOffensiveMoveType  { }

    public interface IDefensiveMoveType  : ITacticalMoveType  { }
    public interface IBlockForkMoveType  : IDefensiveMoveType  { }
    public interface IBlockWinningMoveType  : IDefensiveMoveType  { }

    public interface ICategorizableMove : IMove
    {
        public ICollection<IMoveType> Categories { get; }
    }

    public interface ICategorizedMove : IMove
    {
        public IEnumerable<IMoveType> Categories { get; }
    }

    public class MoveCategorizer
    {

    }
    public interface ICategorizer
    {
        //public IEnumerable<IMoveType> Categorize(IMove move, IMoveTracker moveRetriever);
        public IEnumerable<IMoveType> Categorize(IMove move, IMoveRetriever moveRetriever, IBoard playingBoard);
    }

    public class CompositeCategorizer : ICategorizer
    {
        public CompositeCategorizer(IEnumerable<ICategorizer> categorizers)
        {
            _Categorizers = categorizers;
        }

        protected IEnumerable<ICategorizer> _Categorizers { get; private set; }

        public IEnumerable<IMoveType> Categorize(IMove move, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            var result = new HashSet<IMoveType>();
            foreach (var categorizer in _Categorizers)
            {
                var moveTypes = categorizer.Categorize(move, moveRetriever, playingBoard);
                foreach (var moveType in moveTypes)
                {
                    result.Add(moveType);
                }
            }
            return result;
        }
    }

    public class PossibleMoveTypeCategorizer : ICategorizer
    {
        public virtual IEnumerable<IMoveType> Categorize(IMove move, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            var result = new List<IMoveType>();
            var numberOfRows = playingBoard.NumberOfRows;
            var numberOfColumns = playingBoard.NumberOfColumns;

            var isItAnUnplayedMove = (IMove move) =>
            {
                foreach (var playedMove in moveRetriever)
                {
                    if (move.Row == playedMove.Row
                    && move.Column == playedMove.Column)
                    {
                        return false;
                    }
                }
                return true;
            };

            var isItInAnAllowedRangeMove = (IMove move) =>
            {
                return move.Row > 0 && move.Row <= numberOfRows
                && move.Column > 0 && move.Column <= numberOfColumns;
            };

            if (isItAnUnplayedMove(move)
                && isItInAnAllowedRangeMove(move))
            {
                result.Add(new PossibleMoveType());
            }

            return result;
        }

        protected class PossibleMoveType : IPossibleMoveType { }
    }

    public class TacticalMoveTypeCategorizer : PossibleMoveTypeCategorizer
    {
        public override IEnumerable<IMoveType> Categorize(IMove move, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            var result = new List<IMoveType>(base.Categorize(move, moveRetriever, playingBoard));
            var winningCondition = playingBoard.WinningCondition;

            var isThereAnyOtherMoveInsideTheSquareWithLengthAsTheWinningCondition = (IMove move) =>
            {
                foreach (var playedMove in moveRetriever)
                {
                    if ((Math.Abs(playedMove.Row - move.Row) < (winningCondition - 1))
                    && (Math.Abs(playedMove.Column - move.Column) < (winningCondition - 1)))
                    {
                        return true;
                    }
                }
                return false;
            };

            if (isThereAnyOtherMoveInsideTheSquareWithLengthAsTheWinningCondition(move))
            {
                result.Add(new TacticalMoveType());
            }

            return result;
        }

        protected class TacticalMoveType : ITacticalMoveType { }
    }

    public class OffensiveMoveTypeCategorizer : TacticalMoveTypeCategorizer
    {
        public override IEnumerable<IMoveType> Categorize(IMove move, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            var result = new List<IMoveType>(base.Categorize(move, moveRetriever, playingBoard));
            var winningCondition = playingBoard.WinningCondition;

            var isThereAnyOtherMoveFromSamePlayerInsideTheSquareWithLengthAsTheWinningCondition = (IMove move) =>
            {
                foreach (var playedMove in moveRetriever)
                {
                    if (move.Player.Equals(playedMove.Player)
                    && Math.Abs(playedMove.Row - move.Row) < winningCondition
                    && Math.Abs(playedMove.Column - move.Column) < winningCondition)
                    {
                        return true;
                    }
                }
                return false;
            };

            if (isThereAnyOtherMoveFromSamePlayerInsideTheSquareWithLengthAsTheWinningCondition(move))
            {
                result.Add(new OffensiveMoveType());
            }

            return result;
        }

        protected class OffensiveMoveType : ITacticalMoveType { }
    }

    public class ForkMoveTypeCategorizer : OffensiveMoveTypeCategorizer
    {
        // need to change how to check. The check will come from back and front of checked move (2 tasks). Those tasks will change the value of satisfied move variable. If satisified move variable reach a value, those task will be cancel or will return.
        public override IEnumerable<IMoveType> Categorize(IMove move, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            var winningCondition = playingBoard.WinningCondition;
            var numberOfRows = playingBoard.NumberOfRows;
            var numberOfColumns = playingBoard.NumberOfColumns;

            if (!(winningCondition - 2 > 0))
            {
                throw new ArgumentException();
            }

            var result = new List<IMoveType>(base.Categorize(move, moveRetriever, playingBoard));

            var numberOfNeededSatisifedLineSegment = 2;

            var check = async (
                Func<int, int> getNextFrontMoveRowIndex,
                Func<int, int> getNextFrontMoveColumnIndex,
                Func<int, int> getNextBehindMoveRowIndex,
                Func<int, int> getNextBehindMoveColumnIndex,
                CancellationToken cancellationToken
                ) =>
            {
                var satisfiedCheckedMove = 0;
                var isUnplayedMoveIgnored = false;
                var cancellationTokenSource1 = new CancellationTokenSource();
                var cancellationTokenSource2 = new CancellationTokenSource();

                var checkPlayerofMove = (
                    int limitation,
                    Func<int, int> getNextCheckedMoveRowIndex,
                    Func<int, int> getNextCheckedMoveColumnIndex,
                    Action whenFoundUnplayedMove,
                    Action whenFoundPlayerMove,
                    Action whenFoundOpponentMove,
                    CancellationToken cancellationToken) =>
                {
                    var checkedTimes = 0;
                    var currentCheckedMoveRowIndex = move.Row;
                    var currentChecedkMoveColumnIndex = move.Column;
                    IMove? currentCheckedMove = null;

                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        checkedTimes++;

                        if (checkedTimes >= limitation)
                        {
                            break;
                        }

                        currentCheckedMoveRowIndex = getNextCheckedMoveRowIndex(currentCheckedMoveRowIndex);
                        currentChecedkMoveColumnIndex = getNextCheckedMoveColumnIndex(currentChecedkMoveColumnIndex);
                        currentCheckedMove = moveRetriever.Retrieve(currentCheckedMoveRowIndex, currentChecedkMoveColumnIndex);

                        if (currentCheckedMove is null)
                        {
                            whenFoundUnplayedMove();
                        }
                        else

                        {
                            if (move.Player.Equals(currentCheckedMove.Player))
                            {
                                whenFoundPlayerMove();
                            }
                            else
                            {
                                whenFoundOpponentMove();
                            }
                        }
                    }

                    return Task.CompletedTask;
                };

                var handleFoundLocker = new object();
                var numberOfNeededSatisfiedCheckedMove = winningCondition - 2;
                var searchRangeLimitation = winningCondition - 1;
                // should be search for direction of vector
                var searchFront = checkPlayerofMove(
                    searchRangeLimitation,
                    getNextFrontMoveRowIndex,
                    getNextFrontMoveColumnIndex,
                    () =>
                    {
                        // that unplayed move is the next winning move.
                        lock (handleFoundLocker)
                        {
                            if (isUnplayedMoveIgnored)
                            {
                                cancellationTokenSource1.Cancel();
                            }
                            isUnplayedMoveIgnored = true;
                        }
                    },
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            ++satisfiedCheckedMove;
                            if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove)
                            {
                                cancellationTokenSource1.Cancel();
                                cancellationTokenSource2.Cancel();
                            }
                        }
                    },
                    () => cancellationTokenSource1.Cancel(),
                    cancellationTokenSource1.Token);
                var searchBehind = checkPlayerofMove(
                    searchRangeLimitation,
                    getNextBehindMoveRowIndex,
                    getNextBehindMoveColumnIndex,
                    () =>
                    {
                        // that unplayed move is the next winning move.
                        lock (handleFoundLocker)
                        {
                            if (isUnplayedMoveIgnored)
                            {
                                cancellationTokenSource2.Cancel();
                            }
                            isUnplayedMoveIgnored = true;
                        }
                    },
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            ++satisfiedCheckedMove;
                            if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove)
                            {
                                cancellationTokenSource1.Cancel();
                                cancellationTokenSource2.Cancel();
                            }
                        }
                    },
                    () => cancellationTokenSource2.Cancel(),
                    cancellationTokenSource2.Token);
                await Task.WhenAll(searchFront, searchBehind);
                if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove
                && isUnplayedMoveIgnored)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            };

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var numberOfSatisfiedLineSegment = 0;

            var handleFinishedCheckTaskLocker = new object();
            var handleFinishedCheckTask = (Task<bool> checkTask) =>
            {
                lock (handleFinishedCheckTaskLocker)
                {
                    if (checkTask.Result)
                    {
                        numberOfSatisfiedLineSegment++;
                    }
                    if (numberOfSatisfiedLineSegment >= numberOfNeededSatisifedLineSegment)
                    {
                        cancellationTokenSource.Cancel();
                    }
                    return Task.FromResult(checkTask.Result);
                }
            };

            var checkVerticalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkHorizontalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkLeftDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkRightDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);

            Task.WaitAll(checkVerticalLine, checkHorizontalLine, checkLeftDiagonal, checkRightDiagonal);

            if (numberOfSatisfiedLineSegment >= 2)
            {
                result.Add(new ForkMoveType());
            }

            return result;
        }

        protected class ForkMoveType : IForkMoveType { }
    }

    public class WinningMoveTypeCategorizer : OffensiveMoveTypeCategorizer
    {
        public override IEnumerable<IMoveType> Categorize(IMove move, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            if (!(playingBoard.WinningCondition - 2 > 0))
            {
                throw new ArgumentException();
            }

            var result = new List<IMoveType>(base.Categorize(move, moveRetriever, playingBoard));

            var winningCondition = playingBoard.WinningCondition;
            var numberOfRows = playingBoard.NumberOfRows;
            var numberOfColumns = playingBoard.NumberOfColumns;
            var numberOfNeededSatisfiedLineSegment = 1;

            // need a better name
            var check = async (
                Func<int, int> getNextFrontMoveRowIndex,
                Func<int, int> getNextFrontMoveColumnIndex,
                Func<int, int> getNextBehindMoveRowIndex,
                Func<int, int> getNextBehindMoveColumnIndex,
                CancellationToken cancellationToken
                ) =>
            {
                var satisfiedCheckedMove = 0;
                var cancellationTokenSource1 = new CancellationTokenSource();
                var cancellationTokenSource2 = new CancellationTokenSource();

                var checkPlayerofMove = (
                    int limitation,
                    Func<int, int> getNextCheckedMoveRowIndex,
                    Func<int, int> getNextCheckedMoveColumnIndex,
                    Action whenFoundUnplayedMove,
                    Action whenFoundPlayerMove,
                    Action whenFoundOpponentMove,
                    CancellationToken cancellationToken) =>
                {
                    var checkedTimes = 0;
                    var currentCheckedMoveRowIndex = move.Row;
                    var currentChecedkMoveColumnIndex = move.Column;
                    IMove? currentCheckedMove = null;

                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        checkedTimes++;

                        if (checkedTimes >= limitation)
                        {
                            break;
                        }

                        currentCheckedMoveRowIndex = getNextCheckedMoveRowIndex(currentCheckedMoveRowIndex);
                        currentChecedkMoveColumnIndex = getNextCheckedMoveColumnIndex(currentChecedkMoveColumnIndex);
                        currentCheckedMove = moveRetriever.Retrieve(currentCheckedMoveRowIndex, currentChecedkMoveColumnIndex);

                        if (currentCheckedMove is null)
                        {
                            whenFoundUnplayedMove();
                        }
                        else

                        {
                            if (move.Player.Equals(currentCheckedMove.Player))
                            {
                                whenFoundPlayerMove();
                            }
                            else
                            {
                                whenFoundOpponentMove();
                            }
                        }
                    }

                    return Task.CompletedTask;
                };

                var searchRangeLimitation = winningCondition - 1;
                var numberOfNeededSatisfiedCheckedMove = winningCondition - 1;
                var handleFoundLocker = new object();
                // should be search for direction of vector
                var searchFront = checkPlayerofMove(
                    searchRangeLimitation,
                    getNextFrontMoveRowIndex,
                    getNextFrontMoveColumnIndex,
                    () => cancellationTokenSource1.Cancel(),
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            ++satisfiedCheckedMove;
                            if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove)
                            {
                                cancellationTokenSource1.Cancel();
                                cancellationTokenSource2.Cancel();
                            }
                        }
                    },
                    () => cancellationTokenSource1.Cancel(),
                    cancellationTokenSource1.Token);
                var searchBehind = checkPlayerofMove(
                    searchRangeLimitation,
                    getNextBehindMoveRowIndex,
                    getNextBehindMoveColumnIndex,
                    () => cancellationTokenSource2.Cancel(),
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            ++satisfiedCheckedMove;
                            if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove)
                            {
                                cancellationTokenSource1.Cancel();
                                cancellationTokenSource2.Cancel();
                            }
                        }
                    },
                    () => cancellationTokenSource2.Cancel(),
                    cancellationTokenSource2.Token);
                await Task.WhenAll(searchFront, searchBehind);
                if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            };

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var numberOfSatisfiedLineSegment = 0;

            var handleFinishedCheckTaskLocker = new object();
            var handleFinishedCheckTask = (Task<bool> checkTask) =>
            {
                lock (handleFinishedCheckTaskLocker)
                {
                    if (checkTask.Result)
                    {
                        numberOfSatisfiedLineSegment++;
                    }
                    if (numberOfSatisfiedLineSegment >= numberOfNeededSatisfiedLineSegment)
                    {
                        cancellationTokenSource.Cancel();
                    }
                    return Task.FromResult(checkTask.Result);
                }
            };

            var checkVerticalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkHorizontalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkLeftDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkRightDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);

            Task.WaitAll(checkVerticalLine, checkHorizontalLine, checkLeftDiagonal, checkRightDiagonal);

            if (numberOfSatisfiedLineSegment >= 1)
            {
                result.Add(new WinningMoveType());
            }

            return result;
        }

        protected class WinningMoveType : IWinningMoveType { }
    }

    public class DefensiveMoveTypeCategorizer : TacticalMoveTypeCategorizer
    {
        public override IEnumerable<IMoveType> Categorize(IMove move, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            var result = new List<IMoveType>(base.Categorize(move, moveRetriever, playingBoard));

            var isThereAnyOtherMoveFromOpponentInsideTheSquareWithLengthAsTheWinningCondition = (IMove move) =>
            {
                var winningCondition = playingBoard.WinningCondition;
                var playedMoves = moveRetriever;

                foreach (var playedMove in playedMoves)
                {
                    if (!move.Equals(playedMove)
                    && !move.Player.Equals(playedMove.Player)
                    && Math.Abs(playedMove.Row - move.Row) < winningCondition
                    && Math.Abs(playedMove.Column - move.Column) < winningCondition)
                    {
                        return true;
                    }
                }
                return false;
            };

            if (isThereAnyOtherMoveFromOpponentInsideTheSquareWithLengthAsTheWinningCondition(move))
            {
                result.Add(new DefensiveMoveType());
            }

            return result;
        }

        protected class DefensiveMoveType : IDefensiveMoveType { }
    }

    public class BlockForkMoveTypeCategorizer : DefensiveMoveTypeCategorizer
    {
        public override IEnumerable<IMoveType> Categorize(IMove move, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            if (!(playingBoard.WinningCondition - 2 > 0))
            {
                throw new ArgumentException();
            }

            var result = new List<IMoveType>(base.Categorize(move, moveRetriever, playingBoard));

            var winningCondition = playingBoard.WinningCondition;
            var numberOfRows = playingBoard.NumberOfRows;
            var numberOfColumns = playingBoard.NumberOfColumns;
            var numberOfNeededSatisifedLineSegment = 2;

            var check = async (
                Func<int, int> getNextFrontMoveRowIndex,
                Func<int, int> getNextFrontMoveColumnIndex,
                Func<int, int> getNextBehindMoveRowIndex,
                Func<int, int> getNextBehindMoveColumnIndex,
                CancellationToken cancellationToken
                ) =>
            {
                var satisfiedCheckedMove = 0;
                var cancellationTokenSource1 = new CancellationTokenSource();
                var cancellationTokenSource2 = new CancellationTokenSource();

                var checkPlayerofMove = (
                    int limitation,
                    Func<int, int> getNextCheckedMoveRowIndex,
                    Func<int, int> getNextCheckedMoveColumnIndex,
                    Action whenFoundUnplayedMove,
                    Action whenFoundPlayerMove,
                    Action whenFoundOpponentMove,
                    CancellationToken cancellationToken) =>
                {
                    var checkedTimes = 0;
                    var currentCheckedMoveRowIndex = move.Row;
                    var currentChecedkMoveColumnIndex = move.Column;
                    IMove? currentCheckedMove = null;

                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        checkedTimes++;

                        if (checkedTimes >= limitation)
                        {
                            break;
                        }

                        currentCheckedMoveRowIndex = getNextCheckedMoveRowIndex(currentCheckedMoveRowIndex);
                        currentChecedkMoveColumnIndex = getNextCheckedMoveColumnIndex(currentChecedkMoveColumnIndex);
                        currentCheckedMove = moveRetriever.Retrieve(currentCheckedMoveRowIndex, currentChecedkMoveColumnIndex);

                        if (currentCheckedMove is null)
                        {
                            whenFoundUnplayedMove();
                        }
                        else

                        {
                            if (move.Player.Equals(currentCheckedMove.Player))
                            {
                                whenFoundPlayerMove();
                            }
                            else
                            {
                                whenFoundOpponentMove();
                            }
                        }
                    }

                    return Task.CompletedTask;
                };
                //|O||-||O|
                //|X||X||-|
                //|O||-||X|

                var isUnplayedMoveIgnored = false;
                var handleFoundLocker = new object();
                var numberOfNeededSatisfiedCheckedMove = winningCondition - 2;
                var searchRangeLimitation = winningCondition - 1;

                var searchFront = checkPlayerofMove(
                    searchRangeLimitation,
                    getNextFrontMoveRowIndex,
                    getNextFrontMoveColumnIndex,
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            if (isUnplayedMoveIgnored)
                            {
                                cancellationTokenSource1.Cancel();
                            }
                            isUnplayedMoveIgnored = true;
                        }
                    },
                    () => cancellationTokenSource1.Cancel(),
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            ++satisfiedCheckedMove;
                            if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove)
                            {
                                cancellationTokenSource1.Cancel();
                                cancellationTokenSource2.Cancel();
                            }
                        }
                    },
                    cancellationTokenSource1.Token);
                var searchBehind = checkPlayerofMove(
                    searchRangeLimitation,
                    getNextBehindMoveRowIndex,
                    getNextBehindMoveColumnIndex,
                    () =>
                    {
                        // that unplayed move is the next winning move.
                        lock (handleFoundLocker)
                        {
                            if (isUnplayedMoveIgnored)
                            {
                                cancellationTokenSource2.Cancel();
                            }
                            isUnplayedMoveIgnored = true;
                        }
                    },
                    () => cancellationTokenSource2.Cancel(),
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            ++satisfiedCheckedMove;
                            if (satisfiedCheckedMove >= winningCondition - 2)
                            {
                                cancellationTokenSource1.Cancel();
                                cancellationTokenSource2.Cancel();
                            }
                        }
                    },
                    cancellationTokenSource2.Token);
                await Task.WhenAll(searchFront, searchBehind);
                if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            };

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var numberOfSatisfiedLineSegment = 0;

            var handleFinishedCheckTaskLocker = new object();
            var handleFinishedCheckTask = (Task<bool> checkTask) =>
            {
                lock (handleFinishedCheckTaskLocker)
                {
                    if (checkTask.Result)
                    {
                        numberOfSatisfiedLineSegment++;
                    }
                    if (numberOfSatisfiedLineSegment >= numberOfNeededSatisifedLineSegment)
                    {
                        cancellationTokenSource.Cancel();
                    }
                    return Task.FromResult(checkTask.Result);
                }
            };

            var checkVerticalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkHorizontalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkLeftDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkRightDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);

            Task.WaitAll(checkVerticalLine, checkHorizontalLine, checkLeftDiagonal, checkRightDiagonal);

            if (numberOfSatisfiedLineSegment == 2)
            {
                result.Add(new BlockForkMoveType());
            }

            return result;
        }

        protected class BlockForkMoveType : IBlockForkMoveType { }
    }

    public class BlockWinningMoveTypeCategorizer : DefensiveMoveTypeCategorizer
    {
        public override IEnumerable<IMoveType> Categorize(IMove move, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            if (!(playingBoard.WinningCondition - 2 > 0))
            {
                throw new ArgumentException();
            }

            var result = new List<IMoveType>(base.Categorize(move, moveRetriever, playingBoard));

            var winningCondition = playingBoard.WinningCondition;
            var numberOfRows = playingBoard.NumberOfRows;
            var numberOfColumns = playingBoard.NumberOfColumns;
            var numberOfNeededSatisfiedLineSegment = 1;

            // need a better name
            var check = async (
                Func<int, int> getNextFrontMoveRowIndex,
                Func<int, int> getNextFrontMoveColumnIndex,
                Func<int, int> getNextBehindMoveRowIndex,
                Func<int, int> getNextBehindMoveColumnIndex,
                CancellationToken cancellationToken
                ) =>
            {
                var cancellationTokenSource1 = new CancellationTokenSource();
                var cancellationTokenSource2 = new CancellationTokenSource();

                var checkPlayerofMove = (
                    int limitation,
                    Func<int, int> getNextCheckedMoveRowIndex,
                    Func<int, int> getNextCheckedMoveColumnIndex,
                    Action whenFoundUnplayedMove,
                    Action whenFoundPlayerMove,
                    Action whenFoundOpponentMove,
                    CancellationToken cancellationToken) =>
                {
                    var checkedTimes = 0;
                    var currentCheckedMoveRowIndex = move.Row;
                    var currentChecedkMoveColumnIndex = move.Column;
                    IMove? currentCheckedMove = null;

                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        checkedTimes++;

                        if (checkedTimes >= limitation)
                        {
                            break;
                        }

                        currentCheckedMoveRowIndex = getNextCheckedMoveRowIndex(currentCheckedMoveRowIndex);
                        currentChecedkMoveColumnIndex = getNextCheckedMoveColumnIndex(currentChecedkMoveColumnIndex);
                        currentCheckedMove = moveRetriever.Retrieve(currentCheckedMoveRowIndex, currentChecedkMoveColumnIndex);

                        if (currentCheckedMove is null)
                        {
                            whenFoundUnplayedMove();
                        }
                        else

                        {
                            if (move.Player.Equals(currentCheckedMove.Player))
                            {
                                whenFoundPlayerMove();
                            }
                            else
                            {
                                whenFoundOpponentMove();
                            }
                        }
                    }

                    return Task.CompletedTask;
                };

                var searchRangeLimitation = winningCondition - 1;
                var numberOfNeededSatisfiedCheckedMove = winningCondition - 1;
                var handleFoundLocker = new object();
                // should be search for direction of vector

                var foundUnplayedMoveInFront = false;
                var foundPlayerMoveInFront = false;
                var numberOfFoundInFrontOpponentMove = 0;

                var foundUnplayedMoveInBehind = false;
                var foundPlayerMoveInBehind = false;
                var numberOfFoundInBehindOpponentMove = 0;

                var searchFront = checkPlayerofMove(
                    searchRangeLimitation,
                    getNextFrontMoveRowIndex,
                    getNextFrontMoveColumnIndex,
                    () =>
                    {
                        if (foundUnplayedMoveInFront)
                        {
                            cancellationTokenSource1.Cancel();
                        }
                        foundUnplayedMoveInFront = true;
                    },
                    () =>
                    {
                        foundPlayerMoveInFront = true;
                        cancellationTokenSource1.Cancel();
                    },
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            ++numberOfFoundInFrontOpponentMove;
                            if (numberOfFoundInFrontOpponentMove + numberOfFoundInBehindOpponentMove >= numberOfNeededSatisfiedCheckedMove)
                            {
                                cancellationTokenSource1.Cancel();
                                cancellationTokenSource2.Cancel();
                            }
                        }
                    },
                    cancellationTokenSource1.Token);
                var searchBehind = checkPlayerofMove(
                    searchRangeLimitation,
                    getNextBehindMoveRowIndex,
                    getNextBehindMoveColumnIndex,
                    () =>
                    {
                        if (foundUnplayedMoveInBehind)
                        {
                            cancellationTokenSource2.Cancel();
                        }
                        foundUnplayedMoveInBehind = true;
                    },
                    () =>
                    {
                        foundPlayerMoveInBehind = true;
                        cancellationTokenSource2.Cancel();
                    },
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            ++numberOfFoundInBehindOpponentMove;
                            if (numberOfFoundInFrontOpponentMove + numberOfFoundInBehindOpponentMove >= numberOfNeededSatisfiedCheckedMove)
                            {
                                cancellationTokenSource1.Cancel();
                                cancellationTokenSource2.Cancel();
                            }
                        }
                    },
                    cancellationTokenSource2.Token);
                await Task.WhenAll(searchFront, searchBehind);
                if ((numberOfFoundInFrontOpponentMove + numberOfFoundInBehindOpponentMove == numberOfNeededSatisfiedCheckedMove)
                || (numberOfFoundInFrontOpponentMove == numberOfNeededSatisfiedCheckedMove - 1 && foundPlayerMoveInFront)
                || (numberOfFoundInBehindOpponentMove == numberOfNeededSatisfiedCheckedMove - 1 && foundUnplayedMoveInBehind)
                || (numberOfFoundInFrontOpponentMove == numberOfNeededSatisfiedCheckedMove && foundPlayerMoveInFront)
                || (numberOfFoundInBehindOpponentMove == numberOfNeededSatisfiedCheckedMove && foundPlayerMoveInBehind))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            };

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var numberOfSatisfiedLineSegment = 0;

            var handleFinishedCheckTaskLocker = new object();
            var handleFinishedCheckTask = (Task<bool> checkTask) =>
            {
                lock (handleFinishedCheckTaskLocker)
                {
                    if (checkTask.Result)
                    {
                        numberOfSatisfiedLineSegment++;
                    }
                    if (numberOfSatisfiedLineSegment >= numberOfNeededSatisfiedLineSegment)
                    {
                        cancellationTokenSource.Cancel();
                    }
                    return Task.FromResult(checkTask.Result);
                }
            };

            var checkVerticalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkHorizontalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkLeftDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkRightDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);

            Task.WaitAll(checkVerticalLine, checkHorizontalLine, checkLeftDiagonal, checkRightDiagonal);

            if (numberOfSatisfiedLineSegment == 1)
            {
                result.Add(new BlockWinningMove());
            }

            return result;
        }

        protected class BlockWinningMove : IBlockWinningMoveType { }
    }
}
