namespace ComputerPlayer
{
    public interface IMoveEvaluator<TRate>
    {
        public TRate Evaluate(IMove move);
    }

    //public class MoveEvaluator : IMoveEvaluator<float>
    //{
    //    public MoveEvaluator()
    //    {

    //    }

    //    private float Calculate(IMove move)
    //    {
    //        if (move is INextMovesPredictedMove)
    //        {

    //        }
    //        else
    //        {
    //            return;
    //        }
    //    }

    //    public float Evaluate(IMove move)
    //    {
    //        return Calculate(move);
    //    }
    //}

}
