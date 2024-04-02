namespace ComputerPlayer.Analyzer
{
    // because component of tree don't change its categoried after it is intialized (because its categorized base on state of the board - or played moves between 2 players) so it should be the factory analyze the move category before make it out. But because there is no way that a factory - which need to work with every case of board state - now have to change the design so that a board state - a list of played moves - be allowed to add as parameter of produce method of factory class. The only place that is possible to get the fully played move, the thing that fully understand the sittuation that a move is in is the tree. but because the category - which can be considered as state of move - will never change after the moment it is created, which mean the type of move - the type of tree component - won't change either. So, the move categories - unstable; and the type - fixed but because those two bound with each other, it means both need to be unstable or to be fixed at the same time. Which mean I may have to change the design one more time
    public interface IMoveFacotry<TProduct>
        where TProduct : IMove
    {
        public TProduct Produce(int row, int column, IPlayer player);
    }
}