using NUnit.Framework;
using CardMatch.Logic.Models;
using CardMatch.Logic.Systems;

namespace CardMatch.Logic.Tests
{
    [TestFixture]
    public sealed class GridSystemTests
    {
        private GridModel _gridModel;
        private CardModel[] _cards;
        private GridSystem _sut;

        [SetUp]
        public void SetUp()
        {
            _gridModel = new GridModel();
            _cards = new CardModel[16];
            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                _cards[cardIndex] = new CardModel { GridIndex = cardIndex };
            }
            _sut = new GridSystem(_gridModel, _cards);
        }

        [Test]
        public void Shuffle_Creates8Pairs()
        {
            _sut.Shuffle();

            int[] counts = new int[8];
            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                counts[_gridModel.CardTypeIds[cardIndex]]++;
            }

            for (int typeIndex = 0; typeIndex < 8; typeIndex++)
            {
                Assert.That(counts[typeIndex], Is.EqualTo(2), $"Type {typeIndex} should appear exactly twice");
            }
        }

        [Test]
        public void Shuffle_AllCardsAssigned()
        {
            _sut.Shuffle();

            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                int typeId = _gridModel.CardTypeIds[cardIndex];
                Assert.That(typeId, Is.GreaterThanOrEqualTo(0), $"CardTypeIds[{cardIndex}] should be >= 0");
                Assert.That(typeId, Is.LessThan(8), $"CardTypeIds[{cardIndex}] should be < 8");
            }
        }

        [Test]
        public void Shuffle_WithSeed_IsDeterministic()
        {
            _sut.Shuffle(seed: 12345);
            int[] firstShuffle = new int[16];
            System.Array.Copy(_gridModel.CardTypeIds, firstShuffle, 16);

            _gridModel.CardTypeIds = new int[16];
            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                _cards[cardIndex] = new CardModel { GridIndex = cardIndex };
            }
            _sut = new GridSystem(_gridModel, _cards);

            _sut.Shuffle(seed: 12345);
            int[] secondShuffle = _gridModel.CardTypeIds;

            Assert.That(secondShuffle, Is.EqualTo(firstShuffle), "Same seed should produce same layout");
        }

        [Test]
        public void Shuffle_DifferentSeeds_ProduceDifferentLayouts()
        {
            _sut.Shuffle(seed: 111);
            int[] shuffle1 = new int[16];
            System.Array.Copy(_gridModel.CardTypeIds, shuffle1, 16);

            _gridModel.CardTypeIds = new int[16];
            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                _cards[cardIndex] = new CardModel { GridIndex = cardIndex };
            }
            _sut = new GridSystem(_gridModel, _cards);

            _sut.Shuffle(seed: 222);
            int[] shuffle2 = _gridModel.CardTypeIds;

            bool layoutsAreDifferent = false;
            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                if (shuffle1[cardIndex] != shuffle2[cardIndex])
                {
                    layoutsAreDifferent = true;
                    break;
                }
            }

            Assert.That(layoutsAreDifferent, Is.True, "Different seeds should produce different layouts");
        }

        [Test]
        public void Shuffle_UpdatesCardModelTypes()
        {
            _sut.Shuffle(seed: 42);

            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                Assert.That(_cards[cardIndex].TypeId, Is.EqualTo(_gridModel.CardTypeIds[cardIndex]),
                    $"CardModel[{cardIndex}].TypeId should match GridModel.CardTypeIds[{cardIndex}]");
            }
        }

        [Test]
        public void Shuffle_SetsCardsToInDeckState()
        {
            _sut.Shuffle();

            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                Assert.That(_cards[cardIndex].State, Is.EqualTo(CardState.InDeck),
                    $"CardModel[{cardIndex}].State should be InDeck after shuffle");
            }
        }

        [Test]
        public void GetCardTypeAt_ReturnsCorrectType()
        {
            _sut.Shuffle(seed: 99);

            for (int gridIndex = 0; gridIndex < 16; gridIndex++)
            {
                int expected = _gridModel.CardTypeIds[gridIndex];
                int actual = _sut.GetCardTypeAt(gridIndex);
                Assert.That(actual, Is.EqualTo(expected), $"GetCardTypeAt({gridIndex}) should return correct type");
            }
        }
    }
}
