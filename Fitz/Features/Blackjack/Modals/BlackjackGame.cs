using Fitz.Features.Accounts.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fitz.Features.Blackjack.Modals
{
    public class BlackjackGame
    {
        public List<Player> Players { get; set; }
        public Dealer Dealer { get; private set; }

        public event EventHandler LastStateChanged;

        public event EventHandler AllowedActionsChanged;

        public BlackjackGame(Account fitzAccount, List<Player> players)
        {
            this.Dealer = new Dealer() { Fitz = fitzAccount };
            this.Players = players;
            this.LastState = GameState.Unknown;
            this.AllowedActions = GameAction.None;
        }

        private GameAction allowedActions;
        private GameState lastState;
        private Deck deck;

        public GameAction AllowedActions
        {
            get
            {
                return this.allowedActions;
            }

            private set
            {
                if (this.allowedActions != value)
                {
                    this.allowedActions = value;

                    if (this.AllowedActionsChanged != null)
                    {
                        this.AllowedActionsChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public GameState LastState
        {
            get
            {
                return this.lastState;
            }

            private set
            {
                if (this.lastState != value)
                {
                    this.lastState = value;

                    if (this.LastStateChanged != null)
                    {
                        this.LastStateChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public void Play(decimal balance, decimal bet)
        {
            //this.Player.Balance = balance;
            //this.Player.Bet = bet;
            this.AllowedActions = GameAction.Deal;

            if (this.AllowedActionsChanged != null)
            {
                this.AllowedActionsChanged(this, EventArgs.Empty);
            }
        }

        public void Deal()
        {
            if ((this.AllowedActions & GameAction.Deal) != GameAction.Deal)
            {
                throw new InvalidOperationException();
            }

            this.LastState = GameState.Unknown;

            if (this.deck == null)
            {
                this.deck = new Deck();
            }
            else
            {
                this.deck.Populate();
            }

            this.deck.Shuffle();
            this.Dealer.Hand.Clear();
            //this.Player.Hand.Clear();

            this.deck.Deal(this.Dealer.Hand);
            //this.deck.Deal(this.Player.Hand);

            foreach (Player player in this.Players)
            {
                player.Hand.Clear();
                this.deck.Deal(player.Hand);
                if (player.Hand.SoftValue == 21)
                {
                    if (this.Dealer.Hand.SoftValue == 21)
                    {
                        this.LastState = GameState.Draw;
                    }
                    else
                    {
                        player.Balance += player.Bet / 2;
                        this.LastState = GameState.PlayerWon;
                    }

                    this.Dealer.Hand.Show();
                    this.AllowedActions = GameAction.Deal;
                }
                else if (this.Dealer.Hand.TotalValue == 21)
                {
                    player.Balance -= player.Bet / 2;
                    this.Dealer.Hand.Show();
                    this.LastState = GameState.DealerWon;
                    this.AllowedActions = GameAction.Deal;
                }
                else
                {
                    this.AllowedActions = GameAction.Hit | GameAction.Stand;
                }
            }
        }

        public void Hit(Player player)
        {
            if ((this.AllowedActions & GameAction.Hit) != GameAction.Hit)
            {
                throw new InvalidOperationException();
            }

            this.deck.GiveAdditionalCard(player.Hand);

            if (player.Hand.TotalValue > 21)
            {
                player.Lost = true;
                player.Turn = false;
                this.NextPlayerTurn();
                if (CheckIfAnyPlayersHaveTurn() == false)
                {
                    this.FinishGame();
                }
            }
        }

        public void Stand(Player player)
        {
            if ((this.AllowedActions & GameAction.Stand) != GameAction.Stand)
            {
                throw new InvalidOperationException();
            }
            this.NextPlayerTurn();
            Players.Where(x => x.Account.Id == player.Account.Id).FirstOrDefault().Turn = false;
            Players.Where(x => x.Account.Id == player.Account.Id).FirstOrDefault().HasGone = true;
            // If no players have a turn left
            if (CheckIfAnyPlayersHaveTurn() == false)
            {
                this.FinishGame();
            }
        }

        private void FinishGame()
        {
            // If the dealer's hand is less than 17, draw a card
            while (this.Dealer.Hand.SoftValue < 17)
            {
                this.deck.GiveAdditionalCard(this.Dealer.Hand);
            }

            // If the dealer busts
            if (this.Dealer.Hand.TotalValue > 21)
            {
                // Determine if there is one or more winners
                List<Player> winners = Players.Where(x => x.Hand.TotalValue > Dealer.Hand.TotalValue && x.Lost == false).ToList();
                if (winners.Count == 1)
                {
                }
                if (winners.Count >= 2)
                {
                }
                this.LastState = GameState.PlayerWon;
            }
            else if (this.Dealer.Hand.TotalValue == 21)
            {
                this.LastState = GameState.DealerWon;
            }
            else if (Players.Any(x => x.Hand.TotalValue == this.Dealer.Hand.TotalValue && x.Lost == false))
            {
                this.LastState = GameState.Draw;
            }
            else if (Players.Any(x => x.Hand.TotalValue > this.Dealer.Hand.TotalValue && x.Lost == false))
            {
                this.LastState = GameState.PlayerWon;
            }
            else
            {
                this.LastState = GameState.DealerWon;
            }

            this.Dealer.Hand.Show();
            this.AllowedActions = GameAction.Deal;
        }

        // Try to find the next player that has a turn if they have lost, skip them.
        public void NextPlayerTurn()
        {
            foreach (Player player in Players)
            {
                if (player.HasGone == false && player.Lost == false)
                {
                    Players.Where(x => x.Account.Id == player.Account.Id).FirstOrDefault().Turn = true;
                }
            }
        }

        public bool CheckIfAnyPlayersHaveTurn()
        {
            foreach (Player player in this.Players)
            {
                if (player.HasGone == false && player.Lost == false)
                {
                    return true;
                }
                if (player.Turn)
                {
                    return true;
                }
            }
            return false;
        }
    }
}