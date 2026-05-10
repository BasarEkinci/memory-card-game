using System.Collections.Generic;
using CardMatch.Logic.Models;

namespace CardMatch.Logic.Systems
{
    public sealed class CardSystem
    {
        private readonly CardModel[] _cards;
        private readonly List<int> _faceUpIndices;

        public CardSystem(CardModel[] cards)
        {
            _cards = cards;
            _faceUpIndices = new List<int>(cards.Length);
        }

        public bool FlipCard(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= _cards.Length)
            {
                return false;
            }

            if (_cards[cardIndex].State != CardState.FaceDown)
            {
                return false;
            }

            _cards[cardIndex].State = CardState.FaceUp;
            return true;
        }

        public bool CloseCard(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= _cards.Length)
            {
                return false;
            }

            if (_cards[cardIndex].State != CardState.FaceUp)
            {
                return false;
            }

            _cards[cardIndex].State = CardState.FaceDown;
            return true;
        }

        public void MarkMatched(int cardIndex1, int cardIndex2)
        {
            if (cardIndex1 >= 0 && cardIndex1 < _cards.Length)
            {
                _cards[cardIndex1].State = CardState.Matched;
            }

            if (cardIndex2 >= 0 && cardIndex2 < _cards.Length)
            {
                _cards[cardIndex2].State = CardState.Matched;
            }
        }

        public void ResetAllCards()
        {
            for (int cardIndex = 0; cardIndex < _cards.Length; cardIndex++)
            {
                _cards[cardIndex].State = CardState.InDeck;
            }
        }

        public void DealAllCards()
        {
            for (int cardIndex = 0; cardIndex < _cards.Length; cardIndex++)
            {
                if (_cards[cardIndex].State == CardState.InDeck)
                {
                    _cards[cardIndex].State = CardState.FaceDown;
                }
            }
        }

        public CardModel GetCard(int index)
        {
            if (index < 0 || index >= _cards.Length)
            {
                return null;
            }

            return _cards[index];
        }

        public List<int> GetFaceUpCards()
        {
            _faceUpIndices.Clear();

            for (int cardIndex = 0; cardIndex < _cards.Length; cardIndex++)
            {
                if (_cards[cardIndex].State == CardState.FaceUp)
                {
                    _faceUpIndices.Add(cardIndex);
                }
            }

            return _faceUpIndices;
        }

        public int GetMatchedCount()
        {
            int count = 0;

            for (int cardIndex = 0; cardIndex < _cards.Length; cardIndex++)
            {
                if (_cards[cardIndex].State == CardState.Matched)
                {
                    count++;
                }
            }

            return count;
        }

        public bool AreAllMatched()
        {
            for (int cardIndex = 0; cardIndex < _cards.Length; cardIndex++)
            {
                if (_cards[cardIndex].State != CardState.Matched)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
