using TicTacToeGameEngine.Output;
using TicTacToeGameEngine.Output.Move;

namespace TicTacToeGameEngine.Participant.Player
{
    public abstract partial class AbstractPlayer : IPlayer
    {
        public abstract Task<IMove> GetNextMove();

        public abstract void OnCompleted();

        public abstract void OnError(Exception error);

        public abstract void OnNext(IMove value);

        public abstract void OnNext(MatchResult value);
    }

    partial class AbstractPlayer : IEquatable<IPlayer>
    {
        public bool Equals(IPlayer? other)
        {
            if (other is AbstractNeutralPlayer)
            {
                return false;
            }
            if (other is not null)
            {
                return other.Equals(this);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }
    }
}
