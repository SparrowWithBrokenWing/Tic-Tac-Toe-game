namespace ComputerPlayer
{
    public interface IMoveType { }
    public interface IPossibleMoveType : IMoveType { }
    public interface ITacticalMoveType : IPossibleMoveType { }

    public interface IOffensiveMoveType : ITacticalMoveType { }
    public interface IForkMoveType : IOffensiveMoveType { }
    public interface IWinningMoveType : IOffensiveMoveType { }

    public interface IDefensiveMoveType : ITacticalMoveType { }
    public interface IBlockForkMoveType : IDefensiveMoveType { }
    public interface IBlockWinningMoveType : IDefensiveMoveType { }
}
