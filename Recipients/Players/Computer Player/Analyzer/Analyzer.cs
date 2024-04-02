namespace ComputerPlayer.Analyzer
{
    public interface IAnalyzer
    {
        public Tuple<int, int> Analyze();
    }

    public abstract class AbstractAnalyzer<TTreeComponent> : IAnalyzer
        where TTreeComponent : ITreeComponent, IEvaluableMove, IEvaluatedMove, ICategorizableMove, ICategorizedMove
    {
        public AbstractAnalyzer(
            IPlayer analyzedPlayer,
            Tuple<IPlayer, IPlayer> players,
            IMoveCategorizer moveCategorizer,
            IMoveRetriever playedMoveRetriever,
            IBoard board,
            AbstractTree<TTreeComponent> tree,
            IMoveEvaluator<IMove> moveEvaluator)
        {
            if (!(analyzedPlayer.Equals(players.Item1) || analyzedPlayer.Equals(players.Item2)))
            {
                throw new ArgumentException();
            }
            AnalyzedPlayer = analyzedPlayer;
            _players = players;
            MoveCategrozier = moveCategorizer;
            PlayedMoveRetriever = playedMoveRetriever;
            Board = board;
            PredictionTree = tree;
            MoveEvaluator = moveEvaluator;
        }

        protected IPlayer AnalyzedPlayer { get; private set; }
        private Tuple<IPlayer, IPlayer> _players;
        protected IPlayer CurrentPlayablePlayer { get => PlayedMoveRetriever.Last().Equals(_players.Item1) ? _players.Item2 : _players.Item1; }

        // for categorizing move
        protected IMoveCategorizer MoveCategrozier { get; private set; }
        protected IMoveRetriever PlayedMoveRetriever { get; private set; }
        protected IBoard Board { get; private set; }

        // for calculating move value
        protected ITree PredictionTree { get; private set; }
        protected IMoveEvaluator<IMove> MoveEvaluator { get; private set; }
        

        public abstract Tuple<int, int> Analyze();
    }

    public interface IAnalyzableMove : ITreeComponent, IEvaluableMove, IEvaluatedMove, ICategorizableMove, ICategorizedMove
    {

    }

    public class UnoptimizedAnalyzer : AbstractAnalyzer<IAnalyzableMove>
    {
        public UnoptimizedAnalyzer(
            IPlayer analyzedPlayer,
            Tuple<IPlayer, IPlayer> players,
            IMoveCategorizer moveCategorizer,
            IMoveRetriever playedMoveRetriever,
            IBoard board,
            AbstractTree<IAnalyzableMove> tree,
            IMoveEvaluator<IMove> moveEvaluator)
            : base(analyzedPlayer, players, moveCategorizer, playedMoveRetriever, board, tree, moveEvaluator)
        {
        }

        // need to prune and cutting tree. but when? no, the tree should be pruned or cutting in main program
        public override Tuple<int, int> Analyze()
        {
            // categorize move type of all predicted moves
            Action<IMove>? categorize = null;
            categorize = (IMove move) =>
            {
                if (move is ICategorizableMove categorizableMove)
                {
                    base.MoveCategrozier.Categorize(categorizableMove, PlayedMoveRetriever, Board);
                    if (categorizableMove is INextMovesPredictedMove nextMovesPredictedMove)
                    {
                        foreach (var predictedNextMove in nextMovesPredictedMove.PredictedNextMoves)
                        {
                            if (categorize is not null)
                            {
                                categorize(predictedNextMove);
                            }
                        }
                    }
                }
            };
            if (categorize is not null)
            {
                categorize(base.PredictionTree.Root);
            }

            // get the move with best value from all player predicted moves
            IPlayer analyzedPlayer = AnalyzedPlayer;
            if (!base.PredictionTree.Root.Player.Equals(analyzedPlayer))
            {
                throw new NotImplementedException();
            }
            var rowOfBestLocation = -1;
            var columnOfBestLocation = -1;
            float maxValue = 0;
            foreach (var predictedNextMove in base.PredictionTree.Root.PredictedNextMoves)
            {
                var valueOfPredictedMove = base.MoveEvaluator.Evaluate(predictedNextMove);
                if (maxValue < valueOfPredictedMove)
                {
                    maxValue = valueOfPredictedMove;
                    rowOfBestLocation = predictedNextMove.Row;
                    columnOfBestLocation = predictedNextMove.Column;
                }
            }

            if (rowOfBestLocation == -1 || columnOfBestLocation == -1)
            {
                throw new Exception();
            }

            return new Tuple<int, int>(rowOfBestLocation, columnOfBestLocation);
        }
    }
}