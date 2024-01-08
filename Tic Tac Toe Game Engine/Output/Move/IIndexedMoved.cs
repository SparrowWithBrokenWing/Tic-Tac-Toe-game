namespace TicTacToeGameEngine.Output.Move
{
    // force indexed by uint type. I don't know is it good idea, but it is better than define 2 more template for that. And if the board is in 2 dimension form, then it will need exactly 2 index only. I don't think if move indexed, the index shouldn't be negative number or zero.
    public interface IIndexedMove : IMove
    {
        public uint RowIndex { get; }
        public uint ColumnIndex { get; }
    }
}
