namespace ComputerPlayer.Analyzer
{
    // move validator will be used as dependencies of move factory

    public interface IMoveTypeValidator
    {
        public bool Validate(IMove validatedMove, IMoveRetriever moveRetriever, IBoard playingBoard);
    }

    public sealed class CompositeMoveTypeValidator : IMoveTypeValidator
    {
        public CompositeMoveTypeValidator(IEnumerable<IMoveTypeValidator> checkers)
        {
            _Checkers = checkers;
        }

        private IEnumerable<IMoveTypeValidator> _Checkers { get; set; }

        public bool Validate(IMove validatedMove, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            foreach (var checker in _Checkers)
            {
                if (!checker.Validate(validatedMove, moveRetriever, playingBoard))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class PossibleMoveTypeValidator : IMoveTypeValidator
    {
        public virtual bool Validate(IMove validatedMove, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            var numberOfRows = playingBoard.NumberOfRows;
            var numberOfColumns = playingBoard.NumberOfColumns;

            var isItAnUnplayedMove = (IMove validatedMove) =>
            {
                foreach (var playedMove in moveRetriever)
                {
                    if (validatedMove.Row == playedMove.Row
                    && validatedMove.Column == playedMove.Column)
                    {
                        return false;
                    }
                }
                return true;
            };

            var isItInAnAllowedRangeMove = (IMove validatedMove) =>
            {
                return validatedMove.Row > 0 && validatedMove.Row <= numberOfRows
                && validatedMove.Column > 0 && validatedMove.Column <= numberOfColumns;
            };

            return isItAnUnplayedMove(validatedMove)
                && isItInAnAllowedRangeMove(validatedMove);
        }
    }

    public class TacticalMoveTypeValidator : PossibleMoveTypeValidator
    {
        public override bool Validate(IMove validatedMove, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            var winningCondition = playingBoard.WinningCondition;

            var isThereAnyOtherMoveInsideTheSquareWithLengthAsTheWinningCondition = (IMove validatedMove) =>
            {
                foreach (var playedMove in moveRetriever)
                {
                    if ((Math.Abs(playedMove.Row - validatedMove.Row) < (winningCondition - 1))
                    && (Math.Abs(playedMove.Column - validatedMove.Column) < (winningCondition - 1)))
                    {
                        return true;
                    }
                }
                return false;
            };

            return base.Validate(validatedMove, moveRetriever, playingBoard)
                && isThereAnyOtherMoveInsideTheSquareWithLengthAsTheWinningCondition(validatedMove);
        }
    }

    public class OffensiveMoveTypeValidator : TacticalMoveTypeValidator
    {
        public override bool Validate(IMove validatedMove, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            var winningCondition = playingBoard.WinningCondition;

            var isThereAnyOtherMoveFromSamePlayerInsideTheSquareWithLengthAsTheWinningCondition = (IMove validatedMove) =>
            {
                foreach (var playedMove in moveRetriever)
                {
                    if (validatedMove.Player.Equals(playedMove.Player)
                    && Math.Abs(playedMove.Row - validatedMove.Row) < winningCondition
                    && Math.Abs(playedMove.Column - validatedMove.Column) < winningCondition)
                    {
                        return true;
                    }
                }
                return false;
            };

            return base.Validate(validatedMove, moveRetriever, playingBoard)
                && isThereAnyOtherMoveFromSamePlayerInsideTheSquareWithLengthAsTheWinningCondition(validatedMove);
        }
    }

    public class ForkMoveTypeValidator : OffensiveMoveTypeValidator
    {
        // need to change how to check. The check will come from back and front of checked validatedMove (2 tasks). Those tasks will change the value of satisfied validatedMove variable. If satisified validatedMove variable reach a value, those task will be cancel or will return.
        public override bool Validate(IMove validatedMove, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            var winningCondition = playingBoard.WinningCondition;
            var numberOfRows = playingBoard.NumberOfRows;
            var numberOfColumns = playingBoard.NumberOfColumns;

            if (!(winningCondition - 2 > 0))
            {
                throw new ArgumentException();
            }

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
                    var currentCheckedMoveRowIndex = validatedMove.Row;
                    var currentChecedkMoveColumnIndex = validatedMove.Column;
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
                            if (validatedMove.Player.Equals(currentCheckedMove.Player))
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
                        // that unplayed validatedMove is the next winning validatedMove.
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
                        // that unplayed validatedMove is the next winning validatedMove.
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

            return base.Validate(validatedMove, moveRetriever, playingBoard)
                && numberOfSatisfiedLineSegment >= 2;
        }
    }

    public class WinningMoveTypeValidator : OffensiveMoveTypeValidator
    {
        public override bool Validate(IMove validatedMove, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            if (!(playingBoard.WinningCondition - 2 > 0))
            {
                throw new ArgumentException();
            }

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
                    var currentCheckedMoveRowIndex = validatedMove.Row;
                    var currentChecedkMoveColumnIndex = validatedMove.Column;
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
                            if (validatedMove.Player.Equals(currentCheckedMove.Player))
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

            return base.Validate(validatedMove, moveRetriever, playingBoard)
                && numberOfSatisfiedLineSegment >= 1;
        }
    }

    public class DefensiveMoveTypeValidator : TacticalMoveTypeValidator
    {
        public override bool Validate(IMove validatedMove, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            var isThereAnyOtherMoveFromOpponentInsideTheSquareWithLengthAsTheWinningCondition = (IMove validatedMove) =>
            {
                var winningCondition = playingBoard.WinningCondition;
                var playedMoves = moveRetriever;

                foreach (var playedMove in playedMoves)
                {
                    if (!validatedMove.Equals(playedMove)
                    && !validatedMove.Player.Equals(playedMove.Player)
                    && Math.Abs(playedMove.Row - validatedMove.Row) < winningCondition
                    && Math.Abs(playedMove.Column - validatedMove.Column) < winningCondition)
                    {
                        return true;
                    }
                }
                return false;
            };

            return base.Validate(validatedMove, moveRetriever, playingBoard)
                && isThereAnyOtherMoveFromOpponentInsideTheSquareWithLengthAsTheWinningCondition(validatedMove);
        }
    }

    public class BlockForkMoveTypeValidator : DefensiveMoveTypeValidator
    {
        public override bool Validate(IMove validatedMove, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            if (!(playingBoard.WinningCondition - 2 > 0))
            {
                throw new ArgumentException();
            }

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
                    var currentCheckedMoveRowIndex = validatedMove.Row;
                    var currentChecedkMoveColumnIndex = validatedMove.Column;
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
                            if (validatedMove.Player.Equals(currentCheckedMove.Player))
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
                        // that unplayed validatedMove is the next winning validatedMove.
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

            return base.Validate(validatedMove, moveRetriever, playingBoard)
                && numberOfSatisfiedLineSegment == 2;
        }
    }

    public class BlockWinningMoveTypeValidator : DefensiveMoveTypeValidator
    {
        public override bool Validate(IMove validatedMove, IMoveRetriever moveRetriever, IBoard playingBoard)
        {
            if (!(playingBoard.WinningCondition - 2 > 0))
            {
                throw new ArgumentException();
            }

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
                    var currentCheckedMoveRowIndex = validatedMove.Row;
                    var currentChecedkMoveColumnIndex = validatedMove.Column;
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
                            if (validatedMove.Player.Equals(currentCheckedMove.Player))
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

            return base.Validate(validatedMove, moveRetriever, playingBoard)
                && numberOfSatisfiedLineSegment == 1;
        }
    }
}