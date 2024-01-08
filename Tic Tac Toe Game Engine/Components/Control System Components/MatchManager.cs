using TicTacToeGameEngine.GameStateDescriptor;
using TicTacToeGameEngine.Participant.Arbiter;
using TicTacToeGameEngine.Participant.Player;
using System.Collections.Immutable;
using TicTacToeGameEngine.Output;
using TicTacToeGameEngine.Output.Move;

namespace TicTacToeGameEngine.Components.ControlSystemComponents
{
    public class MatchManager : IObserver<TDecision>
    {
        private ICollection<IObserver<TDecision>> _observers = new List<IObserver<TDecision>>();

        void IObserver<TDecision>.OnNext(TDecision value)
        {
            foreach (IObserver<TDecision> observer in _observers)
            {
                try
                {
                    observer.OnNext(value);
                }
                catch(Exception respond)
                {

                }
            }
        }
    }

    public interface IChannel
    {
        public void Send():
    }



    // the match manager is the one that can start the match.
    // the match manager is only needed arbiter to check player moves only, everything that isn't connect to player then need to check outside the match manager
    public partial class MatchManager
    {
        protected IBoard board;
        protected IEnumerable<IPlayer> players;
        protected IArbiter<IMove> arbiter;

        protected MatchManager(
            IBoard board,
            IEnumerable<IPlayer> players,
            IArbiter<IMove> arbiter)
        {
            board = board;
            players = players;
            arbiter = arbiter;
        }
    }

    partial class MatchManager : IObserver<MatchResult>
    {
        private IDisposable? matchResultSubcriptionConfirmation;
        private MatchResult? matchResult = null;

        public virtual void SubscribeTo(IObservable<MatchResult> matchResultNotifier)
        {
            matchResultSubcriptionConfirmation = matchResultNotifier.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            matchResultSubcriptionConfirmation?.Dispose();
        }

        // when the notifier feel like it should stop to notify, even it is when the notifier feel like it's completed the responsibility to notify or a error has happend force it to stop send notify, all subcriber confirmation should be disposed and the subcriber should, somehow, know that it wion't be notify in anyway anymore.
        public virtual void OnCompleted()
        {
            Unsubscribe();
        }

        public virtual void OnError(Exception error)
        {
            Unsubscribe();
        }

        // this method will be used by the notifier to sent notificaton which is a IMatchResult instance to all subscriber.
        // when notifier notify to subcriber via this method, that mean the game ended and everything will change.
        public virtual void OnNext(MatchResult value)
        {
            matchResult = value;
        }

        public MatchManager(
            IBoard board,
            IEnumerable<IPlayer> players,
            IArbiter<IMove> arbiter,
            IObservable<MatchResult> matchResultNotifier) : this(board, players, arbiter)
        {
            SubscribeTo(matchResultNotifier);
        }
    }

    // make the manager able to notify other player about their opponent move and tell the player if a move have error or not.
    // the player will need to check that the next move is their move or not. If they are the same move, that's mean player's move is accepted. I don't know is there any better way to do this or not.
    partial class MatchManager : IObservable<IMove>, IDisposable
    {
        protected ICollection<IObserver<IMove>> newTurnObservers = new List<IObserver<IMove>>();

        // this field exists for one purpose: to help remove observe ability of observer as easy as posible. The observable object will only notify to the one that have subcription confirmation only, and can only to notify to the one that have subcription confirmation only.
        protected ICollection<SubscriptionConfirmation> confirmations = new List<SubscriptionConfirmation>();

        public IDisposable Subscribe(IObserver<IMove> newObserver)
        {
            newTurnObservers.Add(newObserver);
            var newConfirmation = new SubscriptionConfirmation(newTurnObservers, newObserver);
            confirmations.Add(newConfirmation);
            return newConfirmation;
        }

        protected sealed class SubscriptionConfirmation : IDisposable
        {
            private ICollection<IObserver<IMove>> observers;
            private IObserver<IMove> observer;

            public SubscriptionConfirmation(
                ICollection<IObserver<IMove>> observers,
                IObserver<IMove> observer)
            {
                observers = observers;
                observer = observer;
            }

            public void Dispose()
            {
                observers.Remove(observer);
            }
        }

        public void Dispose()
        {
            foreach (var confirmation in confirmations.ToImmutableList())
            {
                confirmation.Dispose();
            }
        }
    }

    // define the main purpose of match manager: start a match
    partial class MatchManager
    {
        // the match is a process that require player make their move that sasitisfy rule and will be end if the arbiter notify to everyone that the game end by a player.        
        public virtual async void Start()
        {
            while (matchResult is null)
            {
                foreach (var currentPlayer in players)
                {
                    try
                    {
                        IMove nextMoveOfCurrentPlayer = await currentPlayer.GetNextMove();
                        // I hope I can find a way that somehow, when the manager notify to player that their move isn't accepted then it will be able to ask and get new player move at the same time too.
                        while (!arbiter.IsRuleSatisfied(nextMoveOfCurrentPlayer))
                        {
                            ((IObserver<IMove>)currentPlayer).OnError(new IllegalMoveException());
                            nextMoveOfCurrentPlayer = await currentPlayer.GetNextMove();
                        }
                        // cause new turn instance will be created when board apply new move so I make the board apply player move first then send notification of new played turn by current player to other player.
                        board.ApplyPlayerMove(nextMoveOfCurrentPlayer);

                        foreach (var observer in newTurnObservers)
                        {
                            // I need to find a name for the new turn played by current player.
                            observer.OnNext(board.Moves.Last());
                        }
                    }
                    catch(Exception exception)
                    {
                        ((IObserver<IMove>)currentPlayer).OnError(exception);
                    }
                }
            }

            foreach (var observer in newTurnObservers.ToImmutableList())
            {
                observer.OnCompleted();
            }

            Dispose();
        }
    }

    // I don't know where should I define this new exception for illegal move.
    partial class MatchManager
    {
        public class IllegalMoveException : Exception
        {

        }
    }
}
