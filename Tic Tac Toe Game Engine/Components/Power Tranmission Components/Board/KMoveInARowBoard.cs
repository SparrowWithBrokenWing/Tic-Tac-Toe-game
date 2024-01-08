using TicTacToeGameEngine.Output.Move;
using TicTacToeGameEngine.Participant.Player;
using TicTacToeGameEngine.Rule;

namespace TicTacToeGameEngine.GameStateDescriptor.Board
{
    // I know that this kind of board doesn't make any sense. I will try to find another solution on upcoming update.
    // This kind of board will contain only move that is the last move of a win line segment.
    public partial class KMoveInARowBoard : AbstractBoard
    {
        // length of a win line segment.
        public uint K { get; protected set; }

        public KMoveInARowBoard(
            uint numberOfMoveNeededToCompleteAWinLineSegment)
        {
            K = numberOfMoveNeededToCompleteAWinLineSegment;
        }
    }

    partial class KMoveInARowBoard : IEndgameRule<IMove>
    {

        public bool IsSatisfied(IMove obj)
        {
            if (obj is LegalMove)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // A legal move should be move that is the last move in a line segment of K move of the same player.
        public abstract partial class LegalMove : IDecisiveMove<IMove>
        {
            public uint K => Board.K;

            public IPlayer Player { get; protected set; }

            // where move is played.
            protected KMoveInARowBoard Board { get; set; }

            public LegalMove(
                IPlayer player,
                KMoveInARowBoard board)
            {
                Board = board;
                Player = player;
            }

            public bool IsSatisfied(IMove obj)
            {
                // if in the same line, there are already k - 1 continuous move then the k move, this move, is a move that is legal move with this kind of board.

                if (obj is ILinkedToAdjacentMove move)
                {
                    var countNumberOfContinuousMoveFromSamePlayer =
                       (Direction direction,
                       Direction oppositeDirection,
                       ILinkedToAdjacentMove startPoint) =>
                           {
                               uint countedMove = 0;
                               for (int i = 0; i < K - 1 && move.Player == GetMove(oppositeDirection).Player; i++)
                               {
                                   countedMove++;
                               }
                               for (int i = 0; i < K - 1 && move.Player == GetMove(oppositeDirection).Player; i++)
                               {
                                   countedMove++;
                               }
                               return countedMove;
                           };

                    if (countNumberOfContinuousMoveFromSamePlayer(Direction.NORTH, Direction.SOUTH, move) > 0)
                    {
                        return true;
                    }
                    if (countNumberOfContinuousMoveFromSamePlayer(Direction.EAST, Direction.WEST, move) > 0)
                    {
                        return true;
                    }
                    if (countNumberOfContinuousMoveFromSamePlayer(Direction.NORTHEAST, Direction.SOUTHWEST, move) > 0)
                    {
                        return true;
                    }
                    if (countNumberOfContinuousMoveFromSamePlayer(Direction.NORTHWEST, Direction.SOUTHEAST, move) > 0)
                    {
                        return true;
                    }

                    return false;
                }
                else
                {
                    throw new ArgumentException("obj need to instance of ILinkedToAdjacentMove.");
                }
            }
        }

        abstract partial class LegalMove : ILinkedToAdjacentMove
        {
            public abstract bool Equals(IMove? other);

            public abstract IMove GetMove(Direction direction);
        }
    }
}
