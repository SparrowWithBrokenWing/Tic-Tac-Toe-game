using ComputerPlayer;
using Moq;

namespace Computer_Player_Test_Project.Analyzer_Component_Test
{
    [TestClass]
    public class Played_Move_Log_Test
    {
        [TestMethod]
        public void VerfiyMoveLogWorkAsExpected()
        {
            var mockOfPlayer = new Mock<IPlayer>();
            var mockOfOpponent = new Mock<IPlayer>();
            var players = new Tuple<IPlayer, IPlayer>(mockOfPlayer.Object, mockOfOpponent.Object);
            var player = mockOfPlayer.Object;
            var opponent = mockOfOpponent.Object;

            var mockOfMoveRetriever = new Mock<IMoveRetriever>();
            var getNewMockMove = (int row, int column, IPlayer player) =>
            {
                var mockOfMove = new Mock<IMove>();
                mockOfMove.Setup((move) => move.Row).Returns(row);
                mockOfMove.Setup((move) => move.Column).Returns(column);
                mockOfMove.Setup((move) => move.Player).Returns(player);
                return mockOfMove.Object;
            };
            var playedMoveList = new Dictionary<(int, int), IMove>
            {
                [(1, 1)] = getNewMockMove(1, 1, mockOfPlayer.Object),
                [(2, 1)] = getNewMockMove(2, 1, mockOfOpponent.Object)
            };
            mockOfMoveRetriever
                .Setup((moveRetreiver) => moveRetreiver.Retrieve(It.IsAny<int>(), It.IsAny<int>()))
                .Returns<int, int>((row, column) => playedMoveList[new(row, column)]);
            mockOfMoveRetriever
                .Setup((moveRetriever) => moveRetriever.GetEnumerator())
                .Returns(playedMoveList.Values.GetEnumerator());
            var playedMoveRetriever = mockOfMoveRetriever.Object;

            var mockOfBoard = new Mock<IBoard>();
            mockOfBoard.Setup((board) => board.NumberOfRows).Returns(3);
            mockOfBoard.Setup((board) => board.NumberOfColumns).Returns(3);
            mockOfBoard.Setup((board) => board.WinningCondition).Returns(3);
            var board = mockOfBoard.Object;

            var test = new PlayedMoveLog(board, playedMoveRetriever);
            Assert.AreEqual(test.Retrieve(1, 1), playedMoveList[new(1, 1)]);
            var count = 0;
            foreach (var obj in test)
            {
                count++;
            }
            Assert.AreEqual(count, 2);
        }
    }
}
