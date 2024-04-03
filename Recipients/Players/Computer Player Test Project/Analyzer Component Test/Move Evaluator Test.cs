//using ComputerPlayer;
//using ComputerPlayer.Analyzer;
//using Moq;
//using System.Diagnostics;

//namespace Computer_Player_Test_Project.Analyzer_Component_Test
//{
//    [TestClass]
//    public class Move_Evaluator_Test
//    {
//        public static IEnumerable<object[]> TestData1
//        {
//            get
//            {
//                var mockOfPlayer = new Mock<IPlayer>();
//                var mockOfOpponent = new Mock<IPlayer>();
//                var getNewMockMove = (int row, int column, IPlayer player) =>
//                {
//                    var mockOfMove = new Mock<IMove>();
//                    mockOfMove.Setup((move) => move.Row).Returns(row);
//                    mockOfMove.Setup((move) => move.Column).Returns(column);
//                    mockOfMove.Setup((move) => move.Player).Returns(player);
//                    return mockOfMove.Object;
//                };
//                var mockOfBoard = new Mock<IBoard>();
//                mockOfBoard.Setup((board) => board.NumberOfRows).Returns(3);
//                mockOfBoard.Setup((board) => board.NumberOfColumns).Returns(3);
//                mockOfBoard.Setup((board) => board.WinningCondition).Returns(3);
//                var getNewCategorizedMockMove = (int row, int column, IPlayer player, IMoveCategorizer moveCategorizer, IMoveRetriever moveRetriever, IBoard board) =>
//                {
//                    var mockOfCategorizedMove = new Mock<ICategorizedMove>();
//                    mockOfCategorizedMove.Setup((move) => move.Row).Returns(row);
//                    mockOfCategorizedMove.Setup((move) => move.Column).Returns(column);
//                    mockOfCategorizedMove.Setup((move) => move.Player).Returns(player);
//                    mockOfCategorizedMove.Setup((move) => move.Categories).Returns(moveCategorizer.Categorize(getNewMockMove(row, column, player), moveRetriever, board));
//                    return mockOfCategorizedMove.Object;
//                };
//                var moveCategorizer = new CompositeCategorizer(new IMoveCategorizer[]
//                {
//                    new OffensiveMoveTypeCategorizer(),
//                    new ForkMoveTypeCategorizer(),
//                    new WinningMoveTypeCategorizer(),
//                    new DefensiveMoveTypeCategorizer(),
//                    new BlockForkMoveTypeCategorizer(),
//                    new BlockWinningMoveTypeCategorizer()
//                });


//                var mockOfMoveRetriever = new Mock<IMoveRetriever>();
//                var playedMoveList = new Dictionary<(int, int), IMove>
//                {
//                    [(1, 1)] = getNewMockMove(1, 1, mockOfPlayer.Object),
//                    [(2, 1)] = getNewMockMove(2, 1, mockOfOpponent.Object)
//                    //[(0, 1)] = getNewMockMove(1, 1, mockOfPlayer.Object),
//                    //[(2, 1)] = getNewMockMove(1, 1, mockOfOpponent.Object)
//                };
//                //mockOfMoveRetriever
//                //    .Setup((moveRetriever) => moveRetriever.Retrieve(It.IsAny<int>(), It.IsAny<int>()))
//                //    .Returns<int, int>((row, column) => playedMoveList[(row, column)]);
//                //mockOfMoveRetriever
//                //    .Setup((moveRetriever) => moveRetriever.Last())
//                //    .Returns(playedMoveList.Last().Value);
//                mockOfMoveRetriever
//                    .Setup((moveRetriever) => moveRetriever.GetEnumerator())
//                    .Returns(playedMoveList.Values.GetEnumerator());
//                var playedMoveRetriever = mockOfMoveRetriever.Object;


//                var lastPlayedMove = playedMoveList.Last().Value;
//                var basicMoveValueEvaluator = new MoveTypeEvaluator();
//                basicMoveValueEvaluator.Register<IOffensiveMoveType>(1);
//                basicMoveValueEvaluator.Register<IForkMoveType>((float)2.25);
//                basicMoveValueEvaluator.Register<IWinningMoveType>((float)3.25);
//                basicMoveValueEvaluator.Register<IDefensiveMoveType>(1);
//                basicMoveValueEvaluator.Register<IBlockForkMoveType>(2);
//                basicMoveValueEvaluator.Register<IBlockWinningMoveType>(3);
//                var predictedNextMoveValueEvaluator = new PredictedNextMovesValueEvaluator();
//                var temp1 = new CompositeEvaluator<IMove>(new IMoveEvaluator<IMove>[]
//                {
//                    new BasicAdapterEvaluator<INextMovesPredictedMove>(new RateTransformerEvaluator<INextMovesPredictedMove>((float).35, predictedNextMoveValueEvaluator)),
//                    new BasicAdapterEvaluator<ICategorizedMove>(new RateTransformerEvaluator<ICategorizedMove>((float).65, basicMoveValueEvaluator))
//                });
//                var temp = new CompositeEvaluator<IMove>(new IMoveEvaluator<IMove>[]
//                {
//                    new CompositeEvaluator<IMove>(new IMoveEvaluator<IMove>[]
//                    {
//                        new ConditionAdapterEvaluator<IMove>((move)=>move.Player.Equals(lastPlayedMove.Player), new RateTransformerEvaluator<IMove>((float)1, temp1)),
//                        new ConditionAdapterEvaluator<IMove>((move)=>!move.Player.Equals(lastPlayedMove.Player), new RateTransformerEvaluator<IMove>((float)-1, temp1))
//                    })
//                });
//                predictedNextMoveValueEvaluator.Setup(temp);
//                return new object[][]{
//                    new object[]
//                    {
//                        new BasicAdapterEvaluator<INextMovesPredictedMove>(predictedNextMoveValueEvaluator),
//                        getNewCategorizedMockMove(2, 0, mockOfPlayer.Object, moveCategorizer, playedMoveRetriever, mockOfBoard.Object),
//                        (float)5.75
//                    }
//                };
//            }
//        }

//        //[TestMethod]
//        //public void Test()
//        //{
//        //    var t = TestData1;
//        //}

//        [TestMethod]
//        [DynamicData(nameof(TestData1))]
//        public void VerfiyEvaluatorWorkAsExpected(IMoveEvaluator<IMove> testedEvaluator, IMove evaluatedMove, float expectedEvaluateResult)
//        {
//            var evaluateResult = testedEvaluator.Evaluate(evaluatedMove);
//            //Debug.WriteLine("\nEvaluate result: {0}\nExpected Result: {1}\n", evaluateResult, expectedEvaluateResult);
//            Assert.AreEqual(expectedEvaluateResult, evaluateResult);
//        }
//    }
//}
