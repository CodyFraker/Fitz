using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.Blackjack.Modals
{
    public class Hand
    {
        private readonly List<Card> cards;

        public Hand(bool isDealer = false)
        {
            cards = new List<Card>(5);
            IsDealer = isDealer;
        }

        public event EventHandler Changed;

        public bool IsDealer { get; private set; }

        public ReadOnlyCollection<Card> Cards
        {
            get { return this.cards.AsReadOnly(); }
        }

        public int SoftValue
        {
            get { return this.cards.Select(c => (int)c.Rank > 1 && (int)c.Rank < 11 ? (int)c.Rank : 10).Sum(); }
        }

        public int TotalValue
        {
            get
            {
                var totalValue = this.SoftValue;
                var aces = this.cards.Count(c => c.Rank == Rank.Ace);

                while (aces-- > 0 && totalValue > 21)
                {
                    totalValue -= 9;
                }

                return totalValue;
            }
        }

        public int FaceValue
        {
            get
            {
                var faceValue = this.cards.Where(c => c.IsFaceUp)
                    .Select(c => (int)c.Rank > 1 && (int)c.Rank < 11 ? (int)c.Rank : 10).Sum();

                var aces = this.cards.Count(c => c.Rank == Rank.Ace && c.IsFaceUp);

                while (aces-- > 0 && faceValue > 21)
                {
                    faceValue -= 9;
                }

                return faceValue;
            }
        }

        public bool IsBlackjack
        {
            get
            {
                if (this.cards.Count != 2)
                {
                    return false;
                }

                var hasAce = this.cards.Any(c => c.Rank == Rank.Ace);
                var hasTenCard = this.cards.Any(c => (int)c.Rank >= 10);

                return hasAce && hasTenCard;
            }
        }

        public void AddCard(Card card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            this.cards.Add(card);
            OnChanged();
        }

        public void Show()
        {
            foreach (var card in cards.Where(c => !c.IsFaceUp))
            {
                card.Flip();
            }
            OnChanged();
        }

        public void Clear()
        {
            cards.Clear();
            OnChanged();
        }

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}