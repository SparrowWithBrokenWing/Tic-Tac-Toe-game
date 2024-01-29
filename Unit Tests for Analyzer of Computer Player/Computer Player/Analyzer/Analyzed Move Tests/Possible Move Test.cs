using ComputerPlayer;
using Moq;
using Moq.Protected;

namespace Unit_Tests_for_Analyzer_of_Computer_Player.Computer_Player.Analyzer.Analyzed_Move_Tests
{
    [TestClass]
    public class Possible_Move_Test
    {
        [TestMethod]
        public void VerifyInitializationSuccessfulWithUnplayedMove()
        {
            var mockOfBegginingBoardStae = new Mock<IMoveTracker>();

            var mockOfPlayer1 = new Mock<IPlayer>();
            var mockOfPlayer2 = new Mock<IPlayer>();

            uint numberOfRows = 3;
            uint numberOfColumns = 3;
            uint winningCondition = 3;
            var mockOfBoard = new Mock<IBoard>();
            mockOfBoard
                .SetupGet(board => board.NumberOfRows)
                .Returns(numberOfRows);
            mockOfBoard
                .SetupGet(board => board.NumberOfColumns)
                .Returns(numberOfColumns);
            mockOfBoard
                .SetupGet(board => board.WinningCondition)
                .Returns(winningCondition);

            var mockOfPlayedMoves = new IMove[]
            {
                new Move(1, 2, mockOfPlayer1.Object, new MoveTracker(mockOfBoard.Object, new IMove[0]))
            };

            var mockOfBoardState = new Mock<PlayedMoveTracker>(mockOfBoard.Object, mockOfPlayedMoves);
            mockOfBoardState.CallBase = true;

            var tryToCreatePossibleMove1 = () =>
            {
                var possibleMove = new PossbileMove(new Move(3, 3, mockOfPlayer1.Object, mockOfBoardState.Object));
            };

            tryToCreatePossibleMove1.Invoke();
        }

        [TestMethod]
        public void VerifyInitializationFailWithPlayedMove()
        {

            var fakePlayedMoves = new List<IList<IMove?>>(
                Enumerable.Repeat<IList<IMove?>>(
                    new List<IMove?>(Enumerable.Repeat<IMove?>(null, 4).ToList()),
                    4)
                .ToList());

            var mockOfBoardState = new Mock<PlayedMoveTracker>((uint)4, (uint)4, (uint)4);
            mockOfBoardState.CallBase = true;
            mockOfBoardState.Protected()
                .Setup<IList<IList<IMove?>>>("_PlayedMoves")
                .Returns(fakePlayedMoves);

            var mockOfPlayer1 = new Mock<IPlayer>();
            var mockOfPlayer2 = new Mock<IPlayer>();

            fakePlayedMoves[1][2] = new Move(1, 2, mockOfPlayer1.Object, mockOfBoardState.Object);

            var tryToCreatePossibleMove2 = () =>
            {
                var possibleMove = new PossbileMove(new Move(1, 2, mockOfPlayer2.Object, mockOfBoardState.Object));
            };

            Assert.ThrowsException<ArgumentException>(tryToCreatePossibleMove2);
        }
    }
}
