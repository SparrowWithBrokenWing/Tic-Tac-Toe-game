using MessageTransformation.CanonicalDataModel.Decision;
using System.Runtime.CompilerServices;

namespace ComputerPlayer
{

    public interface IPlayer : IEquatable<IPlayer>
    {

    }

    public interface IBoard
    {
        public uint WinningCondition { get; }
        public uint NumberOfRows { get; }
        public uint NumberOfColumns { get; }
    }

    public class AnalyzedMoveTree
    {

    }

    public class TreeCaretaker
    {

    }

    //public class Analyzer
    //{
    //    public Task Analyze() { return; }
    //    public void UpdateBoard(IDecision opponentDecision)
    //    {
    //        Task analyzeTask = this.Analyze();
    //    }
    //    public IDecision SuggestNextDecision() { return; }
    //}

    public interface IMoveType { }
    public interface IDistributor { }

}
