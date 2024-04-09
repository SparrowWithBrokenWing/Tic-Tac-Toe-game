using ComputerPlayer.Analyzer;
using ComputerPlayer;
using Moq;

namespace Computer_Player_Test_Project.Analyzer_Component_Test
{
    [TestClass]
    public class Analyzer_Test
    {
        public static IEnumerable<object[]> TestData1
        {
            get
            {
                var mockOfPlayer = new Mock<IPlayer>();
                var mockOfOpponent = new Mock<IPlayer>();
                var players = new Tuple<IPlayer, IPlayer>(mockOfPlayer.Object, mockOfOpponent.Object);
                var player = players.Item1;
                var opponent = players.Item2;
                mockOfPlayer.Setup((player) => player.Equals(It.IsAny<IPlayer>())).Returns<IPlayer>((otherPlayer) => ReferenceEquals(otherPlayer, player));
                mockOfOpponent.Setup((player) => player.Equals(It.IsAny<IPlayer>())).Returns<IPlayer>((otherPlayer) => ReferenceEquals(otherPlayer, player));

                var mockOfBoard = new Mock<IBoard>();
                mockOfBoard.Setup((board) => board.NumberOfRows).Returns(3);
                mockOfBoard.Setup((board) => board.NumberOfColumns).Returns(3);
                mockOfBoard.Setup((board) => board.WinningCondition).Returns(3);
                var board = mockOfBoard.Object;

                var moveCategorizer = new CompositeCategorizer(new IMoveCategorizer[]
                {
                    new OffensiveMoveTypeCategorizer(),
                    new ForkMoveTypeCategorizer(),
                    new WinningMoveTypeCategorizer(),
                    new DefensiveMoveTypeCategorizer(),
                    new BlockForkMoveTypeCategorizer(),
                    new BlockWinningMoveTypeCategorizer()
                });

                var getNewMockMove = (int row, int column, IPlayer player) =>
                {
                    var mockOfMove = new Mock<IMove>();
                    mockOfMove.Setup((move) => move.Row).Returns(row);
                    mockOfMove.Setup((move) => move.Column).Returns(column);
                    mockOfMove.Setup((move) => move.Player).Returns(() => player);
                    return mockOfMove.Object;
                };
                var playedMoveList = new List<IMove>();
                var addNewPlayedMove = (IMove move) =>
                {
                    playedMoveList.Add(move);
                };
                addNewPlayedMove(getNewMockMove(0, 0, mockOfPlayer.Object));
                addNewPlayedMove(getNewMockMove(1, 0, mockOfOpponent.Object));
                addNewPlayedMove(getNewMockMove(0, 1, mockOfPlayer.Object));
                addNewPlayedMove(getNewMockMove(0, 2, mockOfOpponent.Object));
                var playedMoveRetriever = new PlayedMoveLog(board, playedMoveList);
                var lastPlayedMove = playedMoveList.Last();

                var basicMoveValueEvaluator = new MoveTypeEvaluator();
                basicMoveValueEvaluator.Register<IOffensiveMoveType>(1);
                basicMoveValueEvaluator.Register<IForkMoveType>((float)2.25);
                basicMoveValueEvaluator.Register<IWinningMoveType>((float)3.25);
                basicMoveValueEvaluator.Register<IDefensiveMoveType>(1);
                basicMoveValueEvaluator.Register<IBlockForkMoveType>(2);
                basicMoveValueEvaluator.Register<IBlockWinningMoveType>(3);


                var mockOfTreeBranchFactory = new Mock<IMoveFacotry<IAnalyzableTreeBranch>>();
                mockOfTreeBranchFactory
                    .Setup((factory) => factory.Produce(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IPlayer>()))
                    .Returns<int, int, IPlayer>((row, column, player) =>
                    {
                        var mockOfAnalyzableTreeBranch = new Mock<IAnalyzableTreeBranch>();
                        mockOfAnalyzableTreeBranch.Setup((move) => move.Row).Returns(row);
                        mockOfAnalyzableTreeBranch.Setup((move) => move.Column).Returns(column);
                        mockOfAnalyzableTreeBranch.Setup((move) => move.Player).Returns(player);

                        IMove? previousMove = null;
                        mockOfAnalyzableTreeBranch.As<IPreviousMoveSpecifiableMove>().SetupSet((move) => move.PreviousMove = It.IsAny<IMove>()).Callback<IMove>((move) => previousMove = move);
                        mockOfAnalyzableTreeBranch.As<IPreviousMoveAccessibleMove>().Setup((move) => move.PreviousMove).Returns(() => previousMove is null ? throw new NotImplementedException() : previousMove);

                        List<IMove> nextMoves = new List<IMove>();
                        mockOfAnalyzableTreeBranch.Setup((move) => move.PredictableNextMoves).Returns(() => nextMoves);
                        mockOfAnalyzableTreeBranch.Setup((move) => move.PredictedNextMoves).Returns(() => nextMoves);

                        uint componentHeight = 0;
                        mockOfAnalyzableTreeBranch.Setup((move) => move.ComponentHeight).Returns(() => componentHeight);
                        mockOfAnalyzableTreeBranch.SetupSet((move) => move.ComponentHeight = It.IsAny<uint>()).Callback<uint>((value) => componentHeight = value);

                        List<IMoveType> moveTypes = new List<IMoveType>();
                        mockOfAnalyzableTreeBranch.As<ICategorizableMove>().Setup((move) => move.Categories).Returns(() => moveTypes);
                        mockOfAnalyzableTreeBranch.As<ICategorizedMove>().Setup((move) => move.Categories).Returns(() => moveTypes);

                        return mockOfAnalyzableTreeBranch.Object;
                    });
                var mockOfTreeLeafFactory = new Mock<IMoveFacotry<IAnalyzableTreeLeaf>>();
                mockOfTreeLeafFactory
                    .Setup((factory) => factory.Produce(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IPlayer>()))
                    .Returns<int, int, IPlayer>((row, column, player) =>
                    {
                        var mockOfAnalyzableTreeLeaf = new Mock<IAnalyzableTreeLeaf>();
                        mockOfAnalyzableTreeLeaf.Setup((move) => move.Row).Returns(row);
                        mockOfAnalyzableTreeLeaf.Setup((move) => move.Column).Returns(column);
                        mockOfAnalyzableTreeLeaf.Setup((move) => move.Player).Returns(player);

                        IMove? previousMove = null;
                        mockOfAnalyzableTreeLeaf.As<IPreviousMoveAccessibleMove>().Setup((move) => move.PreviousMove).Returns(() => previousMove is null ? throw new NotImplementedException() : previousMove);
                        mockOfAnalyzableTreeLeaf.As<IPreviousMoveSpecifiableMove>().SetupSet((move) => move.PreviousMove = It.IsAny<IMove>()).Callback<IMove>((move) => previousMove = move);

                        uint componentHeight = 0;
                        mockOfAnalyzableTreeLeaf.Setup((move) => move.ComponentHeight).Returns(() => componentHeight);
                        mockOfAnalyzableTreeLeaf.SetupSet((move) => move.ComponentHeight = It.IsAny<uint>()).Callback<uint>((value) => componentHeight = value);

                        List<IMoveType> moveTypes = new List<IMoveType>();
                        mockOfAnalyzableTreeLeaf.As<ICategorizableMove>().Setup((move) => move.Categories).Returns(() => moveTypes);
                        mockOfAnalyzableTreeLeaf.As<ICategorizedMove>().Setup((move) => move.Categories).Returns(() => moveTypes);

                        return mockOfAnalyzableTreeLeaf.Object;
                    });
                var mockOfTreeFlowerFactory = new Mock<IMoveFacotry<IAnalyzableTreeFlower>>();
                mockOfTreeFlowerFactory
                    .Setup((factory) => factory.Produce(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IPlayer>()))
                    .Returns<int, int, IPlayer>((row, column, player) =>
                    {
                        var mockOfAnalyzableTreeFlower = new Mock<IAnalyzableTreeFlower>();
                        mockOfAnalyzableTreeFlower.Setup((move) => move.Row).Returns(row);
                        mockOfAnalyzableTreeFlower.Setup((move) => move.Column).Returns(column);
                        mockOfAnalyzableTreeFlower.Setup((move) => move.Player).Returns(player);

                        IMove? previousMove = null;
                        mockOfAnalyzableTreeFlower.As<IPreviousMoveAccessibleMove>().Setup((move) => move.PreviousMove).Returns(() => previousMove is null ? throw new NotImplementedException() : previousMove);
                        mockOfAnalyzableTreeFlower.As<IPreviousMoveSpecifiableMove>().SetupSet((move) => move.PreviousMove = It.IsAny<IMove>()).Callback<IMove>((move) => previousMove = move);

                        List<IMove> nextMoves = new List<IMove>();
                        mockOfAnalyzableTreeFlower.Setup((move) => move.PredictableNextMoves).Returns(() => nextMoves);
                        mockOfAnalyzableTreeFlower.Setup((move) => move.PredictedNextMoves).Returns(() => nextMoves);

                        uint componentHeight = 0;
                        mockOfAnalyzableTreeFlower.Setup((move) => move.ComponentHeight).Returns(() => componentHeight);
                        mockOfAnalyzableTreeFlower.SetupSet((move) => move.ComponentHeight = It.IsAny<uint>()).Callback<uint>((value) => componentHeight = value);

                        List<IMoveType> moveTypes = new List<IMoveType>();
                        mockOfAnalyzableTreeFlower.As<ICategorizableMove>().Setup((move) => move.Categories).Returns(() => moveTypes);
                        mockOfAnalyzableTreeFlower.As<ICategorizedMove>().Setup((move) => move.Categories).Returns(() => moveTypes);

                        return mockOfAnalyzableTreeFlower.Object;
                    });
                var mockOfTreeFruitFactory = new Mock<IMoveFacotry<IAnalyzableTreeFruit>>();
                mockOfTreeFruitFactory
                    .Setup((factory) => factory.Produce(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IPlayer>()))
                    .Returns<int, int, IPlayer>((row, column, player) =>
                    {
                        var mockOfAnalyzableTreeFruit = new Mock<IAnalyzableTreeFruit>();
                        mockOfAnalyzableTreeFruit.Setup((move) => move.Row).Returns(row);
                        mockOfAnalyzableTreeFruit.Setup((move) => move.Column).Returns(column);
                        mockOfAnalyzableTreeFruit.Setup((move) => move.Player).Returns(player);

                        IMove? previousMove = null;
                        mockOfAnalyzableTreeFruit.As<IPreviousMoveAccessibleMove>().Setup((move) => move.PreviousMove).Returns(() => previousMove is null ? throw new NotImplementedException() : previousMove);
                        mockOfAnalyzableTreeFruit.As<IPreviousMoveSpecifiableMove>().SetupSet((move) => move.PreviousMove = It.IsAny<IMove>()).Callback<IMove>((move) => previousMove = move);

                        uint componentHeight = 0;
                        mockOfAnalyzableTreeFruit.Setup((move) => move.ComponentHeight).Returns(() => componentHeight);
                        mockOfAnalyzableTreeFruit.SetupSet((move) => move.ComponentHeight = It.IsAny<uint>()).Callback<uint>((value) => componentHeight = value);

                        List<IMoveType> moveTypes = new List<IMoveType>();
                        mockOfAnalyzableTreeFruit.As<ICategorizableMove>().Setup((move) => move.Categories).Returns(() => moveTypes);
                        mockOfAnalyzableTreeFruit.As<ICategorizedMove>().Setup((move) => move.Categories).Returns(() => moveTypes);

                        return mockOfAnalyzableTreeFruit.Object;
                    });
                var treeBranchFactory = mockOfTreeBranchFactory.Object;
                var treeLeafFactory = mockOfTreeLeafFactory.Object;
                var treeFlowerFactory = mockOfTreeFlowerFactory.Object;
                var treeFruitFactory = mockOfTreeFruitFactory.Object;

                var tree = new TreeImplement1
                    <IAnalyzableTreeComponent,
                    IAnalyzableTreeBranch,
                    IAnalyzableTreeLeaf,
                    IAnalyzableTreeFlower,
                    IAnalyzableTreeFruit>
                    (players, board, lastPlayedMove, playedMoveRetriever, moveCategorizer, treeBranchFactory, treeLeafFactory, treeFlowerFactory, treeFruitFactory);

                return new object[][]{
                    new object[]
                    {
                        new AnalyzerImplement1
                            (player, players, board, tree, new BasicAdapterEvaluator<ICategorizedMove>(basicMoveValueEvaluator)),
                        new Tuple<int, int>(1, 1)
                    }
                };
            }
        }

        [TestMethod]
        public void Try()
        {

            var mockOfTreeBranchFactory = new Mock<IMoveFacotry<IAnalyzableTreeBranch>>();
            mockOfTreeBranchFactory
                .Setup((factory) => factory.Produce(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IPlayer>()))
                .Returns<int, int, IPlayer>((row, column, player) =>
                {
                    var mockOfAnalyzableTreeBranch = new Mock<IAnalyzableTreeBranch>();
                    mockOfAnalyzableTreeBranch.Setup((move) => move.Row).Returns(row);
                    mockOfAnalyzableTreeBranch.Setup((move) => move.Column).Returns(column);
                    mockOfAnalyzableTreeBranch.Setup((move) => move.Player).Returns(player);

                    IMove? previousMove = null;
                    mockOfAnalyzableTreeBranch.As<IPreviousMoveSpecifiableMove>().SetupSet((move) => move.PreviousMove = It.IsAny<IMove>()).Callback<IMove>((move) => previousMove = move);
                    mockOfAnalyzableTreeBranch.As<IPreviousMoveAccessibleMove>().Setup((move) => move.PreviousMove).Returns(() => previousMove is null ? throw new NotImplementedException() : previousMove);

                    List<IMove> nextMoves = new List<IMove>();
                    mockOfAnalyzableTreeBranch.Setup((move) => move.PredictableNextMoves).Returns(nextMoves);
                    mockOfAnalyzableTreeBranch.Setup((move) => move.PredictedNextMoves).Returns(nextMoves);

                    uint componentHeight = 0;
                    mockOfAnalyzableTreeBranch.Setup((move) => move.ComponentHeight).Returns(() => componentHeight);
                    mockOfAnalyzableTreeBranch.SetupSet((move) => move.ComponentHeight = It.IsAny<uint>()).Callback<uint>((value) => componentHeight = value);

                    List<IMoveType> moveTypes = new List<IMoveType>();
                    mockOfAnalyzableTreeBranch.As<ICategorizableMove>().Setup((move) => move.Categories).Returns(moveTypes);
                    mockOfAnalyzableTreeBranch.As<ICategorizedMove>().Setup((move) => move.Categories).Returns(moveTypes);

                    return mockOfAnalyzableTreeBranch.Object;
                });
            var treeBranchFactory = mockOfTreeBranchFactory.Object;

            var mockOfPlayer = new Mock<IPlayer>();
            var mockOfOpponent = new Mock<IPlayer>();
            var players = new Tuple<IPlayer, IPlayer>(mockOfPlayer.Object, mockOfOpponent.Object);
            var player = players.Item1;
            var opponent = players.Item2;
            mockOfPlayer.Setup((player) => player.Equals(It.IsAny<IPlayer>())).Returns<IPlayer>((otherPlayer) => ReferenceEquals(otherPlayer, player));
            mockOfOpponent.Setup((player) => player.Equals(It.IsAny<IPlayer>())).Returns<IPlayer>((otherPlayer) => ReferenceEquals(otherPlayer, player));

            var test = treeBranchFactory.Produce(1, 1, player);
            test.ComponentHeight = 5;
            Assert.AreEqual((uint)5, test.ComponentHeight);

        }

        [TestMethod]
        [DynamicData(nameof(TestData1))]
        public void VerifyAnalyzerWorkAsExpected(IAnalyzer analyzer, Tuple<int, int> expectedAnalyzeResult)
        {
            Assert.AreEqual(analyzer.Analyze(), expectedAnalyzeResult);
        }
    }
}
