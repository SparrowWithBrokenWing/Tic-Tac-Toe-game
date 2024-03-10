namespace ComputerPlayer.Analyzer
{
    public interface IAnalyzer
    {
        public Tuple<int, int> Analyze();
    }

    public abstract class AbstractAnalyzer : IAnalyzer, IComparer<IMove>
    {

        protected IPlayer Player { get; private set; }
        protected ITree PredictedNextMovesTree { get; private set; }
        protected IMoveRetriever PlayedMoveRetriever { get; private set; }
        protected IComparer<INextMovesPredictedMove> MoveQualityComparer { get; private set; }
        protected IComparer<IPreviousMoveAccessibleMove> MovePossibilityComparer { get; private set; }

        public abstract int Compare(IMove? x, IMove? y);

        // result of suggestion base on the current state of the tree
        public Tuple<int, int> Analyze()
        {
            // need to find the move with the position that bring back the best chance to win. should it be a chain? follow a branch to predict the best move?
            // move that is the best will be the move that is have the most value in current and in the chain that have best value. but how long the chain? what is the end point of the chain? but what if the next move in chain cannot be played anymore? so the chance to be able to played the next move in chain count too. Also, the result of chain is the most value thing. The best move should be the move that have best value when predict. Base on the player, I can setup a parameter to change how should anzlyer suggest the best move. The best chain move is the chain that have the last move that have the biggest number valuable next moves (quantity) and have the most valuable move (quality) and have the least block chanin move (posibility)(note: move in chain still possible and still keep value after opponent move)

            // have to wait for opponent move to suggest next coached player move
            if (PlayedMoveRetriever.Last().Player.Equals(Player))
            {
                throw new NotImplementedException();
            }

            // what is the exchange rate between value of move and the posibility of its? (note: move that is in the future, which have to be after at least one opponent move)
            // the value of move (the predicted move in tree after opponent move) will be base its type, and its next predicted moves after it is played. the number of chain, the height of those chain will affect this aspect. 
            Func<INextMovesPredictedMove, Tuple<int, int>>? analyze = null;
            analyze = (INextMovesPredictedMove nextMovePredictedMove) =>
            {
                var predictedNextMoves = nextMovePredictedMove.PredictedNextMoves;
                IMove bestMove = predictedNextMoves.First();
                foreach (var predictedMove in predictedNextMoves)
                {
                    var compareResult = Compare(predictedMove, bestMove);
                    if (compareResult > 0)
                    {
                        bestMove = predictedMove;
                    }
                }
                return new Tuple<int, int>(bestMove.Row, bestMove.Column);
            };

            return analyze is not null ? analyze(PredictedNextMovesTree.Root) : throw new NullReferenceException();
        }

        private class PossibilityComparer : IComparer<IPredictedPreviousMoveAccessibleMove>
        {
            public int Compare(IPredictedPreviousMoveAccessibleMove? x, IPredictedPreviousMoveAccessibleMove? y)
            {
                // which is the move have less chance to be blocked and have more routes to reach => search in tree for both moves, and compare them? or look back on their predicted previous move? and the endpoint should be the last played move?

                throw new NotImplementedException();
            }
        }

        private class QualityComparer : IComparer<IMove>
        {
            public int Compare(IMove? x, IMove? y)
            {
                // base on type of move and then how good of the chai3rfn, on quality and quantity, which this move belong? the chain 1 move will have better value than chain n?
                throw new NotImplementedException();
            }
        }
    }

    public class Analyzer1 : AbstractAnalyzer
    {
        protected IDictionary<Type, uint> MoveTypeValues { get; private set; }

        public void Register<TCategorizedMove>(uint value)
            where TCategorizedMove : ICategorizedMove
        {
            MoveTypeValues.Add(typeof(TCategorizedMove), value);
        }

        // compare number of routes that can lead to each move
        protected int ComparePossiblity(IPreviousMoveAccessibleMove? firstMove, IPreviousMoveAccessibleMove? secondMove)
        {

        }

        // compare type
        protected int CompareQuality(ICategorizedMove? firstMove, ICategorizedMove? secondMove)
        {

        }

        // should I remember the best analyze result cause the analyze process will take a lot of time?
        // compare how good the situation can become after a chain of move which this move belong and how possible that chain is too.
        protected int CompareChain(INextMovesPredictedMove? firstMove, INextMovesPredictedMove? secondMove)
        {

        }

        public override int Compare(IMove? firstMove, IMove? secondMove)
        {
            if (firstMove is null 
            ||  secondMove is null)
            {
                throw new ArgumentNullException();
            }


            return;
        }
    }
}
