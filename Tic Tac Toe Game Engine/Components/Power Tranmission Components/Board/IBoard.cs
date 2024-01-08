using TicTacToeGameEngine.Output.Move;

namespace TicTacToeGameEngine.GameStateDescriptor
{
    public interface IBoard
    {
        // provide a way to get the status of board if have access to an instance.
        public IReadOnlyList<IMove> Moves { get; }
        // provide a way to update the status of board if need to when have access to an instance.
        public void ApplyPlayerMove(IMove move);
    }
}
