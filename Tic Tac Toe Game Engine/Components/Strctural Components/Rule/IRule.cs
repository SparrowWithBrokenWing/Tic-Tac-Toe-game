namespace TicTacToeGameEngine.Rule
{
    // let's take example with playable time. The rule will be sastisfied if the player make their move in time, so this instance of rule need a reference to the playable time left of current player.
    // T is template for type of object that will be check if that obj sastisify the rule or not.
    public interface IMovement<T>
    {
        public bool IsSatisfied(T obj);
    }
}
