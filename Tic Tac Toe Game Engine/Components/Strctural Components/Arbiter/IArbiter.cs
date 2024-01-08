namespace TicTacToeGameEngine.Participant.Arbiter
{
    // the arbiter is the one that take respond to check if a move is a legal move or not.
    public interface IArbiter<T>
    {
        public bool IsRuleSatisfied(T obj);
    }
}