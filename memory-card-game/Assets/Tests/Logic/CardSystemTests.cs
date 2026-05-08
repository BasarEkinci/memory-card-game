using NUnit.Framework;
using CardMatch.Logic.Models;
using CardMatch.Logic.Systems;

namespace CardMatch.Logic.Tests
{
    [TestFixture]
    public sealed class CardSystemTests
    {
        private CardModel[] _cards;
        private CardSystem _sut;

        [SetUp]
        public void SetUp()
        {
            _cards = new CardModel[16];
            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                _cards[cardIndex] = new CardModel
                {
                    GridIndex = cardIndex,
                    TypeId = cardIndex / 2,
                    State = CardState.FaceDown
                };
            }
            _sut = new CardSystem(_cards);
        }

        [Test]
        public void FlipCard_FaceDown_BecomesFaceUp()
        {
            _cards[0].State = CardState.FaceDown;

            bool result = _sut.FlipCard(0);

            Assert.That(result, Is.True);
            Assert.That(_cards[0].State, Is.EqualTo(CardState.FaceUp));
        }

        [Test]
        public void FlipCard_AlreadyFaceUp_ReturnsFalse()
        {
            _cards[0].State = CardState.FaceUp;

            bool result = _sut.FlipCard(0);

            Assert.That(result, Is.False);
        }

        [Test]
        public void FlipCard_AlreadyMatched_ReturnsFalse()
        {
            _cards[0].State = CardState.Matched;

            bool result = _sut.FlipCard(0);

            Assert.That(result, Is.False);
        }

        [Test]
        public void CloseCard_FaceUp_BecomesFaceDown()
        {
            _cards[0].State = CardState.FaceUp;

            bool result = _sut.CloseCard(0);

            Assert.That(result, Is.True);
            Assert.That(_cards[0].State, Is.EqualTo(CardState.FaceDown));
        }

        [Test]
        public void CloseCard_NotFaceUp_ReturnsFalse()
        {
            _cards[0].State = CardState.FaceDown;

            bool result = _sut.CloseCard(0);

            Assert.That(result, Is.False);
        }

        [Test]
        public void MarkMatched_TwoCards_BothBecomeMatched()
        {
            _cards[0].State = CardState.FaceUp;
            _cards[1].State = CardState.FaceUp;

            _sut.MarkMatched(0, 1);

            Assert.That(_cards[0].State, Is.EqualTo(CardState.Matched));
            Assert.That(_cards[1].State, Is.EqualTo(CardState.Matched));
        }

        [Test]
        public void ResetAllCards_SetsAllToInDeck()
        {
            _cards[0].State = CardState.FaceUp;
            _cards[1].State = CardState.Matched;
            _cards[2].State = CardState.FaceDown;

            _sut.ResetAllCards();

            for (int cardIndex = 0; cardIndex < _cards.Length; cardIndex++)
            {
                Assert.That(_cards[cardIndex].State, Is.EqualTo(CardState.InDeck));
            }
        }

        [Test]
        public void AreAllMatched_AllMatched_ReturnsTrue()
        {
            for (int cardIndex = 0; cardIndex < _cards.Length; cardIndex++)
            {
                _cards[cardIndex].State = CardState.Matched;
            }

            bool result = _sut.AreAllMatched();

            Assert.That(result, Is.True);
        }

        [Test]
        public void AreAllMatched_SomeRemaining_ReturnsFalse()
        {
            for (int cardIndex = 0; cardIndex < _cards.Length; cardIndex++)
            {
                _cards[cardIndex].State = CardState.Matched;
            }
            _cards[0].State = CardState.FaceDown;

            bool result = _sut.AreAllMatched();

            Assert.That(result, Is.False);
        }

        [Test]
        public void GetFaceUpCards_ReturnsFaceUpIndices()
        {
            _cards[2].State = CardState.FaceUp;
            _cards[5].State = CardState.FaceUp;

            var faceUpIndices = _sut.GetFaceUpCards();

            Assert.That(faceUpIndices.Count, Is.EqualTo(2));
            Assert.That(faceUpIndices, Does.Contain(2));
            Assert.That(faceUpIndices, Does.Contain(5));
        }

        [Test]
        public void GetMatchedCount_ReturnsCorrectCount()
        {
            _cards[0].State = CardState.Matched;
            _cards[1].State = CardState.Matched;
            _cards[2].State = CardState.Matched;

            int count = _sut.GetMatchedCount();

            Assert.That(count, Is.EqualTo(3));
        }
    }
}
