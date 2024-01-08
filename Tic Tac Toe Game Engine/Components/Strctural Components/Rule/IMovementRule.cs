using TicTacToeGameEngine.Output.Move;

namespace TicTacToeGameEngine.Rule
{
    // Note: TMove - Template for Move type
    public interface IMovementRule<TMove> : IMovement<TMove> where TMove : IMove
    {

    }
}
