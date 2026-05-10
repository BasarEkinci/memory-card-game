using System;
using CardMatch.Logic.Models;

namespace CardMatch.Logic.Systems
{
    public sealed class GridSystem
    {
        private readonly GridModel _gridModel;
        private readonly CardModel[] _cards;
        private readonly int _availableTypeCount;
        private readonly int _pairsNeeded;

        public GridSystem(GridModel gridModel, CardModel[] cards, int availableTypeCount)
        {
            _gridModel = gridModel;
            _cards = cards;
            _availableTypeCount = availableTypeCount;
            _pairsNeeded = cards.Length / 2;
        }

        public void Shuffle(int? seed = null)
        {
            var random = seed.HasValue ? new Random(seed.Value) : new Random();

            int[] selectedTypes = SelectRandomTypes(random);

            int[] types = new int[_pairsNeeded * 2];
            for (int pairIndex = 0; pairIndex < _pairsNeeded; pairIndex++)
            {
                types[pairIndex * 2] = selectedTypes[pairIndex];
                types[pairIndex * 2 + 1] = selectedTypes[pairIndex];
            }

            for (int i = types.Length - 1; i > 0; i--)
            {
                int randomIndex = random.Next(i + 1);
                (types[i], types[randomIndex]) = (types[randomIndex], types[i]);
            }

            Array.Copy(types, _gridModel.CardTypeIds, types.Length);

            for (int cardIndex = 0; cardIndex < _cards.Length; cardIndex++)
            {
                _cards[cardIndex].TypeId = _gridModel.CardTypeIds[cardIndex];
                _cards[cardIndex].State = CardState.InDeck;
            }
        }

        private int[] SelectRandomTypes(Random random)
        {
            int[] pool = new int[_availableTypeCount];
            for (int i = 0; i < _availableTypeCount; i++)
            {
                pool[i] = i;
            }

            for (int i = 0; i < _pairsNeeded; i++)
            {
                int randomIndex = random.Next(i, _availableTypeCount);
                (pool[i], pool[randomIndex]) = (pool[randomIndex], pool[i]);
            }

            int[] selected = new int[_pairsNeeded];
            Array.Copy(pool, selected, _pairsNeeded);
            return selected;
        }

        public int GetCardTypeAt(int gridIndex)
        {
            return _gridModel.CardTypeIds[gridIndex];
        }
    }
}
