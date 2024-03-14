namespace ComputerPlayer.Analyzer
{

    public interface IMoveFacotry<TProduct>
        where TProduct : IMove
    {
        public TProduct Produce(int row, int column, IPlayer player);
    }


}