namespace ComputerPlayer
{
    public class OffensiveMove : TacticalMove
    {
        public OffensiveMove(IMove move) : base(move)
        {
        }

        protected override bool Validate(IMove move)
        {
            var isThereAnyOtherMoveFromSamePlayerInsideTheSquareWithLengthAsTheWinningCondition = (IMove move) =>
            {
                foreach (var playedMove in this.BoardStateBeforeBeingPlayed.PlayedMoves)
                {
                    if (move.Player.Equals(playedMove.Player)
                    && Math.Abs(playedMove.Row - move.Row) < this.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition
                    && Math.Abs(playedMove.Column - move.Column) < this.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition)
                    {
                        return true;
                    }
                }
                return false;
            };

            return base.Validate(move)
                && isThereAnyOtherMoveFromSamePlayerInsideTheSquareWithLengthAsTheWinningCondition(move);
        }
    }
    }
