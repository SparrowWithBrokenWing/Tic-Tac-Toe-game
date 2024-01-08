using System.Collections.Immutable;
using TicTacToeGameEngine.Rule;
using TicTacToeGameEngine.Output;
using TicTacToeGameEngine.Output.Move;

namespace TicTacToeGameEngine.Participant.Arbiter;

public partial class EndgameNotifier : AbstractArbiter<IMove>
{
    // require the priority of rule when intialize. It will be use to avoid the case game go to a state that sastisfy more than one endgame rule which represent for different game result.
    public EndgameNotifier(IEnumerable<Tuple<uint, IEndgameRule<IMove>>> ruleAndPrioritySet) 
        : base(ruleAndPrioritySet.OrderBy(t => t.Item1).Select(t => t.Item2))
    { }

    // if a rule is sastisfied, that mean game is going to endgame state.
    // if there are more than 2 rule are sasitisfied at the sametime, throw an error cause, I don't know, I don't think I should do that so, let's ignore it. I hope I'll get better answer on next update.
    public override bool IsRuleSatisfied(IMove obj)
    {
        foreach (IEndgameRule<IMove> rule in Rules)
        {
            if (rule.IsSatisfied(obj))
            {
                // notify to all subcribers/observers.
                // hope that after foreach loop is finieshed, the newly created list from observers.ToImmutableList() don't keep any reference to elements of observers.
                foreach (var subcriber in observers.ToImmutableList())
                {
                    try
                    {
                        subcriber.OnNext(new MatchResult(obj.Player));
                        subcriber.OnCompleted();
                    }
                    catch (Exception e)
                    {
                        subcriber.OnError(e);
                    }
                }

                // destroy all subcription confirmation
                foreach (var confirmation in subcriptionConfimations)
                {
                    confirmation.Dispose();
                }

                return true;
            }
        }
        return false;
    }
}

// provide the ability to notify about match result to all subcriber which is needed when game end.
partial class EndgameNotifier : IObservable<MatchResult>
{
    private ICollection<IObserver<MatchResult>> observers = new List<IObserver<MatchResult>>();

    private ICollection<SubcriptionConfirmation> subcriptionConfimations = new List<SubcriptionConfirmation>();

    public virtual IDisposable Subscribe(IObserver<MatchResult> observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }

        var confirmation = new SubcriptionConfirmation(observers, observer);
        subcriptionConfimations.Add(confirmation);

        return confirmation;
    }

    protected sealed class SubcriptionConfirmation : IDisposable
    {
        private ICollection<IObserver<MatchResult>> observers;
        private IObserver<MatchResult> observer;

        public SubcriptionConfirmation(
            ICollection<IObserver<MatchResult>> observers,
            IObserver<MatchResult> observer)
        {
            observers = observers;
            observer = observer;
        }

        public void Dispose()
        {
            observers?.Remove(observer);
        }
    }
}
