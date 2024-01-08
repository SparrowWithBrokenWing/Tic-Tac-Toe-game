using TicTacToeGameEngine.Participant.Player;

namespace TicTacToeGameEngine.Input
{
    public sealed class Turn
    {
        public IPlayer PlayingPlayer { get; protected set; }

        public Turn(IPlayer playingPlayer)
        {
            PlayingPlayer = playingPlayer;
        }
    }
}
