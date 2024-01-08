using TicTacToeGameEngine.Output;
using TicTacToeGameEngine.Output.Move;

namespace TicTacToeGameEngine.Participant.Player
{
    public abstract partial class AbstractNeutralPlayer : IPlayer
    {
        public virtual Task<IMove> GetNextMove() => throw new NotSupportedException();
        public virtual void OnCompleted() => throw new NotSupportedException();
        public virtual void OnNext(IMove value) => throw new NotSupportedException();
        public virtual void OnNext(MatchResult value) => throw new NotSupportedException();

        // I don't know how should this class handle this exception that maybe come from itselt when observalbe try to call any unsupported method.
        public abstract void OnError(Exception error);
    }

    partial class AbstractNeutralPlayer : IEquatable<IPlayer>
    {
        public virtual bool Equals(IPlayer? other)
        {
            if (other is null)
            {
                throw new ArgumentNullException();
            }

            if (other is not AbstractNeutralPlayer)
            {
                return false;
            }

            return ReferenceEquals(this, other);
        }

        protected class NeutralPlayer : AbstractNeutralPlayer
        {
            public override Task<IMove> GetNextMove() => throw new NotSupportedException();
            public override void OnCompleted() => throw new NotSupportedException();
            public override void OnError(Exception error) => throw new NotSupportedException();
            public override void OnNext(IMove value) => throw new NotSupportedException();
            public override void OnNext(MatchResult value) => throw new NotSupportedException();
        }
    }

    
}
