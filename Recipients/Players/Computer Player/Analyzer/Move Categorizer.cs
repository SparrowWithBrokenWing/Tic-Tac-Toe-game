using Moq;
using System.Reflection;

namespace ComputerPlayer.Analyzer
{
    public interface IMoveType { }

    public interface IPossibleMoveType : IMoveType { }
    public interface ITacticalMoveType : IPossibleMoveType { }

    public interface IOffensiveMoveType : ITacticalMoveType { }
    public interface IForkMoveType : IOffensiveMoveType { }
    public interface IWinningMoveType : IOffensiveMoveType { }

    public interface IDefensiveMoveType : ITacticalMoveType { }
    public interface IBlockForkMoveType : IDefensiveMoveType { }
    public interface IBlockWinningMoveType : IDefensiveMoveType { }

    public interface ICategorizableMove : IMove
    {
        public ICollection<IMoveType> Categories { get; }
    }

    public interface ICategorizedMove : IMove
    {
        public IEnumerable<IMoveType> Categories { get; }
    }

    public interface IMoveCategorizer
    {
        public IEnumerable<IMoveType> Categorize(IMove move, IMoveRetriever moveRetriever, IBoard playingBoard);
    }

    public class CompositeCategorizer : IMoveCategorizer
    {
        public CompositeCategorizer(IEnumerable<IMoveCategorizer> categorizers)
        {
            _Categorizers = categorizers;
        }

        protected IEnumerable<IMoveCategorizer> _Categorizers { get; private set; }

        public IEnumerable<IMoveType> Categorize(IMove move, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            var mockOfComparer = new Mock<IEqualityComparer<IMoveType>>();
            mockOfComparer
                .Setup((comparer) => comparer.Equals(It.IsAny<IMoveType>(), It.IsAny<IMoveType>()))
                .Returns((IMoveType? first, IMoveType? second) =>
                {
                    const bool EQUAL = true;
                    const bool DIFFERENT = false;
                    if ((first is null || second is null)
                        && (first is not null || second is not null))
                    {
                        return DIFFERENT;
                    }
                    else
                    {
                        if (first is not null && second is not null)
                        {
                            var interfacesOfFirst = first.GetType().GetInterfaces();
                            var interfacesOfSecond = second.GetType().GetInterfaces();
                            if (!interfacesOfFirst.Except(interfacesOfSecond).Any()
                            && !interfacesOfSecond.Except(interfacesOfFirst).Any())
                            {
                                return EQUAL;
                            }
                            else
                            {
                                return DIFFERENT;
                            }
                        }
                        else
                        {
                            return DIFFERENT;
                        }
                    }
                });
            var result = new HashSet<IMoveType>(mockOfComparer.Object);
            //var result = new List<IMoveType>();
            foreach (var categorizer in _Categorizers)
            {
                var moveTypes = categorizer.Categorize(move, moveRetriever, playingBoard);
                foreach (var moveType in moveTypes)
                {
                    //if (moveTypes.OfType)
                    result.Add(moveType);
                }
            }
            return result;
        }
    }

    public class PossibleMoveTypeCategorizer : IMoveCategorizer
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

            var check = (
                Func<int, int> getNextFrontMoveRowIndex,
                Func<int, int> getNextFrontMoveColumnIndex,
                Func<int, int> getNextBehindMoveRowIndex,
                Func<int, int> getNextBehindMoveColumnIndex,
                CancellationToken cancellationToken
                ) =>
            {
                var checkPlayerOfMove = (
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
                    var currentCheckedMoveColumnIndex = move.Column;
                    IMove? currentCheckedMove = null;

                    while (true)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return Task.FromCanceled(cancellationToken);
                        }

                        checkedTimes++;

                        if (checkedTimes > limitation)
                        {
                            break;
                        }

                        currentCheckedMoveRowIndex = getNextCheckedMoveRowIndex(currentCheckedMoveRowIndex);
                        currentCheckedMoveColumnIndex = getNextCheckedMoveColumnIndex(currentCheckedMoveColumnIndex);
                        currentCheckedMove = moveRetriever.Retrieve(currentCheckedMoveRowIndex, currentCheckedMoveColumnIndex);

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
                var satisfiedCheckedMove = 0;
                var isUnplayedMoveIgnored = false;
                var frontSearchCancellationTokenSource = new CancellationTokenSource();
                var behindSearchCancellationTokenSource = new CancellationTokenSource();

                // should be search for direction of vector
                var searchFront = checkPlayerOfMove(
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
                                frontSearchCancellationTokenSource.Cancel();
                            }
                            isUnplayedMoveIgnored = true;
                        }
                    },
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            satisfiedCheckedMove = satisfiedCheckedMove + 1;
                            if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove)
                            {
                                frontSearchCancellationTokenSource.Cancel();
                                behindSearchCancellationTokenSource.Cancel();
                            }
                        }
                    },
                    () => frontSearchCancellationTokenSource.Cancel(),
                    frontSearchCancellationTokenSource.Token);
                var searchBehind = checkPlayerOfMove(
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
                                behindSearchCancellationTokenSource.Cancel();
                            }
                            isUnplayedMoveIgnored = true;
                        }
                    },
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            satisfiedCheckedMove = satisfiedCheckedMove + 1;
                            if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove)
                            {
                                frontSearchCancellationTokenSource.Cancel();
                                behindSearchCancellationTokenSource.Cancel();
                            }
                        }
                    },
                    () => behindSearchCancellationTokenSource.Cancel(),
                    behindSearchCancellationTokenSource.Token);
                if (cancellationToken.IsCancellationRequested)
                {
                    frontSearchCancellationTokenSource.Cancel();
                    behindSearchCancellationTokenSource.Cancel();
                }
                try
                {
                    Task.WaitAll(searchFront, searchBehind);
                }
                catch
                {

                }
                if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove
                /*&& isUnplayedMoveIgnored*/)
                {
                    return Task.FromResult(true);
                }
                else
                {
                    return Task.FromResult(false);
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
                    if (checkTask.IsCanceled)
                    {
                        return Task.CompletedTask;
                    }
                    if (checkTask.Result)
                    {
                        numberOfSatisfiedLineSegment = numberOfSatisfiedLineSegment + 1;
                    }
                    if (numberOfSatisfiedLineSegment >= numberOfNeededSatisifedLineSegment)
                    {
                        cancellationTokenSource.Cancel();
                    }
                    return Task.CompletedTask;
                }
            };

            var checkVerticalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkHorizontalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkLeftDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkRightDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);

            try
            {
                Task.WaitAll(checkVerticalLine, checkHorizontalLine, checkLeftDiagonal, checkRightDiagonal);
            }
            catch (AggregateException aggregateException)
            {
                if (!(aggregateException.InnerException is TaskCanceledException))
                {
                    throw;
                }
            }

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
            var winningCondition = playingBoard.WinningCondition;
            var numberOfRows = playingBoard.NumberOfRows;
            var numberOfColumns = playingBoard.NumberOfColumns;

            if (!(winningCondition - 2 > 0))
            {
                throw new ArgumentException();
            }

            var result = new List<IMoveType>(base.Categorize(move, moveRetriever, playingBoard));

            var numberOfNeededSatisifedLineSegment = 2;

            var check = (
                Func<int, int> getNextFrontMoveRowIndex,
                Func<int, int> getNextFrontMoveColumnIndex,
                Func<int, int> getNextBehindMoveRowIndex,
                Func<int, int> getNextBehindMoveColumnIndex,
                CancellationToken cancellationToken
                ) =>
            {
                var checkPlayerOfMove = (
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
                    var currentCheckedMoveColumnIndex = move.Column;
                    IMove? currentCheckedMove = null;

                    while (true)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return Task.FromCanceled(cancellationToken);
                        }

                        checkedTimes++;

                        if (checkedTimes > limitation)
                        {
                            break;
                        }

                        currentCheckedMoveRowIndex = getNextCheckedMoveRowIndex(currentCheckedMoveRowIndex);
                        currentCheckedMoveColumnIndex = getNextCheckedMoveColumnIndex(currentCheckedMoveColumnIndex);
                        currentCheckedMove = moveRetriever.Retrieve(currentCheckedMoveRowIndex, currentCheckedMoveColumnIndex);

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
                var numberOfNeededSatisfiedCheckedMove = winningCondition - 1;
                var searchRangeLimitation = winningCondition - 1;
                var satisfiedCheckedMove = 0;
                //var isUnplayedMoveIgnored = false;
                var frontSearchCancellationTokenSource = new CancellationTokenSource();
                var behindSearchCancellationTokenSource = new CancellationTokenSource();

                // should be search for direction of vector
                var searchFront = checkPlayerOfMove(
                    searchRangeLimitation,
                    getNextFrontMoveRowIndex,
                    getNextFrontMoveColumnIndex,
                    () => frontSearchCancellationTokenSource.Cancel(),
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            satisfiedCheckedMove = satisfiedCheckedMove + 1;
                            if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove)
                            {
                                frontSearchCancellationTokenSource.Cancel();
                                behindSearchCancellationTokenSource.Cancel();
                            }
                        }
                    },
                    () => frontSearchCancellationTokenSource.Cancel(),
                    frontSearchCancellationTokenSource.Token);
                var searchBehind = checkPlayerOfMove(
                    searchRangeLimitation,
                    getNextBehindMoveRowIndex,
                    getNextBehindMoveColumnIndex,
                    () => behindSearchCancellationTokenSource.Cancel(),
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            satisfiedCheckedMove = satisfiedCheckedMove + 1;
                            if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove)
                            {
                                frontSearchCancellationTokenSource.Cancel();
                                behindSearchCancellationTokenSource.Cancel();
                            }
                        }
                    },
                    () => behindSearchCancellationTokenSource.Cancel(),
                    behindSearchCancellationTokenSource.Token);
                if (cancellationToken.IsCancellationRequested)
                {
                    frontSearchCancellationTokenSource.Cancel();
                    behindSearchCancellationTokenSource.Cancel();
                }
                try
                {
                    Task.WaitAll(searchFront, searchBehind);
                }
                catch
                {

                }
                if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove
                /*&& isUnplayedMoveIgnored*/)
                {
                    return Task.FromResult(true);
                }
                else
                {
                    return Task.FromResult(false);
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
                    if (checkTask.IsCanceled)
                    {
                        return Task.CompletedTask;
                    }
                    if (checkTask.Result)
                    {
                        numberOfSatisfiedLineSegment = numberOfSatisfiedLineSegment + 1;
                    }
                    if (numberOfSatisfiedLineSegment >= numberOfNeededSatisifedLineSegment)
                    {
                        cancellationTokenSource.Cancel();
                    }
                    return Task.CompletedTask;
                }
            };

            var checkVerticalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkHorizontalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkLeftDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkRightDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);

            try
            {
                Task.WaitAll(checkVerticalLine, checkHorizontalLine, checkLeftDiagonal, checkRightDiagonal);
            }
            catch (AggregateException aggregateException)
            {
                if (!(aggregateException.InnerException is TaskCanceledException))
                {
                    throw;
                }
            }

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
            var winningCondition = playingBoard.WinningCondition;
            var numberOfRows = playingBoard.NumberOfRows;
            var numberOfColumns = playingBoard.NumberOfColumns;

            if (!(winningCondition - 2 > 0))
            {
                throw new ArgumentException();
            }

            var result = new List<IMoveType>(base.Categorize(move, moveRetriever, playingBoard));

            var numberOfNeededSatisifedLineSegment = 2;

            var check = (
                Func<int, int> getNextFrontMoveRowIndex,
                Func<int, int> getNextFrontMoveColumnIndex,
                Func<int, int> getNextBehindMoveRowIndex,
                Func<int, int> getNextBehindMoveColumnIndex,
                CancellationToken cancellationToken
                ) =>
            {
                var checkPlayerOfMove = (
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
                    var currentCheckedMoveColumnIndex = move.Column;
                    IMove? currentCheckedMove = null;

                    while (true)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return Task.FromCanceled(cancellationToken);
                        }

                        checkedTimes++;

                        if (checkedTimes > limitation)
                        {
                            break;
                        }

                        currentCheckedMoveRowIndex = getNextCheckedMoveRowIndex(currentCheckedMoveRowIndex);
                        currentCheckedMoveColumnIndex = getNextCheckedMoveColumnIndex(currentCheckedMoveColumnIndex);
                        currentCheckedMove = moveRetriever.Retrieve(currentCheckedMoveRowIndex, currentCheckedMoveColumnIndex);

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
                var satisfiedCheckedMove = 0;
                var isUnplayedMoveIgnored = false;
                var frontSearchCancellationTokenSource = new CancellationTokenSource();
                var behindSearchCancellationTokenSource = new CancellationTokenSource();

                // should be search for direction of vector
                var searchFront = checkPlayerOfMove(
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
                                frontSearchCancellationTokenSource.Cancel();
                            }
                            isUnplayedMoveIgnored = true;
                        }
                    },
                    () => frontSearchCancellationTokenSource.Cancel(),
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            satisfiedCheckedMove = satisfiedCheckedMove + 1;
                            if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove)
                            {
                                frontSearchCancellationTokenSource.Cancel();
                                behindSearchCancellationTokenSource.Cancel();
                            }
                        }
                    },
                    frontSearchCancellationTokenSource.Token);
                var searchBehind = checkPlayerOfMove(
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
                                behindSearchCancellationTokenSource.Cancel();
                            }
                            isUnplayedMoveIgnored = true;
                        }
                    },
                    () => behindSearchCancellationTokenSource.Cancel(),
                    () =>
                    {
                        lock (handleFoundLocker)
                        {
                            satisfiedCheckedMove = satisfiedCheckedMove + 1;
                            if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove)
                            {
                                frontSearchCancellationTokenSource.Cancel();
                                behindSearchCancellationTokenSource.Cancel();
                            }
                        }
                    },
                    behindSearchCancellationTokenSource.Token);
                if (cancellationToken.IsCancellationRequested)
                {
                    frontSearchCancellationTokenSource.Cancel();
                    behindSearchCancellationTokenSource.Cancel();
                }
                try
                {
                    Task.WaitAll(searchFront, searchBehind);
                }
                catch
                {

                }
                if (satisfiedCheckedMove >= numberOfNeededSatisfiedCheckedMove
                /*&& isUnplayedMoveIgnored*/)
                {
                    return Task.FromResult(true);
                }
                else
                {
                    return Task.FromResult(false);
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
                    if (checkTask.IsCanceled)
                    {
                        return Task.CompletedTask;
                    }
                    if (checkTask.Result)
                    {
                        numberOfSatisfiedLineSegment = numberOfSatisfiedLineSegment + 1;
                    }
                    if (numberOfSatisfiedLineSegment >= numberOfNeededSatisifedLineSegment)
                    {
                        cancellationTokenSource.Cancel();
                    }
                    return Task.CompletedTask;
                }
            };

            var checkVerticalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkHorizontalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkLeftDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkRightDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);

            try
            {
                Task.WaitAll(checkVerticalLine, checkHorizontalLine, checkLeftDiagonal, checkRightDiagonal);
            }
            catch (AggregateException aggregateException)
            {
                if (!(aggregateException.InnerException is TaskCanceledException))
                {
                    throw;
                }
            }

            if (numberOfSatisfiedLineSegment >= 2)
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
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return Task.FromCanceled(cancellationToken);
                        }

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
                            numberOfFoundInFrontOpponentMove = numberOfFoundInFrontOpponentMove + 1;
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
                            numberOfFoundInBehindOpponentMove = numberOfFoundInBehindOpponentMove + 1;
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
                    if (checkTask.IsCanceled)
                    {
                        return Task.CompletedTask;
                    }
                    if (checkTask.Result)
                    {
                        numberOfNeededSatisfiedLineSegment = numberOfSatisfiedLineSegment + 1;
                    }
                    if (numberOfSatisfiedLineSegment >= numberOfNeededSatisfiedLineSegment)
                    {
                        cancellationTokenSource.Cancel();
                    }
                    return Task.FromResult(checkTask.Result);
                }
            };

            var checkVerticalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkHorizontalLine = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkLeftDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    cancellationToken)
                .ContinueWith(handleFinishedCheckTask);
            var checkRightDiagonal = check(
                    (int checkingMoveRowIndex) => checkingMoveRowIndex - 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex + 1,
                    (int checkingMoveRowIndex) => checkingMoveRowIndex + 1,
                    (int checkingMoveColumnIndex) => checkingMoveColumnIndex - 1,
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