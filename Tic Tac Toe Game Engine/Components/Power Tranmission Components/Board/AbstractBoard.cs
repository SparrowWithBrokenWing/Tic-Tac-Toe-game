using TicTacToeGameEngine.Output.Move;
using TicTacToeGameEngine.Participant;

namespace TicTacToeGameEngine.GameStateDescriptor.Board
{
    public abstract partial class AbstractBoard : IBoard
    {
        private List<IMove> moves = new List<IMove>();

        public IReadOnlyList<IMove> Moves
        {
            get => moves;
        }

        public virtual void ApplyPlayerMove(IMove move)
        {
            moves.Add(move);
        }
    }
}
