using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Xml.Xsl;

namespace ComputerPlayer
{
    // from a turn instance, I need to know the move of player and player's opponent that follow an fixed order. the owner of instance can access to move that played in that turn through that order or a player identifier.
    public interface ITurn
    {
        // if index return null, that mean move hasn't played by that player/has been played by opponent (when using IPlayer to index) or the player has that play ordinal hasn't played.

        public IMove? this[IPlayer player] { get; }
        // first player has ordinal as 0.
        public IMove? this[uint ordinal] { get; }
    }

    //public class TurnBuilder
    //{
    //    public TurnBuilder(Queue<IPlayer> playOrder)
    //    {
    //        PlayOrder = playOrder;

    //        CurrentTurnOrdinal = 0;
    //    }

    //    public uint CurrentTurnOrdinal { private get; set; }
    //    protected Queue<IPlayer> PlayOrder { get; private set; }

    //    public ITurn Build(IMove move)
    //    {
    //        return;
    //    }
    //}

    public interface IMatch
    {
        public IEnumerable<IPlayer> Players { get; }
    }
    public interface ITurnLog
    {
        public IBoard PlayingBoard { get; }
        public ITurn this[uint ordinal] { get; }
    }

    public interface IMoveRetriever : IEnumerable<IMove>, IEnumerable
    {
        public IMove? Retrieve(uint row, uint column);
        //public IEnumerable<IMove> Moves { get; }
    }

    public interface IMoveRecorder
    {
        public void Record(IMove move);
    }

    public interface IMoveLog : IMoveRetriever, IMoveRecorder
    {
        public IBoard PlayingBoard { get; }
    }

    public interface IMatchLog
    {
        public IMatch PlayingMatch { get; }
    }

    public class PlayedMoveLog : IMoveLog
    {
        public PlayedMoveLog(IBoard board, IEnumerable<IMove> playedMoves)
        {
            this.PlayingBoard = board;

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

        public PlayedMoveLog(IBoard board, IMatch match, IDictionary<uint, IDictionary<uint, IMove>> playedMoves)
        {
            this.PlayingBoard = board;
            _PlayedMoves = playedMoves;
        }

        public PlayedMoveLog(IBoard board, IMatch match, Func<IDictionary<uint, IDictionary<uint, IMove>>> boardFactory, Func<IDictionary<uint, IMove>> rowFactory, IEnumerable<IMove> playedMoves)
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

        // only work if move order is unnescessary
        protected IDictionary<uint, IDictionary<uint, IMove>> _PlayedMoves { get; private set; }
        public IBoard PlayingBoard { get; private set; }

        protected IMove? this[uint row, uint column]
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
                    _PlayedMoves[row] = new Dictionary<uint, IMove>();
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

        IMove? IMoveRetriever.Retrieve(uint row, uint column)
        {
            return this[row, column];
        }

        void IMoveRecorder.Record(IMove move)
        {
            this[move.Row, move.Column] = move;
        }

        IEnumerator<IMove> IEnumerable<IMove>.GetEnumerator()
        {
            return new LogEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new LogEnumerator();
        }

        // don't what happen if I define enumerator like this.
        protected struct LogEnumerator : IEnumerator<IMove>, IEnumerator
        {
            public LogEnumerator(IDictionary<uint, IDictionary<uint, IMove>> playedMoves)
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
            private uint _index;

            public IMove Current => !(_playedMoves.Count > 0) || _current is null ? throw new NotImplementedException() : _current;

            object? IEnumerator.Current => !(_playedMoves.Count > 0) ? throw new NotImplementedException() : _current;

            public void Dispose()
            {
                _playedMoves.Clear();
            }

            public bool MoveNext()
            {
                if (_index < (uint)_playedMoves.Count)
                {
                    _current = _playedMoves[(int)_index];
                    _index++;
                    return true;
                }
                else
                {
                    Reset();
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
