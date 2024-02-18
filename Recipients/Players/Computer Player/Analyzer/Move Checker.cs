namespace ComputerPlayer
{
    public interface IOpponentNextMovePossibilityChecker
    {
        public bool IsPossible(ICategorizedMove categorizedMove);
    }

}