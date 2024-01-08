using TicTacToeGameEngine.GameStateDescriptor;
using TicTacToeGameEngine.Rule;

namespace TicTacToeGameEngine.Output.Move
{
    public interface IDecisiveMove<TMove> : IMove, IEndgameRule<TMove> where TMove : IMove
    {

    }
}
