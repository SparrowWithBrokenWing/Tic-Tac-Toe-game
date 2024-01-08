namespace TicTacToeGameEngine.GameStateDescriptor.Timer
{
    // define the constructor
    public abstract partial class AbstractTimer : ITimer
    {
        protected TimeSpan playableTime;
        protected readonly TimeSpan maximumPlayableTime;

        // not advised
        protected AbstractTimer(TimeSpan maximumPlayableTime)
        {
            maximumPlayableTime = maximumPlayableTime;
            playableTime = maximumPlayableTime;
        }

        // I don't expect negative number here.
        protected AbstractTimer(
            int numberOfPlayableDay,
            int numberOfPlayableHour,
            int numberOfPlayableMinute,
            int numberOfPlayableSecond)
            : this(new TimeSpan(
                (int)numberOfPlayableDay,
                (int)numberOfPlayableHour,
                (int)numberOfPlayableMinute,
                (int)numberOfPlayableSecond))
        {

        }

        protected Action? runWhenTimerStarts;
        protected Action? runWhenTimerStops;
        protected Action? runWhenTimerRunsOutOfTime;

        // not advised
        protected AbstractTimer(
            TimeSpan maximumPlayableTime,
            Action executeWhenTimerStarts,
            Action executeWhenTimerStops,
            Action executeWhenTimerRunsOutOfTime) : this(maximumPlayableTime)
        {
            runWhenTimerStarts = executeWhenTimerStarts;
            runWhenTimerStops = executeWhenTimerStops;
            runWhenTimerRunsOutOfTime = executeWhenTimerRunsOutOfTime;
        }

        protected AbstractTimer(
            int numberOfPlayableDay,
            int numberOfPlayableHour,
            int numberOfPlayableMinute,
            int numberOfPlayableSecond,
            Action executeWhenTimerStarts,
            Action executeWhenTimerStops,
            Action executeWhenTimerRunsOutOfTime)
            : this(
                  new TimeSpan(
                    numberOfPlayableDay,
                    numberOfPlayableHour,
                    numberOfPlayableMinute,
                    numberOfPlayableSecond),
                  executeWhenTimerStarts,
                  executeWhenTimerStops,
                  executeWhenTimerRunsOutOfTime)
        {

        }
    }

    // the implementaiton of ITimer
    public partial class AbstractTimer
    {
        public TimeSpan TimeLeft { get => playableTime; }

        private System.Timers.Timer? oneSecTimer;
        private System.Timers.ElapsedEventHandler? reducePlayableTimeByOneSecondAction;

        public virtual void Start()
        {
            if (oneSecTimer is not null)
            {
                oneSecTimer.Dispose();
            }

            oneSecTimer = new System.Timers.Timer();
            oneSecTimer.Interval = 1000;

            if (reducePlayableTimeByOneSecondAction is null)
            {
                reducePlayableTimeByOneSecondAction = (obj, e) =>
                {
                    // timer need to check if there is playable time left or not, if not stop the timer and invoke the action, or else continue to reduce the playable time.
                    if (playableTime.TotalSeconds > 0)
                    {
                        playableTime = playableTime.Subtract(new TimeSpan(0, 0, 1));
                    }
                    else
                    {
                        Stop();
                        runWhenTimerRunsOutOfTime?.Invoke();
                    }                    
                };
            }

            // reduce playable time to 1 second everytime onesectimer runs out of time.
            oneSecTimer.Elapsed += reducePlayableTimeByOneSecondAction;

            oneSecTimer.AutoReset = true;
            oneSecTimer.Enabled = true;
            runWhenTimerStarts?.Invoke();
        }

        public virtual void Stop()
        {
            if (oneSecTimer is not null)
            {
                oneSecTimer.Elapsed -= reducePlayableTimeByOneSecondAction;
                oneSecTimer.AutoReset = false;
                oneSecTimer.Enabled = false;
                oneSecTimer.Dispose();
                runWhenTimerStops?.Invoke();
            }
            else
            {
                throw new NotImplementedException("Something wrong...");
            }
        }

        public virtual void Reset()
        {
            playableTime = maximumPlayableTime;
        }
    }
}
