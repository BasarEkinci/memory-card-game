using CardMatch.Logic.Models;
using UnityEngine;

namespace CardMatch.Runtime.Services
{
    public sealed class SaveSystem
    {
        private const string GameStateKey = "CardMatch_GameState";
        private const string BestScoreKey = "CardMatch_BestScore";
        private const string MusicVolumeKey = "CardMatch_MusicVolume";
        private const string SfxVolumeKey = "CardMatch_SfxVolume";

        public void SaveGameState(GameStateModel gameState, CardModel[] cards)
        {
            var data = new SaveData
            {
                Score = gameState.Score,
                StrikeCount = gameState.StrikeCount,
                FailCount = gameState.FailCount,
                MaxStrike = gameState.MaxStrike,
                Phase = (int)gameState.Phase,
                CardStates = new int[cards.Length],
                CardTypeIds = new int[cards.Length]
            };

            for (int cardIndex = 0; cardIndex < cards.Length; cardIndex++)
            {
                data.CardStates[cardIndex] = (int)cards[cardIndex].State;
                data.CardTypeIds[cardIndex] = cards[cardIndex].TypeId;
            }

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(GameStateKey, json);
            PlayerPrefs.Save();
        }

        public bool TryLoadGameState(out SaveData data)
        {
            data = null;
            string json = PlayerPrefs.GetString(GameStateKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            data = JsonUtility.FromJson<SaveData>(json);
            return data != null;
        }

        public void ClearGameState()
        {
            PlayerPrefs.DeleteKey(GameStateKey);
            PlayerPrefs.Save();
        }

        public void SaveBestScore(int score)
        {
            int current = PlayerPrefs.GetInt(BestScoreKey, 0);
            if (score > current)
            {
                PlayerPrefs.SetInt(BestScoreKey, score);
                PlayerPrefs.Save();
            }
        }

        public int LoadBestScore() => PlayerPrefs.GetInt(BestScoreKey, 0);

        public void SaveSettings(float musicVolume, float sfxVolume)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
            PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
            PlayerPrefs.Save();
        }

        public (float musicVolume, float sfxVolume) LoadSettings()
        {
            return (
                PlayerPrefs.GetFloat(MusicVolumeKey, 1f),
                PlayerPrefs.GetFloat(SfxVolumeKey, 1f)
            );
        }

        public bool HasSavedGame() => PlayerPrefs.HasKey(GameStateKey);
    }
}
