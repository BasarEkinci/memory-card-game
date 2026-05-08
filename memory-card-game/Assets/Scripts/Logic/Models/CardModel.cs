namespace CardMatch.Logic.Models
{
    public sealed class CardModel
    {
        public int GridIndex { get; set; }

        public int TypeId { get; set; }

        public CardState State { get; set; }
    }
}
