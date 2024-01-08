using TicTacToeGameEngine.Output;
using TicTacToeGameEngine.Output.Move;

namespace TicTacToeGameEngine.Participant.Player
{
    // You will need to define the implementation of IPlayer interface with explicit interface implementation. There is no other way to do that, and I think it is necessary to do this. This is better than rename method.
    public interface IPlayer : IObserver<IMove>, IObserver<MatchResult>, IEquatable<IPlayer>
    {
        public Task<IMove> GetNextMove();
    }
}
