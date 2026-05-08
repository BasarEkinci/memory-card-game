using UnityEngine;

namespace CardMatch.Runtime.ScriptableObjects
{
    [CreateAssetMenu(menuName = "CardMatch/CardDefinitions", fileName = "CardDefinitions")]
    public sealed class CardDefinitions : ScriptableObject
    {
        [SerializeField] private Sprite[] _faceSprites;
        [SerializeField] private Sprite _backSprite;

        public Sprite GetFaceSprite(int typeId) => _faceSprites[typeId];
        public Sprite BackSprite => _backSprite;
        public int FaceCount => _faceSprites?.Length ?? 0;
    }
}
