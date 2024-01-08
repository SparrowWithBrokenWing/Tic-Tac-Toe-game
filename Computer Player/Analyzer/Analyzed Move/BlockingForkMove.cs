namespace ComputerPlayer
{
    public class BlockingForkMove : DefensiveMove
    {
        public BlockingForkMove(IMove move) : base(move)
        {
        }

        protected override bool Validate(IMove move)
        {
            var areThereAnyAtLeast2OpponentsLineSegmentsWithLengthAsTheWinningConditionMinusTwo = (IMove move) =>
            {
                if (!(this.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition - 2 > 0))
                {
                    throw new ArgumentException();
                }

                var numberOfSatisfiedLineSegments = 0;

                var checkedMove = 0;
                var satisfiedCheckedMove = 0;

                var latestCheckedMoveRow = (uint)Math.Abs(move.Row - this.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition + 1);
                var latestCheckedMoveColumn = (uint)Math.Abs(move.Column - this.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition + 1);
                while (checkedMove < 8)
                {
                    var latestCheckedMove = this.BoardStateBeforeBeingPlayed[latestCheckedMoveRow, latestCheckedMoveColumn];
                    if (latestCheckedMove is not null)
                    {
                        if (!latestCheckedMove.Player.Equals(move.Player))
                        {
                            satisfiedCheckedMove++;
                        }
                        else
                        {
                            satisfiedCheckedMove = 0;
                        }
                    }

                    if (satisfiedCheckedMove == move.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition - 2)
                    {
                        numberOfSatisfiedLineSegments++;
                    }

                    checkedMove++;
                    latestCheckedMoveRow++;
                    latestCheckedMoveColumn++;
                }

                checkedMove = 0;
                satisfiedCheckedMove = 0;
                latestCheckedMoveRow = (uint)Math.Abs(move.Row - this.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition + 1);
                latestCheckedMoveColumn = (uint)Math.Abs(move.Column);
                while (checkedMove < 8)
                {
                    var latestCheckedMove = this.BoardStateBeforeBeingPlayed[latestCheckedMoveRow, latestCheckedMoveColumn];
                    if (latestCheckedMove is not null)
                    {
                        if (!latestCheckedMove.Player.Equals(move.Player))
                        {
                            satisfiedCheckedMove++;
                        }
                        else
                        {
                            satisfiedCheckedMove = 0;
                        }
                    }

                    if (satisfiedCheckedMove == move.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition - 2)
                    {
                        numberOfSatisfiedLineSegments++;
                    }

                    checkedMove++;
                    latestCheckedMoveRow++;
                }
                if (numberOfSatisfiedLineSegments == 2)
                {
                    return true;
                }

                checkedMove = 0;
                satisfiedCheckedMove = 0;
                latestCheckedMoveRow = (uint)Math.Abs(move.Row - this.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition + 1);
                latestCheckedMoveColumn = (uint)Math.Abs(move.Column + this.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition - 1);
                while (checkedMove < 8)
                {
                    var latestCheckedMove = this.BoardStateBeforeBeingPlayed[latestCheckedMoveRow, latestCheckedMoveColumn];
                    if (latestCheckedMove is not null)
                    {
                        if (!latestCheckedMove.Player.Equals(move.Player))
                        {
                            satisfiedCheckedMove++;
                        }
                        else
                        {
                            satisfiedCheckedMove = 0;
                        }
                    }

                    if (satisfiedCheckedMove == move.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition - 2)
                    {
                        numberOfSatisfiedLineSegments++;
                    }

                    checkedMove++;
                    latestCheckedMoveRow++;
                    latestCheckedMoveColumn++;
                }
                if (numberOfSatisfiedLineSegments == 2)
                {
                    return true;
                }

                checkedMove = 0;
                satisfiedCheckedMove = 0;
                latestCheckedMoveRow = (uint)Math.Abs(move.Row);
                latestCheckedMoveColumn = (uint)Math.Abs(move.Column - this.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition + 1);
                while (checkedMove < 8)
                {
                    var latestCheckedMove = this.BoardStateBeforeBeingPlayed[latestCheckedMoveRow, latestCheckedMoveColumn];
                    if (latestCheckedMove is not null)
                    {
                        if (!latestCheckedMove.Player.Equals(move.Player))
                        {
                            satisfiedCheckedMove++;
                        }
                        else
                        {
                            satisfiedCheckedMove = 0;
                        }
                    }

                    if (satisfiedCheckedMove == move.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition - 2)
                    {
                        numberOfSatisfiedLineSegments++;
                    }

                    checkedMove++;
                    latestCheckedMoveColumn++;
                }
                if (numberOfSatisfiedLineSegments == 2)
                {
                    return true;
                }

                return false;
            };

            return base.Validate(move)
                && areThereAnyAtLeast2OpponentsLineSegmentsWithLengthAsTheWinningConditionMinusTwo(move);
        }
    }
    }
