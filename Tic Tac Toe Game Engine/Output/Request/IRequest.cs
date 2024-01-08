namespace TicTacToeGameEngine.Output.Request
{
    // I don't want to focus at this ability at this time. A player will send request to match manager and match manager will send that request to other player, and other player will respond with another request. To use this ability then normal observer need to used in a bidirectional way.
    public interface IRequest
    {
        public bool Request();
    }
}
