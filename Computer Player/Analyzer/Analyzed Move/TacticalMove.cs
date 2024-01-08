namespace ComputerPlayer
{
    public class TacticalMove : PossbileMove
    {
        public TacticalMove(IMove move) : base(move)
        {
        }

        protected override bool Validate(IMove move)
        {
            var isThereAnyOtherMoveInsideTheSquareWithLengthAsTheWinningCondition = (IMove move) =>
            {
                foreach (var playedMove in this.BoardStateBeforeBeingPlayed.PlayedMoves)
                {
                    if ((Math.Abs(playedMove.Row - move.Row) < (this.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition - 1))
                    && (Math.Abs(playedMove.Column - move.Column) < (this.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition - 1)))
                    {
                        return true;
                    }
                }
                return false;
            };

            return base.Validate(move)
                && isThereAnyOtherMoveInsideTheSquareWithLengthAsTheWinningCondition(move);
        }
    }
    }
