namespace ComputerPlayer
{
    public class PossbileMove : AnalyzedMove
    {
        public PossbileMove(IMove move) : base(move)
        {
        }

        protected override bool Validate(IMove move)
        {
            var isItAnUnplayedMove = (IMove move) =>
            {
                foreach (var playedMove in BoardStateBeforeBeingPlayed.PlayedMoves)
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
                return move.Row > 0 && move.Row <= this.BoardStateBeforeBeingPlayed.PlayingBoard.NumberOfRows
                && move.Column > 0 && move.Column <= this.BoardStateBeforeBeingPlayed.PlayingBoard.NumberOfColumns;
            };

            return isItAnUnplayedMove(move)
                && isItInAnAllowedRangeMove(move);
        }
    }
}
