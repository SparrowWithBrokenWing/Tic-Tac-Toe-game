using TicTacToeGameEngine.Output.Move;
using TicTacToeGameEngine.Rule;

namespace TicTacToeGameEngine.Participant.Arbiter;

public abstract partial class AbstractArbiter<T> : IArbiter<T> where T : IMove
{
    private IList<IMovement<T>> rules;

    protected IList<IMovement<T>> Rules => rules;

    protected AbstractArbiter(IEnumerable<IMovement<T>> rules)
    {
        rules = rules.ToList<IMovement<T>>();
    }

    // a move need to sasitsify all rules to be considered as a legal move.
    public virtual bool IsRuleSatisfied(T obj)
    {
        foreach (var rule in rules)
        {
            if (!rule.IsSatisfied(obj))
            {
                return false;
            }
        }

        return true;
    }
}

