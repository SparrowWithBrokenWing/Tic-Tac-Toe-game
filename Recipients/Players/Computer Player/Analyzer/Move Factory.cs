namespace ComputerPlayer.Analyzer
{
    public interface ICategorizedMove : IMove { }

    public interface IPossibleMove : ICategorizedMove { }
    public interface ITacticalMove : IPossibleMove { }

    public interface IOffensiveMove : ITacticalMove { }
    public interface IForkMove : IOffensiveMove { }
    public interface IWinningMove : IOffensiveMove { }

    public interface IDefensiveMove : ITacticalMove { }
    public interface IBlockForkMove : IDefensiveMove { }
    public interface IBlockWinningMove : IDefensiveMove { }

    public interface IMoveFacotry<TProduct>
        where TProduct : IMove
    {
        public TProduct Produce(int row, int column, IPlayer player);
    }


}