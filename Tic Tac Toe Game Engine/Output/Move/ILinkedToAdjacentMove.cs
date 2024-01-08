namespace TicTacToeGameEngine.Output.Move
{
    public enum Direction
    {
        NORTH,
        SOUTH,
        EAST,
        WEST,
        NORTHEAST,
        NORTHWEST,
        SOUTHEAST,
        SOUTHWEST
    }

    public interface ILinkedToAdjacentMove : IMove
    {
        public IMove GetMove(Direction direction);
    }
}
