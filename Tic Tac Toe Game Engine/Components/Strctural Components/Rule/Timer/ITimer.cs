namespace TicTacToeGameEngine.GameStateDescriptor.Timer
{
    public interface ITimer
    {
        public TimeSpan TimeLeft { get; }
        public void Start();
        public void Stop();
        public void Reset();
    }
    
}
