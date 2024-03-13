using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Xml.Xsl;

namespace ComputerPlayer
{
    // from a turn instance, I need to know the move of player and player's opponent that follow an fixed order. the owner of instance can access to move that played in that turn through that order or a player identifier.
    public interface ITurn : ICollection<IMove>
    {
        // if index return null, that mean move hasn't played by that player/has been played by opponent (when using IPlayer to index) or the player has that play ordinal hasn't played.
        public IMove? this[IPlayer player] { get; }
        // first player has ordinal as 0.
        public IMove? this[int ordinal] { get; }
    }

    public interface ITurnLog : ICollection<ITurn>
    {
        public IBoard PlayingBoard { get; }
        public ITurn this[int turnOrdinal] { get; }
    }

    public interface IMoveRetriever : IEnumerable<IMove>, IEnumerable
    {
        public IMove? Retrieve(int row, int column);
    }

    public interface IMoveRecorder
    {
        public void Record(IMove move);
    }

    public interface IMoveLog : IMoveRetriever, IMoveRecorder
    {
        public IBoard PlayingBoard { get; }
    }

    public class PlayedMoveLog : IMoveLog
    {
        public PlayedMoveLog(IBoard board, IDictionary<int, IDictionary<int, IMove>> playedMoves)
        {
            this.PlayingBoard = board;
            _PlayedMoves = playedMoves;
        }

        public PlayedMoveLog(
            IBoard board, 
            Func<IDictionary<int, IDictionary<int, IMove>>> boardFactory, 
            Func<IDictionary<int, IMove>> rowFactory, 
            IEnumerable<IMove> playedMoves)
        {
            this.PlayingBoard = board;
            _PlayedMoves = boardFactory();
            foreach (var playedMove in playedMoves)
            {
                if (!_PlayedMoves.ContainsKey(playedMove.Row))
                {
                    _PlayedMoves.Add(playedMove.Row, rowFactory());
                }
                _PlayedMoves[playedMove.Row][playedMove.Column] = playedMove;
            }
        }

        public PlayedMoveLog(IBoard board, IEnumerable<IMove> playedMoves)
        {
            this.PlayingBoard = board;

            _PlayedMoves = new Dictionary<int, IDictionary<int, IMove>>();
            foreach (var playedMove in playedMoves)
            {
                if (!_PlayedMoves.ContainsKey(playedMove.Row))
                {
                    _PlayedMoves.Add(playedMove.Row, new Dictionary<int, IMove>());
                }
                _PlayedMoves[playedMove.Row][playedMove.Column] = playedMove;
            }
        }

        // only work if move order is unnescessary
        protected IDictionary<int, IDictionary<int, IMove>> _PlayedMoves { get; private set; }
        public IBoard PlayingBoard { get; private set; }

        protected IMove? this[int row, int column]
        {
            get
            {
                try
                {
                    return _PlayedMoves[row][column];
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
            }
            set
            {
                if (_PlayedMoves[row] is null)
                {
                    _PlayedMoves[row] = new Dictionary<int, IMove>();
                }

                if (_PlayedMoves[row][column] is not null)
                {
                    throw new ArgumentException();
                }

                if (value is not null)
                {
                    _PlayedMoves[row][column] = value;
                }
            }
        }

        public virtual IEnumerable<IMove> Moves
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

        IMove? IMoveRetriever.Retrieve(int row, int column)
        {
            return this[row, column];
        }

        void IMoveRecorder.Record(IMove move)
        {
            this[move.Row, move.Column] = move;
        }

        IEnumerator<IMove> IEnumerable<IMove>.GetEnumerator()
        {
            return new LogEnumerator(this._PlayedMoves);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new LogEnumerator(this._PlayedMoves);
        }

        // don't what happen if I define enumerator like this.
        protected struct LogEnumerator : IEnumerator<IMove>, IEnumerator
        {
            public LogEnumerator(IDictionary<int, IDictionary<int, IMove>> playedMoves)
            {
                _playedMoves = new List<IMove>();
                foreach (var keyValuePairOfRow in playedMoves)
                {
                    var row = keyValuePairOfRow.Value;
                    foreach (var keyValuePairOfCell in row)
                    {
                        _playedMoves.Add(keyValuePairOfCell.Value);
                    }
                }
                _index = 0;
                if (_playedMoves.Count > 0)
                {
                    _current = _playedMoves[0];
                }
                else
                {
                    _current = null;
                }
            }

            private IList<IMove> _playedMoves;
            private IMove? _current;
            private int _index;

            public IMove Current => !(_playedMoves.Count > 0) || _current is null ? throw new NotImplementedException() : _current;

            object? IEnumerator.Current => !(_playedMoves.Count > 0) ? throw new NotImplementedException() : _current;

            public void Dispose()
            {
                _playedMoves.Clear();
            }

            public bool MoveNext()
            {
                if (_index < (int)_playedMoves.Count)
                {
                    _current = _playedMoves[(int)_index];
                    _index++;
                    return true;
                }
                else
                {
                    //Reset();
                    return false;
                }
            }

            public void Reset()
            {
                _index = 0;
                if (_playedMoves.Count > 0)
                {
                    _current = _playedMoves[0];
                }
                else
                {
                    _current = null;
                }
            }
        }

    }
}
