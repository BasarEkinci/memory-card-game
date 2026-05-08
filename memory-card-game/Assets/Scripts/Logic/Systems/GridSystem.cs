using System;
using CardMatch.Logic.Models;

namespace CardMatch.Logic.Systems
{
    public sealed class GridSystem
    {
        private readonly GridModel _gridModel;
        private readonly CardModel[] _cards;

        public GridSystem(GridModel gridModel, CardModel[] cards)
        {
            _gridModel = gridModel;
            _cards = cards;
        }

        public void Shuffle(int? seed = null)
        {
            var random = seed.HasValue ? new Random(seed.Value) : new Random();

            int[] types = new int[16];
            for (int typeIndex = 0; typeIndex < 8; typeIndex++)
            {
                types[typeIndex * 2] = typeIndex;
                types[typeIndex * 2 + 1] = typeIndex;
            }

            for (int i = types.Length - 1; i > 0; i--)
            {
                int randomIndex = random.Next(i + 1);
                int temp = types[i];
                types[i] = types[randomIndex];
                types[randomIndex] = temp;
            }

            Array.Copy(types, _gridModel.CardTypeIds, types.Length);

            for (int cardIndex = 0; cardIndex < _cards.Length; cardIndex++)
            {
                _cards[cardIndex].TypeId = _gridModel.CardTypeIds[cardIndex];
                _cards[cardIndex].State = CardState.InDeck;
            }
        }

        public int GetCardTypeAt(int gridIndex)
        {
            return _gridModel.CardTypeIds[gridIndex];
        }
    }
}
