namespace ComputerPlayer
{
    public interface IBoard
    {
        public int WinningCondition { get; }
        public int NumberOfRows { get; }
        public int NumberOfColumns { get; }
    }
}
