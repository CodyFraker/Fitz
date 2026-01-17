using System.Diagnostics.Metrics;

namespace Fitz.Metrics
{
    public class FitzMetrics
    {
        private readonly Meter _meter;
        private readonly Counter<long> _beerTransferredTotal;
        private readonly Counter<long> _beerTransferredAmount;
        private readonly Counter<long> _beerAwardedTotal;
        private readonly Counter<long> _beerAwardedAmount;
        private readonly Counter<long> _beerDeductedTotal;
        private readonly Counter<long> _beerDeductedAmount;
        private readonly ObservableGauge<long> _totalBeerBalance;
        private readonly Counter<long> _transactionsTotal;
        private readonly Counter<long> _lotteryTicketsPurchasedTotal;
        private readonly Counter<long> _lotteryTicketsPurchasedAmount;
        private readonly Counter<long> _lotteryDrawingsTotal;
        private readonly Counter<long> _lotteryWinnersTotal;
        private readonly ObservableGauge<long> _lotteryPoolSize;
        private readonly ObservableGauge<long> _lotterySubscriptionsActive;
        private readonly Histogram<double> _lotteryDurationSeconds;
        private readonly Counter<long> _pollsCreatedTotal;
        private readonly Counter<long> _pollsApprovedTotal;
        private readonly Counter<long> _pollsDeclinedTotal;
        private readonly Counter<long> _pollVotesTotal;
        private readonly ObservableGauge<long> _pollsActive;
        private readonly Counter<long> _happyHourEventsTotal;
        private readonly Counter<long> _happyHourBeerAwardedTotal;
        private readonly Counter<long> _happyHourBeerAwardedAmount;
        private readonly Counter<long> _accountsCreatedTotal;
        private readonly ObservableGauge<long> _accountsActive;
        private readonly Counter<long> _accountJobExecutionsTotal;
        private readonly Counter<long> _renamesCreatedTotal;
        private readonly ObservableGauge<long> _renamesActive;
        private readonly Counter<long> _renamesBoughtOutTotal;
        private readonly Counter<long> _renamesExpiredTotal;
        private readonly Counter<long> _renameCostTotal;
        private readonly Counter<long> _renameJobExecutionsTotal;
        private readonly Counter<long> _jobExecutionsTotal;
        private readonly Histogram<double> _jobExecutionDurationSeconds;
        private readonly Counter<long> _jobExecutionErrorsTotal;
        private readonly Counter<long> _apiRequestsTotal;
        private readonly Histogram<double> _apiRequestDurationSeconds;
        private readonly Counter<long> _apiErrorsTotal;

        private long _totalBeerBalanceValue;
        private long _lotteryPoolSizeValue;
        private long _lotterySubscriptionsActiveValue;
        private long _pollsActiveValue;
        private long _accountsActiveValue;
        private long _renamesActiveValue;

        public FitzMetrics()
        {
            _meter = new Meter("Fitz.Metrics", "1.0.0");

            _beerTransferredTotal = _meter.CreateCounter<long>("fitz_beer_transferred_total");
            _beerTransferredAmount = _meter.CreateCounter<long>("fitz_beer_transferred_amount");
            _beerAwardedTotal = _meter.CreateCounter<long>("fitz_beer_awarded_total");
            _beerAwardedAmount = _meter.CreateCounter<long>("fitz_beer_awarded_amount");
            _beerDeductedTotal = _meter.CreateCounter<long>("fitz_beer_deducted_total");
            _beerDeductedAmount = _meter.CreateCounter<long>("fitz_beer_deducted_amount");
            _totalBeerBalance = _meter.CreateObservableGauge("fitz_total_beer_balance", () => _totalBeerBalanceValue);
            _transactionsTotal = _meter.CreateCounter<long>("fitz_transactions_total");

            _lotteryTicketsPurchasedTotal = _meter.CreateCounter<long>("fitz_lottery_tickets_purchased_total");
            _lotteryTicketsPurchasedAmount = _meter.CreateCounter<long>("fitz_lottery_tickets_purchased_amount");
            _lotteryDrawingsTotal = _meter.CreateCounter<long>("fitz_lottery_drawings_total");
            _lotteryWinnersTotal = _meter.CreateCounter<long>("fitz_lottery_winners_total");
            _lotteryPoolSize = _meter.CreateObservableGauge("fitz_lottery_pool_size", () => _lotteryPoolSizeValue);
            _lotterySubscriptionsActive = _meter.CreateObservableGauge("fitz_lottery_subscriptions_active", () => _lotterySubscriptionsActiveValue);
            _lotteryDurationSeconds = _meter.CreateHistogram<double>("fitz_lottery_duration_seconds");

            _pollsCreatedTotal = _meter.CreateCounter<long>("fitz_polls_created_total");
            _pollsApprovedTotal = _meter.CreateCounter<long>("fitz_polls_approved_total");
            _pollsDeclinedTotal = _meter.CreateCounter<long>("fitz_polls_declined_total");
            _pollVotesTotal = _meter.CreateCounter<long>("fitz_poll_votes_total");
            _pollsActive = _meter.CreateObservableGauge("fitz_polls_active", () => _pollsActiveValue);

            _happyHourEventsTotal = _meter.CreateCounter<long>("fitz_happy_hour_events_total");
            _happyHourBeerAwardedTotal = _meter.CreateCounter<long>("fitz_happy_hour_beer_awarded_total");
            _happyHourBeerAwardedAmount = _meter.CreateCounter<long>("fitz_happy_hour_beer_awarded_amount");

            _accountsCreatedTotal = _meter.CreateCounter<long>("fitz_accounts_created_total");
            _accountsActive = _meter.CreateObservableGauge("fitz_accounts_active", () => _accountsActiveValue);
            _accountJobExecutionsTotal = _meter.CreateCounter<long>("fitz_account_job_executions_total");

            _renamesCreatedTotal = _meter.CreateCounter<long>("fitz_renames_created_total");
            _renamesActive = _meter.CreateObservableGauge("fitz_renames_active", () => _renamesActiveValue);
            _renamesBoughtOutTotal = _meter.CreateCounter<long>("fitz_renames_bought_out_total");
            _renamesExpiredTotal = _meter.CreateCounter<long>("fitz_renames_expired_total");
            _renameCostTotal = _meter.CreateCounter<long>("fitz_rename_cost_total");
            _renameJobExecutionsTotal = _meter.CreateCounter<long>("fitz_rename_job_executions_total");

            _jobExecutionsTotal = _meter.CreateCounter<long>("fitz_job_executions_total");
            _jobExecutionDurationSeconds = _meter.CreateHistogram<double>("fitz_job_execution_duration_seconds");
            _jobExecutionErrorsTotal = _meter.CreateCounter<long>("fitz_job_execution_errors_total");

            _apiRequestsTotal = _meter.CreateCounter<long>("fitz_api_requests_total");
            _apiRequestDurationSeconds = _meter.CreateHistogram<double>("fitz_api_request_duration_seconds");
            _apiErrorsTotal = _meter.CreateCounter<long>("fitz_api_errors_total");
        }

        public void RecordBeerTransfer(int amount, string reason)
        {
            _beerTransferredTotal.Add(1, new KeyValuePair<string, object?>("reason", reason));
            _beerTransferredAmount.Add(amount);
        }

        public void RecordBeerAward(int amount, string reason)
        {
            _beerAwardedTotal.Add(1, new KeyValuePair<string, object?>("reason", reason));
            _beerAwardedAmount.Add(amount);
        }

        public void RecordBeerDeduction(int amount, string reason)
        {
            _beerDeductedTotal.Add(1, new KeyValuePair<string, object?>("reason", reason));
            _beerDeductedAmount.Add(amount);
        }

        public void SetTotalBeerBalance(long balance)
        {
            _totalBeerBalanceValue = balance;
        }

        public void RecordTransaction(string transactionType)
        {
            _transactionsTotal.Add(1, new KeyValuePair<string, object?>("type", transactionType));
        }

        public void RecordLotteryTicketPurchase(int ticketCount, int amount)
        {
            _lotteryTicketsPurchasedTotal.Add(ticketCount);
            _lotteryTicketsPurchasedAmount.Add(amount);
        }

        public void RecordLotteryDrawing(int winnerCount)
        {
            _lotteryDrawingsTotal.Add(1);
            _lotteryWinnersTotal.Add(winnerCount);
        }

        public void SetLotteryPoolSize(long poolSize)
        {
            _lotteryPoolSizeValue = poolSize;
        }

        public void SetLotterySubscriptionsActive(long count)
        {
            _lotterySubscriptionsActiveValue = count;
        }

        public void RecordLotteryDuration(double durationSeconds)
        {
            _lotteryDurationSeconds.Record(durationSeconds);
        }

        public void RecordPollCreated()
        {
            _pollsCreatedTotal.Add(1);
        }

        public void RecordPollApproved()
        {
            _pollsApprovedTotal.Add(1);
        }

        public void RecordPollDeclined()
        {
            _pollsDeclinedTotal.Add(1);
        }

        public void RecordPollVote()
        {
            _pollVotesTotal.Add(1);
        }

        public void SetPollsActive(long count)
        {
            _pollsActiveValue = count;
        }

        public void RecordHappyHourEvent(int beerAwarded)
        {
            _happyHourEventsTotal.Add(1);
            _happyHourBeerAwardedTotal.Add(1);
            _happyHourBeerAwardedAmount.Add(beerAwarded);
        }

        public void RecordAccountCreated()
        {
            _accountsCreatedTotal.Add(1);
        }

        public void SetAccountsActive(long count)
        {
            _accountsActiveValue = count;
        }

        public void RecordAccountJobExecution(string result)
        {
            _accountJobExecutionsTotal.Add(1, new KeyValuePair<string, object?>("result", result));
        }

        public void RecordRenameCreated(int cost)
        {
            _renamesCreatedTotal.Add(1);
            _renameCostTotal.Add(cost);
        }

        public void SetRenamesActive(long count)
        {
            _renamesActiveValue = count;
        }

        public void RecordRenameBoughtOut()
        {
            _renamesBoughtOutTotal.Add(1);
        }

        public void RecordRenameExpired()
        {
            _renamesExpiredTotal.Add(1);
        }

        public void RecordRenameJobExecution(string jobType)
        {
            _renameJobExecutionsTotal.Add(1, new KeyValuePair<string, object?>("job_type", jobType));
        }

        public void RecordJobExecution(string jobName, string result, double durationSeconds)
        {
            _jobExecutionsTotal.Add(1, 
                new KeyValuePair<string, object?>("job_name", jobName),
                new KeyValuePair<string, object?>("result", result));
            _jobExecutionDurationSeconds.Record(durationSeconds, new KeyValuePair<string, object?>("job_name", jobName));
        }

        public void RecordJobExecutionError(string jobName)
        {
            _jobExecutionErrorsTotal.Add(1, new KeyValuePair<string, object?>("job_name", jobName));
        }

        public void RecordApiRequest(string endpoint, string method)
        {
            _apiRequestsTotal.Add(1,
                new KeyValuePair<string, object?>("endpoint", endpoint),
                new KeyValuePair<string, object?>("method", method));
        }

        public void RecordApiRequestDuration(string endpoint, double durationSeconds)
        {
            _apiRequestDurationSeconds.Record(durationSeconds, new KeyValuePair<string, object?>("endpoint", endpoint));
        }

        public void RecordApiError(string endpoint, string errorType)
        {
            _apiErrorsTotal.Add(1,
                new KeyValuePair<string, object?>("endpoint", endpoint),
                new KeyValuePair<string, object?>("error_type", errorType));
        }
    }
}
