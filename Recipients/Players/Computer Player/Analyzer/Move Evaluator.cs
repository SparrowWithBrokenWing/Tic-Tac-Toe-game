using System.Linq;

namespace ComputerPlayer.Analyzer
{
    public interface IEvaluableMove : IMove
    {
        public float EvaluateValue { set; }
    }

    public interface IEvaluatedMove : IMove
    {
        public float EvaluateValue { get; }
    }

    public interface IMoveEvaluator
    {
        public float Evaluate(IMove move);
    }

    public class MoveTypeEvaluator : IMoveEvaluator
    {
        public MoveTypeEvaluator()
        {
            MoveTypeValueDictionary = new Dictionary<Type, float>();
        }

        protected IDictionary<Type, float> MoveTypeValueDictionary { get; private set; }

        public void Register<TMoveType>(float value)
            where TMoveType : IMoveType
        {
            MoveTypeValueDictionary.Add(typeof(IMoveType), value);
        }

        public float Evaluate(IMove move)
        {
            if (move is not ICategorizedMove categorizedMove)
            {
                throw new ArgumentException();
            }

            float totalValue = default(float);
            HashSet<Type> usedTypes = new HashSet<Type>();

            foreach (var category in categorizedMove.Categories)
            {
                var categoryType = category.GetType();
                foreach (var registeredType in MoveTypeValueDictionary.Keys)
                {
                    if (categoryType.IsAssignableFrom(registeredType)
                    && !usedTypes.Contains(registeredType))
                    {
                        totalValue += MoveTypeValueDictionary[registeredType];
                        usedTypes.Add(registeredType);
                    }
                }
            }

            return totalValue;
        }
    }

    public class PredictedNextMoveValueEvaluator : IMoveEvaluator
    {
        private bool _hasBeenSetup = false;
        protected IMoveEvaluator? MoveEvaluator { get; private set; }

        public void Setup(IMoveEvaluator moveEvaluator)
        {
            if (_hasBeenSetup)
            {
                throw new NotImplementedException();
            }
            _hasBeenSetup = true;
            MoveEvaluator = moveEvaluator;
        }

        public float Evaluate(IMove move)
        {
            if (MoveEvaluator is null)
            {
                throw new NotImplementedException();
            }
            if (move is not INextMovesPredictedMove)
            {
                throw new ArgumentException();
            }
            var nextMovesPredictedMove = (INextMovesPredictedMove)move;
            var numberOfPredictedNextMoves = nextMovesPredictedMove.PredictedNextMoves.Count();
            float result = 0;
            foreach (var predictedNextMove in nextMovesPredictedMove.PredictedNextMoves)
            {
                result += MoveEvaluator.Evaluate(predictedNextMove) * 1 / numberOfPredictedNextMoves;
            }
            result /= numberOfPredictedNextMoves;
            return result;
        }
    }

    public class ExchangeRateEvaluator : IMoveEvaluator
    {
        public ExchangeRateEvaluator(IEnumerable<Tuple<float, IMoveEvaluator>> exchangeRates)
        {
            ExchangeRates = exchangeRates;
        }

        protected IEnumerable<Tuple<float, IMoveEvaluator>> ExchangeRates { get; private set; }

        public float Evaluate(IMove move)
        {
            float sum = 0;
            foreach (var evaluator in ExchangeRates)
            {
                sum += evaluator.Item1 * evaluator.Item2.Evaluate(move);
            }
            return sum;
        }
    }

    public class ConditionEvaluator : IMoveEvaluator
    {
        // need better name for property and constructor parameter
        public ConditionEvaluator(IEnumerable<Tuple<Func<IMove, bool>, IMoveEvaluator>> tuples)
        {
            Tuples = tuples;
        }

        protected IEnumerable<Tuple<Func<IMove, bool>, IMoveEvaluator>> Tuples { get; private set; }

        public float Evaluate(IMove move)
        {
            float sum = 0;
            foreach (var tuple in Tuples)
            {
                var condition = tuple.Item1;
                if (condition(move))
                {
                    var evaluator = tuple.Item2;
                    sum += evaluator.Evaluate(move);
                }
            }
            return sum;
        }
    }

    // this how should evaluator be set up
    //public class Demo
    //{
    //    public void Test(IMoveRetriever playedMoveRetriever)
    //    {
    //        var lastPlayedMove = playedMoveRetriever.Last();
    //        var basicMoveValueEvaluator = new MoveTypeEvaluator();

    //        var predictedNextMoveValueEvaluator = new PredictedNextMoveValueEvaluator();
    //        var exchangeRate = new ExchangeRateEvaluator(new Tuple<float, IMoveEvaluator>[]
    //        {
    //            new Tuple<float, IMoveEvaluator>((float).65, basicMoveValueEvaluator),
    //            new Tuple<float, IMoveEvaluator>((float).35, predictedNextMoveValueEvaluator)
    //        });
    //        var conditionEvaluator = new ConditionEvaluator(
    //            new Tuple<Func<IMove, bool>, IMoveEvaluator>[] {
    //                new Tuple<Func<IMove, bool>, IMoveEvaluator>(
    //                    (IMove move) => move.Player.Equals(lastPlayedMove), 
    //                    new ExchangeRateEvaluator(new Tuple<float, IMoveEvaluator>[]
    //                    {
    //                        new Tuple<float, IMoveEvaluator>((float)1.0, exchangeRate)
    //                    })
    //                ),
    //                new Tuple<Func<IMove, bool>, IMoveEvaluator>(
    //                    (IMove move) => !move.Player.Equals(lastPlayedMove),
    //                    new ExchangeRateEvaluator(new Tuple<float, IMoveEvaluator>[]
    //                    {
    //                        new Tuple<float, IMoveEvaluator>((float)-1.0, exchangeRate)
    //                    })
    //                )

    //        });
    //        predictedNextMoveValueEvaluator.Setup(conditionEvaluator);
    //    }
    //}
}
