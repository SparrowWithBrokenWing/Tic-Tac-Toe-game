using TicTacToeGameEngine.Output.Move;
using TicTacToeGameEngine.Participant.Player;
using TicTacToeGameEngine.Rule;

namespace TicTacToeGameEngine.GameStateDescriptor.Board;

public partial class LimitedMoveBoard
{
    public uint NumberOfColumn { get; protected set; }
    public uint NumberOfRow { get; protected set; }

    public LimitedMoveBoard(uint numberOfRow, uint numberOfColumn)
    {
        NumberOfRow = numberOfRow;
        NumberOfColumn = numberOfColumn;
    }
}

partial class LimitedMoveBoard : AbstractBoard
{
    public override void ApplyPlayerMove(IMove move)
    {
        if (!IsSatisfied(move))
        {
            throw new ArgumentException("Cannot accept illegal move.");
        }
        base.ApplyPlayerMove(move);
    }
}

partial class LimitedMoveBoard : IMovementRule<IMove>
{
    // need to make sure the move is inside the board and it shouldn't overlap with other move.
    public bool IsSatisfied(IMove obj)
    {
        // if obj is an intance of LegalMove class, then the it will be satisfied the rule that it need to satisfied because if it doesn't, an exception will be thrown and no instance can be created. But if obj is an intance of a class that inherited from LegalMove class, then there is a chance that the rule isn't satisfied then the board need to recheck if is rule satisfied or not.
        if (obj is not LegalMove)
        {
            // need a message here.
            throw new ArgumentException();
        }

        var newMove = (LegalMove)obj;
        // check if is new move inside board. recheck the rule to make sure new move satisfy rule.
        if (!newMove.IsSatisfied(newMove))
        {
            return false;
        }

        // if there any played in the same position that new move will be played, then new move is a illegal move.
        if (Moves.Any((playedMove) => playedMove is LegalMove && playedMove.Equals(obj)))
        {
            return false;
        }

        return true;
    }

    public abstract partial class LegalMove : IIndexedMove
    {
        public uint RowIndex { get; protected set; }
        public uint ColumnIndex { get; protected set; }
        public IPlayer Player { get; protected set; }
        public LimitedMoveBoard Board { get; protected set; }

        public LegalMove(uint rowIndex, uint columnIndex, IPlayer player, LimitedMoveBoard board)
        {
            RowIndex = rowIndex;    
            ColumnIndex = columnIndex;

            Player = player;
            Board = board;

            if (!IsSatisfied(this))
            {
                // need to find a good message here that tell the rule of a legal move inside this kind of board isn't satisfied.
                throw new ArgumentException();
            }
        }

        public bool Equals(IMove? other)
        {
            if (other is null)
            {
                throw new ArgumentNullException("Cannot compare with a null.");
            }

            if (other is not IIndexedMove)
            {
                throw new ArgumentException("Cannot compared.");
            }

            if (!(IsSatisfied(other)))
            {
                throw new ArgumentException("The other move is not a legal move.");
            }

            IIndexedMove otherMove = (IIndexedMove)other;
            if (otherMove.RowIndex == RowIndex 
                && otherMove.ColumnIndex == ColumnIndex 
                && otherMove.Player == Player)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    partial class LegalMove : IMovementRule<IMove>
    {
        public bool IsSatisfied(IMove obj)
        {
            if (obj is not IIndexedMove)
            {
                throw new ArgumentException("Needs to be an instance of IIndexedMove.");
            }

            IIndexedMove newIndexedMove = (IIndexedMove)obj;

            // check if move is put inside board or not.
            if ((newIndexedMove.RowIndex > 0 && newIndexedMove.RowIndex <= Board.NumberOfRow)
               && (newIndexedMove.ColumnIndex > 0 && newIndexedMove.ColumnIndex <= Board.NumberOfColumn))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
