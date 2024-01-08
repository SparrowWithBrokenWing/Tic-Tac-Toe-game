namespace ComputerPlayer
{
    public class FirstWinningBlockingMove : DefensiveMove
    {
        public FirstWinningBlockingMove(IMove move) : base(move)
        {
        }

        protected override bool Validate(IMove move)
        {
            var willThisMoveBlockingAOppnentLineSegmentWithLengthAsWinningConditionMinusOne = (IMove move) =>
            {
                if (!(this.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition - 1 > 0))
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
                if (numberOfSatisfiedLineSegments == 1)
                {
                    return true;
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
                if (numberOfSatisfiedLineSegments == 1)
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
                if (numberOfSatisfiedLineSegments == 1)
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
                if (numberOfSatisfiedLineSegments == 1)
                {
                    return true;
                }

                return false;
            };
            return base.Validate(move)
                && willThisMoveBlockingAOppnentLineSegmentWithLengthAsWinningConditionMinusOne(move);
        }
    }
    }
