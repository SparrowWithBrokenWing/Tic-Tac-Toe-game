namespace ComputerPlayer
{
    
    public interface IMoveEvaluator<TRate>
    {
        public TRate Evaluate(IMove move);
    }

    public class RateEvaluator : IMoveEvaluator<float>
    {
        public float Evaluate(IMove move)
        {
            if (move is not INextMovesPredictedMove)
            {
                throw new ArgumentException();
            }

            var nextMovesPredictedMove = (INextMovesPredictedMove)move;

            foreach (var nextMove in nextMovesPredictedMove.PredictedNextMoves)
            {

            }

            var result = 0f;

            return result;
        }
    }

    public interface IPlayer : IEquatable<IPlayer>
    {

    }

    public interface IBoard
    {
        public uint WinningCondition { get; }
        public uint NumberOfRows { get; }
        public uint NumberOfColumns { get; }
    }

    public interface IPreviousMoveSpecifiableMove : IMove
    {
        public IMove PreviousMove { set; }
    }

    public interface IPreviousMoveAccessibleMove : IMove
    {
        public IMove PreviousMove { get; }
    }

    public interface INextMovesPredictableMove : IMove
    {
        public ICollection<IMove> PredictableNextMoves { get; }
    }

    public interface INextMovesPredictedMove : IMove
    {
        public IEnumerable<IMove> PredictedNextMoves { get; }
    }

    public interface IMovePool
    {
        public IMove this[uint row, uint column] { get; }
        public IEnumerable<IMove> PlayedMove { get; }
    }

    public abstract class PredictedMove : IMove
    {
        protected PredictedMove(uint row, uint column, IPlayer player, IMoveTracker boardStateBeforeBeingPlayed)
        {
        }

        private IMove _move { get; set; }

        public uint Row { get; private set; }

        public uint Column { get; private set; }

        public IPlayer Player { get; private set; }

        public IMoveTracker BoardStateBeforeBeingPlayed => throw new NotImplementedException();

        public bool Equals(IMove? other)
        {
            throw new NotImplementedException();
        }
    }

    public interface ICategorizableMove : IMove
    {
        public ICollection<IMoveType> CategorizableMoveTypes { get; }
    }

    public interface ICategorizedMove : IMove
    {
        public IEnumerable<IMoveType> CategorizedMoveTypes { get; }
    }

    public class Tree
    {
        public Tree(uint allowedMaxHeight)
        {
            _AllowedMaxHeight = allowedMaxHeight;
            // when act on tree, check whether that branch has been fully developed or not. If not, add that branch to this list. The regenerate method should work until this list is empty
            // code smell
            _UnfullyGrowthBranchs = new List<INextMovesPredictableMove>();
        }

        public IMoveTracker PlayingBoardState { protected get; set; }
        protected uint _AllowedMaxHeight { get; private set; }
        protected ICategorizer _Categorizer { get; set; }
        protected ICollection<INextMovesPredictableMove> _UnfullyGrowthBranchs { get; private set; }
        public INextMovesPredictedMove Root { private get; set; }
        public uint CurrentMaxHeight
        {
            get => Task.Run(() => GetBranchMaxHeight(this.Root)).Result;
        }

        private async Task<uint> GetBranchMaxHeight(IMove move)
        {
            if (move is INextMovesPredictedMove nextPredictedMoves
                && nextPredictedMoves.PredictedNextMoves.Count() > 0)
            {
                var runningTasks = new List<Task<uint>>();
                uint maxBranchHeight = 0;
                foreach (var nextPredictedMove in nextPredictedMoves.PredictedNextMoves)
                {
                    runningTasks.Add(GetBranchMaxHeight(nextPredictedMove).ContinueWith((task) =>
                    {
                        if (task.Result > maxBranchHeight)
                        {
                            maxBranchHeight = task.Result;
                        }
                        return maxBranchHeight;
                    }));
                }
                await Task.WhenAll(runningTasks);
                return 1 + maxBranchHeight;
            }
            else
            {
                return 0;
            }
        }

        private uint GetBranchHeight(IPreviousMoveAccessibleMove referenceablePreviousMoveMove)
        {
            IMove currentCheckingMove = referenceablePreviousMoveMove;
            uint result = 0;
            while (currentCheckingMove is IPreviousMoveAccessibleMove previousMoveReferenceableMove)
            {
                result++;
                currentCheckingMove = previousMoveReferenceableMove;
            }
            return result;
        }

        protected class TrunkMove : IPreviousMoveAccessibleMove, INextMovesPredictableMove, INextMovesPredictableMove, ICategorizableMove, ICategorizedMove
        {

        }

        protected class LeafMove : IPreviousMoveAccessibleMove, IPreviousMoveSpecifiableMove, ICategorizableMove, ICategorizedMove
        {

        }

        // this method alway generate next predicted move from the player who played the move that passed to this method. I should change the played move list that need to be devided by turn, each turn have 2 player move. A turn can be in incomplete state, which is the value of player property is not set to anything. I don't know should I do like this or not. 
        // this method should generate next moves for the player who is provided by the caller even that he is not the next player who can play.
        private async Task<INextMovesPredictedMove> Generate(INextMovesPredictableMove nextMovesPredictableMove, IEnumerable<IMove> playedMove, IPlayer player)
        {

        }

        public Task<Tree> Regenerate()
        {
            while (_UnfullyGrowthBranchs.Count > 0)
            {

            }
        }

        public class NextMovesPredictedMove : INextMovesPredictableMove, INextMovesPredictedMove
        {

        }


        class MeaningPlayableMovesFinder
        {
            public IEnumerable<IMove> Find(IMove move, IEnumerable<IMove> playedMoves)
            {
                var playedMoves = move.BoardStateBeforeBeingPlayed.PlayedMoves;
                var playingBoard = move.BoardStateBeforeBeingPlayed.PlayingBoard;
                var winningCondition = move.BoardStateBeforeBeingPlayed.PlayingBoard.WinningCondition;
                var numberOfRowsInBoard = move.BoardStateBeforeBeingPlayed.PlayingBoard.NumberOfRows;
                var numberOfColumnInBoard = move.BoardStateBeforeBeingPlayed.PlayingBoard.NumberOfColumns;

                var result = new List<IMove>();

                if (move is INextMovesPredictedMove
                    && move is IPreviousMovePredicted predictedMove)
                {
                    var currentCheckedMove = predictedMove.PredictedPreviousMove;
                    while (currentCheckedMove is IPreviousMovePredicted
                        && currentCheckedMove is INextMovesPredictedMove)
                    {

                    }

                    return result;
                }

                var rowIndex = move.Row - (winningCondition - 1);
                rowIndex = rowIndex > 0 ? rowIndex : 0;
                while (true)
                {
                    if (!(rowIndex >= 0
                        && rowIndex < numberOfRowsInBoard
                        && rowIndex - move.Row < winningCondition))
                    {
                        break;
                    }

                    var columnIndex = move.Column - (winningCondition - 1);
                    columnIndex = columnIndex > 0 ? columnIndex : 0;
                    while (true)
                    {
                        if (!(columnIndex >= 0
                            && columnIndex < numberOfColumnInBoard
                            && columnIndex - move.Column < winningCondition))
                        {
                            break;
                        }

                        var checkedMove = move.BoardStateBeforeBeingPlayed[rowIndex, columnIndex];
                        if (checkedMove is not null)
                        {
                            result.Add(checkedMove);
                        }

                        columnIndex++;
                    }

                    rowIndex++;
                }

                return result;
            }
        }
    }

    // analyzer will chose the best move with most possible score.
    public class Analyzer
    {

    }
}
