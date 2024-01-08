namespace ComputerPlayer
{
    public interface IBoardState : IEquatable<IBoardState>
    {
        public IBoard PlayingBoard { get; }
        public IEnumerable<IMove> PlayedMoves { get; }
        public IMove? this[uint rowIndex, uint columnIndex] { get; }
    }

    public class BoardState : IBoardState
    {
        public BoardState(IBoard board, IEnumerable<IMove> playedMoves)
        {
            PlayingBoard = board;

            // code smell here! I shouldn't initialize played moves here, but I cannot find better way.
            _PlayedMoves = new Dictionary<uint, IDictionary<uint, IMove>>();
            foreach (var playedMove in playedMoves)
            {
                if (!_PlayedMoves.ContainsKey(playedMove.Row))
                {
                    _PlayedMoves.Add(playedMove.Row, new Dictionary<uint, IMove>());
                }
                _PlayedMoves[playedMove.Row][playedMove.Column] = playedMove;
            }
        }

        protected IDictionary<uint, IDictionary<uint, IMove>> _PlayedMoves { get; private set; }

        public virtual IMove? this[uint row, uint column]
        {
            get
            {
                IMove? move;
                try
                {
                    move = _PlayedMoves[row][column];
                }
                catch (KeyNotFoundException)
                {
                    move = null;
                }
                return move;
            }
            protected set => this[row, column] = value;
        }

        public virtual IEnumerable<IMove> PlayedMoves
        {
            get
            {
                ICollection<IMove> result = new List<IMove>();
                foreach (var keyValuePairOfRow in _PlayedMoves)
                {
                    var row = keyValuePairOfRow.Value;
                    foreach (var keyValuePairOfCell in row)
                    {
                        result.Add(keyValuePairOfCell.Value);
                    }
                }
                return result;
            }
        }

        public IBoard PlayingBoard { get; private set; }

        public virtual bool Equals(IBoardState? other)
        {
            if (other is null)
            {
                return false;
            }
            var thisPlayedMoves = this.PlayedMoves.ToList();
            var otherPlayedMoves = other.PlayedMoves.ToList();

            var countOfThisPlayedMoves = thisPlayedMoves.Count();
            var countOfOtherPlayedMoves = otherPlayedMoves.Count();

            if (countOfThisPlayedMoves != countOfOtherPlayedMoves)
            {
                return false;
            }

            var countOfPlayedMoves = countOfOtherPlayedMoves = countOfThisPlayedMoves;

            for (int i = 0; i < countOfPlayedMoves; i++)
            {
                var isMovesIsInTheSamePositionAndPlayedBySamePlayer = (IMove move1, IMove move2) =>
                {
                    return move1.Row == move2.Row
                        && move1.Column == move2.Column
                        && move1.Player == move2.Player;
                };
                if (!isMovesIsInTheSamePositionAndPlayedBySamePlayer(thisPlayedMoves[i], otherPlayedMoves[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }

}
