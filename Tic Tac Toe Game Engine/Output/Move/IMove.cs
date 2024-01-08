using TicTacToeGameEngine.Participant.Player;

namespace TicTacToeGameEngine.Output.Move
{
    public interface IMove : IEquatable<IMove>
    {
        // I don't know that should I define player in here or not cause I hard to tell if move one the same position but played by different player in different time, is the same or not. The main reason I put the information about player move here cause it is easier to develop when know who play that move.
        public IPlayer Player { get; }
    }
}
