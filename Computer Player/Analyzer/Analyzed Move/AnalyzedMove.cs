namespace ComputerPlayer
{

    public abstract class AnalyzedMove : Move
    {
        public AnalyzedMove(IMove move)
            : base(move.Row,move.Column,move.Player,move.BoardStateBeforeBeingPlayed)
        {
            if (!Validate(move))
            {
                throw new ArgumentException();
            }
        }

        // this is a code smell. I understand that left everything like this would be tricky, but I cannot find a better way to do this. I hope someone can help me find out is there any better idea to do this. 
        // Ps: The most important point is, the analyzed move need to consider every previous analyzed move as a played move with played move that is actually played.
        public AnalyzedMove(AnalyzedMove analyzedMove)
            : base(analyzedMove.Row, 
                  analyzedMove.Column, 
                  analyzedMove.Player, 
                  new BoardState(
                      analyzedMove.BoardStateBeforeBeingPlayed.PlayingBoard,
                      analyzedMove.BoardStateBeforeBeingPlayed.PlayedMoves.ToList().Concat(GetPreviousAnalyzedMovesCollection(analyzedMove))))
        {
            if (!Validate(analyzedMove))
            {
                throw new ArgumentException();
            }
        }

        protected IMove? PreviousMove { get; set; }

        protected IEnumerable<IMove>? NextMoves { get; set; }

        protected abstract bool Validate(IMove move);

        private static IEnumerable<IMove> GetPreviousAnalyzedMovesCollection(AnalyzedMove analyzedMove)
        {
            var result = new List<IMove>();
            AnalyzedMove currentCheckedMove = analyzedMove;
            while (currentCheckedMove.PreviousMove is not null)
            {
                result.Add(currentCheckedMove);
            }
            return result;
        }
    }
}
