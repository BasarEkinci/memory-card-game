using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using CardMatch.Logic.Models;
using CardMatch.Runtime.Services;

namespace CardMatch.Runtime.Tests
{
    [TestFixture]
    public sealed class SaveSystemTests
    {
        private const string BestScoreKey = "CardMatch_BestScore";

        private SaveSystem _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new SaveSystem();
            _sut.ClearGameState();
            PlayerPrefs.DeleteKey(BestScoreKey);
            PlayerPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            _sut.ClearGameState();
            PlayerPrefs.DeleteKey(BestScoreKey);
            PlayerPrefs.Save();
        }

        // ─────────────────────────────────────────────────
        // Save / Load Round-Trip
        // ─────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator SaveAndLoad_GameState_RoundTrips()
        {
            // Arrange
            var gameState = new GameStateModel
            {
                Score = 42,
                StrikeCount = 3,
                FailCount = 1,
                MaxStrike = 5,
                Phase = GamePhase.Playing
            };
            var cards = CreateTestCards();

            // Act
            _sut.SaveGameState(gameState, cards);
            yield return null; // Let PlayerPrefs flush to disk

            bool loaded = _sut.TryLoadGameState(out SaveData loadedData);

            // Assert
            Assert.That(loaded, Is.True);
            Assert.That(loadedData.Score, Is.EqualTo(42));
            Assert.That(loadedData.StrikeCount, Is.EqualTo(3));
            Assert.That(loadedData.FailCount, Is.EqualTo(1));
            Assert.That(loadedData.MaxStrike, Is.EqualTo(5));
            Assert.That(loadedData.Phase, Is.EqualTo((int)GamePhase.Playing));
        }

        [UnityTest]
        public IEnumerator SaveAndLoad_CardStates_RoundTrips()
        {
            // Arrange
            var gameState = new GameStateModel();
            var cards = CreateTestCards();
            cards[0].State = CardState.Matched;
            cards[2].State = CardState.FaceDown;

            // Act
            _sut.SaveGameState(gameState, cards);
            yield return null;

            bool loaded = _sut.TryLoadGameState(out SaveData loadedData);

            // Assert
            Assert.That(loaded, Is.True);
            Assert.That(loadedData.CardStates[0], Is.EqualTo((int)CardState.Matched));
            Assert.That(loadedData.CardStates[2], Is.EqualTo((int)CardState.FaceDown));
        }

        [UnityTest]
        public IEnumerator SaveAndLoad_CardTypeIds_RoundTrips()
        {
            // Arrange
            var gameState = new GameStateModel();
            var cards = CreateTestCards();

            // Act
            _sut.SaveGameState(gameState, cards);
            yield return null;

            bool loaded = _sut.TryLoadGameState(out SaveData loadedData);

            // Assert
            Assert.That(loaded, Is.True);
            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                Assert.That(loadedData.CardTypeIds[cardIndex], Is.EqualTo(cards[cardIndex].TypeId));
            }
        }

        // ─────────────────────────────────────────────────
        // No Saved State
        // ─────────────────────────────────────────────────

        [Test]
        public void Load_NoSavedState_ReturnsFalse()
        {
            bool loaded = _sut.TryLoadGameState(out SaveData data);

            Assert.That(loaded, Is.False);
            Assert.That(data, Is.Null);
        }

        [Test]
        public void HasSavedGame_NoSavedState_ReturnsFalse()
        {
            bool hasSave = _sut.HasSavedGame();

            Assert.That(hasSave, Is.False);
        }

        [Test]
        public void HasSavedGame_AfterSave_ReturnsTrue()
        {
            var gameState = new GameStateModel();
            var cards = CreateTestCards();

            _sut.SaveGameState(gameState, cards);

            Assert.That(_sut.HasSavedGame(), Is.True);
        }

        [Test]
        public void HasSavedGame_AfterClear_ReturnsFalse()
        {
            var gameState = new GameStateModel();
            var cards = CreateTestCards();
            _sut.SaveGameState(gameState, cards);

            _sut.ClearGameState();

            Assert.That(_sut.HasSavedGame(), Is.False);
        }

        // ─────────────────────────────────────────────────
        // Best Score
        // ─────────────────────────────────────────────────

        [Test]
        public void BestScore_OnlyUpdatesIfHigher()
        {
            _sut.SaveBestScore(100);
            _sut.SaveBestScore(50); // Lower — should not overwrite

            int best = _sut.LoadBestScore();

            Assert.That(best, Is.EqualTo(100));
        }

        [Test]
        public void BestScore_UpdatesWhenNewScoreIsHigher()
        {
            _sut.SaveBestScore(50);
            _sut.SaveBestScore(150); // Higher — should overwrite

            int best = _sut.LoadBestScore();

            Assert.That(best, Is.EqualTo(150));
        }

        [Test]
        public void BestScore_NoPriorRecord_DefaultsToZero()
        {
            int best = _sut.LoadBestScore();

            Assert.That(best, Is.EqualTo(0));
        }

        [Test]
        public void BestScore_EqualScore_DoesNotChange()
        {
            _sut.SaveBestScore(75);
            _sut.SaveBestScore(75); // Equal — should remain the same

            int best = _sut.LoadBestScore();

            Assert.That(best, Is.EqualTo(75));
        }

        // ─────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────

        private static CardModel[] CreateTestCards()
        {
            var cards = new CardModel[16];
            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                cards[cardIndex] = new CardModel
                {
                    GridIndex = cardIndex,
                    TypeId = cardIndex / 2,
                    State = CardState.FaceDown
                };
            }
            return cards;
        }
    }
}
