using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.Blackjack.Modals
{
    public class Hand(bool isDealer = false)
    {
        private readonly List<Card> cards = new List<Card>(5);

        public event EventHandler Changed;

        public bool IsDealer { get; private set; } = isDealer;

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

                var aces = this.cards.Count(c => c.Rank == Rank.Ace);

                while (aces-- > 0 && faceValue > 21)
                {
                    faceValue -= 9;
                }

                return faceValue;
            }
        }

        public bool IsBlackjack
        {
            get { throw new NotImplementedException(); }
        }

        public void AddCard(Card card)
        {
            this.cards.Add(card);

            if (this.Changed != null)
            {
                this.Changed(this, EventArgs.Empty);
            }
        }

        public void Show()
        {
            this.cards.ForEach(
                card =>
                {
                    if (!card.IsFaceUp)
                    {
                        card.Flip();

                        if (this.Changed != null)
                        {
                            this.Changed(this, EventArgs.Empty);
                        }
                    }
                });
        }

        public void Clear()
        {
            this.cards.Clear();
        }
    }
}