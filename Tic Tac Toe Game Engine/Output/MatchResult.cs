using TicTacToeGameEngine.Participant.Player;

namespace TicTacToeGameEngine.Output
{
    public class MatchResult
    {
        public IPlayer Winner { get; protected set; }
        public MatchResult(IPlayer winner)
        {
            Winner = winner;
        }
    }
}
