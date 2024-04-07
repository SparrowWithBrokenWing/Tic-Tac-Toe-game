namespace ComputerPlayer.Analyzer
{
    public interface IAnalyzer
    {
        public Tuple<int, int> Analyze();
    }

    public interface IAnalyzableTreeComponent : ITreeComponent, ICategorizableMove, ICategorizedMove { }
    public interface IAnalyzableTreeBranch : ITreeBranch, IAnalyzableTreeComponent { }
    public interface IAnalyzableTreeLeaf : ITreeLeaf, IAnalyzableTreeComponent { }
    public interface IAnalyzableTreeFlower : ITreeFlower, IAnalyzableTreeComponent { }
    public interface IAnalyzableTreeFruit : ITreeFruit, IAnalyzableTreeComponent { }

    public class AnalyzerImplement1 : IAnalyzer
    {
        public AnalyzerImplement1(
            IPlayer analyzedPlayer,
            Tuple<IPlayer, IPlayer> players,
            IBoard board,
            TreeImplement1<IAnalyzableTreeComponent, IAnalyzableTreeBranch, IAnalyzableTreeLeaf, IAnalyzableTreeFlower, IAnalyzableTreeFruit> tree,
            IMoveEvaluator<IMove> moveEvaluator)
        {
            if (!(analyzedPlayer.Equals(players.Item1) || analyzedPlayer.Equals(players.Item2)))
            {
                throw new ArgumentException();
            }
            AnalyzedPlayer = analyzedPlayer;
            Players = players;
            Board = board;
            AnalyzedTree = tree;
            MoveEvaluator = moveEvaluator;
        }

        protected IPlayer AnalyzedPlayer { get; private set; }
        protected Tuple<IPlayer, IPlayer> Players { get; private set; }
        protected IBoard Board { get; private set; }
        protected ITree AnalyzedTree { get; private set; }
        protected IMoveEvaluator<IMove> MoveEvaluator { get; private set; }

        // need to prune and cutting tree. but when? no, the tree should be pruned or cutting in main program. THERE IS NO WAY TO KNOW WHICH BRANCH SHOULD BE PRUNE!!! what exactly the condition? there alsway a horizon come from tree, so there is no way to gurantee the best move is actually the worst move. This is the best implement I can define. Just move to test code.
        public Tuple<int, int> Analyze()
        {
            if (!AnalyzedTree.PredictedNextMoves.First().Player.Equals(AnalyzedPlayer))
            {
                throw new NotImplementedException();
            }

            Func<IMove, float>? evaluate = null;
            evaluate = (evaluatedMove) =>
            {
                float evaluateValue =(AnalyzedPlayer.Equals(evaluatedMove.Player) ? 1 : -1) * MoveEvaluator.Evaluate(evaluatedMove);
                if (evaluatedMove is INextMovesPredictedMove nextMovesPredictedMove)
                {
                    evaluateValue *= (float).65;
                    float predictedNextMovesValue = 0;
                    var numberOfPredictedNextMoves = nextMovesPredictedMove.PredictedNextMoves.Count();
                    foreach (var predictedNextMove in nextMovesPredictedMove.PredictedNextMoves)
                    {
                        if (predictedNextMove is INextMovesPredictedMove sub
                        && evaluate is not null)
                        {
                            predictedNextMovesValue += evaluate(sub);
                        }
                        else
                        {
                            predictedNextMovesValue += MoveEvaluator.Evaluate(predictedNextMove) * 1 / numberOfPredictedNextMoves * (AnalyzedPlayer.Equals(nextMovesPredictedMove.Player) ? 1 : -1);
                        }
                    }
                    predictedNextMovesValue *= (float).35;
                    predictedNextMovesValue /= numberOfPredictedNextMoves;
                    evaluateValue += predictedNextMovesValue;
                }
                return evaluateValue;
            };

            var rowOfBestLocation = -1;
            var columnOfBestLocation = -1;
            float maxValue = 0;
            foreach (var predictedNextMove in AnalyzedTree.PredictedNextMoves)
            {
                var valueOfPredictedMove = evaluate(predictedNextMove);
                
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