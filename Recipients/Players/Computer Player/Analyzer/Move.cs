namespace ComputerPlayer
{
    public interface IMove : IEquatable<IMove>
    {
        public int Row { get; }
        public int Column { get; }
        public IPlayer Player { get; }
    }

    public class Move : IMove
    {
        public Move(int row, int column, IPlayer player, IMoveLog currentBoardState)
        {
            if (row < 0 || column < 0)
            {
                throw new ArgumentException();
            }

            if (row > currentBoardState.PlayingBoard.NumberOfRows
                || column > currentBoardState.PlayingBoard.NumberOfColumns)
            {
                throw new ArgumentException();
            }

            Row = row;
            Column = column;
            Player = player;
        }

        public int Row { get; private set; }
        public int Column { get; private set; }
        public virtual IPlayer Player { get; private set; }

        public bool Equals(IMove? other)
        {
            // this is something I cannot decide here. It is true that if 2 move from different match, which have different board state before it can be played, then they are different. But, first, I cannot see what will I do with it at this moment and second, it won't make infinite recursion (because the first move won't have any played move at all if everything go as expected) but it will take very long to check. So I left this comment here so that someone can see it and answer it someday.
            return other is not null
                && this.Row == other.Row
                && this.Column == other.Column
                && this.Player == other.Player;
            //&& this.BoardStateBeforeBeingPlayed.Equals(other.BoardStateBeforeBeingPlayed);
        }
    }
}
