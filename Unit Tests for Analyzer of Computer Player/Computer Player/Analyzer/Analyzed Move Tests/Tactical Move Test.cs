
namespace Unit_Tests_for_Analyzer_of_Computer_Player.Computer_Player.Analyzer.Analyzed_Move_Tests
{
    [TestClass]
    public class Tactical_Move_Test
    {
        [TestMethod]
        public void VerifyInitializationSuccessfulWhenThereIsOtherMoveInSquareWithWinningConditionLength()
        {
            var numberOfRows = 3;
            var numberOfColumns = 3;
            var winningCondition = 3;
            var fakePlayedMoves = new List<IList<IMove?>>(
                Enumerable.Repeat<IList<IMove?>>(
                    new List<IMove?>(Enumerable.Repeat<IMove?>(null, numberOfRows).ToList()),
                    numberOfColumns)
                .ToList());

            var mockOfBoardState = new Mock<BoardState>((uint)numberOfRows, (uint)numberOfColumns, (uint)winningCondition);
            mockOfBoardState.CallBase = true;
            mockOfBoardState.Protected()
                .Setup<IList<IList<IMove?>>>("_PlayedMoves")
                .Returns(fakePlayedMoves);

            var mockOfPlayer1 = new Mock<IPlayer>();
            var mockOfPlayer2 = new Mock<IPlayer>();

            fakePlayedMoves[0][1] = new Move(0, 1, mockOfPlayer1.Object, mockOfBoardState.Object);
            fakePlayedMoves[2][0] = new Move(2, 0, mockOfPlayer2.Object, mockOfBoardState.Object);
            fakePlayedMoves[0][0] = new Move(0, 0, mockOfPlayer1.Object, mockOfBoardState.Object);

            var try1 = () =>
            {
                var possibleMove = new TacticalMove(new Move(1, 1, mockOfPlayer2.Object, mockOfBoardState.Object));
            };

            try1.Invoke();
        }

        [TestMethod]
        public void VerifyInitializationThrowExceptionWhenThereIsNoOtherPlayedMoveInSquareWithWinningConditionAsLength()
        {
            var numberOfRows = 5;
            var numberOfColumns = 5;
            var winningCondition = 3;
            var fakePlayedMoves = new List<IList<IMove?>>(
                Enumerable.Repeat<IList<IMove?>>(
                    new List<IMove?>(Enumerable.Repeat<IMove?>(null, numberOfRows).ToList()),
                    numberOfColumns)
                .ToList());

            var mockOfBoardState = new Mock<BoardState>((uint)numberOfRows, (uint)numberOfColumns, (uint)winningCondition);
            mockOfBoardState.CallBase = true;
            mockOfBoardState.Protected()
                .Setup<IList<IList<IMove?>>>("_PlayedMoves")
                .Returns(fakePlayedMoves);

            var mockOfPlayer1 = new Mock<IPlayer>();
            var mockOfPlayer2 = new Mock<IPlayer>();

            fakePlayedMoves[1][1] = new Move(1, 1, mockOfPlayer1.Object, mockOfBoardState.Object);

            var try2 = () =>
            {
                var possibleMove = new TacticalMove(new Move(3, 3, mockOfPlayer2.Object, mockOfBoardState.Object));
            };

            Assert.ThrowsException<ArgumentException>(try2);
        }
    }
}
