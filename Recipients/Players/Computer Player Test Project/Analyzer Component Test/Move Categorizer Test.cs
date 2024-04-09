using ComputerPlayer;
using ComputerPlayer.Analyzer;
using Moq;
namespace Computer_Player_Test_Project.Analyzer_Component_Test
{
    [TestClass]
    public class Move_Categorizer_Test
    {
        [TestMethod]
        public void VerifyCategorizerWorkAsExpected()
        {
            var testCategorizer = new ForkMoveTypeCategorizer();

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

            var getNewMockMove = (int row, int column, IPlayer player) =>
            {
                var mockOfMove = new Mock<IMove>();
                mockOfMove.Setup((move) => move.Row).Returns(row);
                mockOfMove.Setup((move) => move.Column).Returns(column);
                mockOfMove.Setup((move) => move.Player).Returns(()=>player);
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

            var mockOfMoveFactory = new Mock<IMoveFacotry<IMove>>();
            mockOfMoveFactory
                .Setup((factory) => factory.Produce(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IPlayer>()))
                .Returns<int, int, IPlayer>((row, column, player) =>
                {
                    var mockOfMove = new Mock<IAnalyzableTreeBranch>();
                    mockOfMove.Setup((move) => move.Row).Returns(row);
                    mockOfMove.Setup((move) => move.Column).Returns(column);
                    mockOfMove.Setup((move) => move.Player).Returns(player);

                    return mockOfMove.Object;
                });
            var moveFactory = mockOfMoveFactory.Object;
            var categories = testCategorizer.Categorize(moveFactory.Produce(1, 1, player), playedMoveRetriever, board);
            Assert.IsTrue(categories.OfType<IForkMoveType>().Any());
        }
    }
}
