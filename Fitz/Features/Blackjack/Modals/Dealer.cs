using Fitz.Features.Accounts.Models;

namespace Fitz.Features.Blackjack.Modals
{
    public class Dealer : PlayerBase
    {
        public Account Fitz { get; set; }

        public Dealer()
        {
            this.Hand = new Hand(isDealer: true);
        }
    }
}