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

    public interface IMoveEvaluator<TMove>
        where TMove : IMove
    {
        public float Evaluate(TMove move);
    }

    public class MoveTypeEvaluator : IMoveEvaluator<ICategorizedMove> 
    {
        public MoveTypeEvaluator()
        {
            MoveTypeValueDictionary = new Dictionary<Type, float>();
        }

        protected IDictionary<Type, float> MoveTypeValueDictionary { get; private set; }

        public void Register<TMoveType>(float value)
            where TMoveType : IMoveType
        {
            MoveTypeValueDictionary.Add(typeof(TMoveType), value);
        }

        public float Evaluate(ICategorizedMove categorizedMove)
        {
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

    public class PredictedNextMovesValueEvaluator : IMoveEvaluator<INextMovesPredictedMove> 
    {
        public void Setup(IMoveEvaluator<IMove> realMoveEvaluator)
        {
            MoveEvaluator = realMoveEvaluator;
        }

        protected IMoveEvaluator<IMove>? MoveEvaluator { get; private set; }

        public float Evaluate(INextMovesPredictedMove nextMovesPredictedMove)
        {
            if (MoveEvaluator is null)
            {
                throw new NotImplementedException();
            }
            var numberOfPredictedNextMoves = nextMovesPredictedMove.PredictedNextMoves.Count();
            float result = 0;
            foreach (var predictedNextMove in nextMovesPredictedMove.PredictedNextMoves)
            {
                if (predictedNextMove is INextMovesPredictedMove sub)
                {
                    result += Evaluate(sub);
                }
                else
                {
                    result += MoveEvaluator.Evaluate(predictedNextMove) * 1 / numberOfPredictedNextMoves;
                }
            }
            result /= numberOfPredictedNextMoves;
            return result;
        }
    }

    public class CompositeEvaluator<TMove> : IMoveEvaluator<TMove>
        where TMove : IMove
    {
        public CompositeEvaluator(IEnumerable<IMoveEvaluator<TMove>> components)
        {
            MoveEvaluators = new LinkedList<IMoveEvaluator<TMove>>(components);
        }

        protected LinkedList<IMoveEvaluator<TMove>> MoveEvaluators { get; private set; }

        public void Add(IMoveEvaluator<TMove> evaluator)
        {
            MoveEvaluators.AddLast(evaluator);
        }

        public float Evaluate(TMove move)
        {
            float result = 0;
            foreach (var evaluator in MoveEvaluators)
            {
                result += evaluator.Evaluate(move);
            }
            return result;
        }
    }

    public class BasicAdapterEvaluator<TMove> : IMoveEvaluator<IMove>
        where TMove : IMove
    {
        public BasicAdapterEvaluator(IMoveEvaluator<TMove> adaptedMoveEvaluator)
        {
            AdapterMoveEvaluator = adaptedMoveEvaluator;
        }

        protected IMoveEvaluator<TMove> AdapterMoveEvaluator { get; private set; }

        public float Evaluate(IMove move)
        {
            if (move is not TMove)
            {
                return 0;
            }
            return AdapterMoveEvaluator.Evaluate((TMove)move);
        }
    }

    public class RateTransformerEvaluator<TMove> : IMoveEvaluator<TMove> 
        where TMove : IMove
    {
        public RateTransformerEvaluator(float rate, IMoveEvaluator<TMove> moveEvaluator)
        {
            Rate = rate;
            MoveEvauator = moveEvaluator;
        }

        protected IMoveEvaluator<TMove> MoveEvauator { get; private set; }
        protected float Rate { get; private set; }

        public float Evaluate(TMove move)
        {
            return Rate * MoveEvauator.Evaluate(move);
        }
    }

    public class ConditionAdapterEvaluator<TMove> : IMoveEvaluator<TMove> 
        where TMove : IMove
    {
        public ConditionAdapterEvaluator(Func<TMove, bool> evaluateCondition, IMoveEvaluator<TMove> moveEvaluator)
        {
            EvaluateCondition = evaluateCondition;
            MoveEvaluator = moveEvaluator;
        }

        protected Func<TMove, bool> EvaluateCondition { get; private set; }
        protected IMoveEvaluator<TMove> MoveEvaluator { get; private set; }

        public float Evaluate(TMove move)
        {
            return EvaluateCondition(move) ? MoveEvaluator.Evaluate(move) : 0;
        }
    }

    public class ActBaseOnEvaluatingResultEvaluator<TMove> : IMoveEvaluator<TMove> 
        where TMove : IMove
    {
        public ActBaseOnEvaluatingResultEvaluator(IMoveEvaluator<TMove> moveEvaluator, Func<float, bool> actCondition, Action act)
        {
            MoveEvaluator = moveEvaluator;
            ActCondition = actCondition;
            Act = act;
        }

        protected IMoveEvaluator<TMove> MoveEvaluator { get; private set; }
        protected Func<float, bool> ActCondition { get; private set; }
        protected Action Act { get; private set; }

        public float Evaluate(TMove move)
        {
            var result = MoveEvaluator.Evaluate(move);
            if (ActCondition(result))
            {
                Act();
            }
            return result;
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
