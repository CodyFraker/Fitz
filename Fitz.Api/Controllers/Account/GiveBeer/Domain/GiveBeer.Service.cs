using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Database.Entities;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Bank;
using Fitz.Core.Discord;

namespace Fitz.Api.Controllers.Account.GiveBeer.Domain;

public class GiveBeerService(
    IGiveBeer giveBeer,
    BankService bankService,
    AccountService accountService,
    BotLog botLog,
    ILogger<GiveBeerService> logger)
{
    private readonly IGiveBeer _giveBeer = giveBeer;
    private readonly BankService _bankService = bankService;
    private readonly AccountService _accountService = accountService;
    private readonly BotLog _botLog = botLog;
    private readonly ILogger<GiveBeerService> _logger = logger;

    public async Task<GiveBeerModel> ExecuteAsync(GiveBeerCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GiveBeerService execution started. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        if (command.Amount <= 0)
        {
            _logger.LogError("GiveBeer validation failed - Amount must be greater than 0. Amount: {Amount}", command.Amount);
            throw new ArgumentException("Amount must be greater than 0.", nameof(command.Amount));
        }

        var accountEntity = await _giveBeer.FindAccountByIdAsync(command.UserId, cancellationToken);
        if (accountEntity == null)
        {
            _logger.LogWarning("Account not found. UserId: {UserId}", command.UserId);
            throw new AccountNotFound(command.UserId);
        }

        if (accountEntity.Beer < command.Amount)
        {
            _logger.LogWarning("User does not have enough beer. UserId: {UserId}, Required: {Required}, Current: {Current}", 
                command.UserId, command.Amount, accountEntity.Beer);
            throw new InvalidOperationException("You don't have enough money to give that much beer!");
        }

        double percentageOfBeer = (double)command.Amount / accountEntity.Beer * 100;

        double newFavorability = 0;

        if (accountEntity.Favorability <= 5)
        {
            newFavorability = accountEntity.Favorability + (percentageOfBeer * .2);
        }
        else if (accountEntity.Favorability <= 20 && accountEntity.Favorability >= 6)
        {
            newFavorability = accountEntity.Favorability + (percentageOfBeer * .3);
        }
        else if (accountEntity.Favorability <= 40 && accountEntity.Favorability >= 21)
        {
            newFavorability = accountEntity.Favorability + (percentageOfBeer * .4);
        }
        else if (accountEntity.Favorability <= 60 && accountEntity.Favorability >= 41)
        {
            newFavorability = accountEntity.Favorability + (percentageOfBeer * .5);
        }
        else if (accountEntity.Favorability <= 80 && accountEntity.Favorability >= 61)
        {
            newFavorability = accountEntity.Favorability + (percentageOfBeer * .6);
        }

        if (newFavorability > 100)
        {
            newFavorability = 100;
        }

        var scopeFactory = _giveBeer.GetScopeFactory();
        var setFavorabilityCommand = new SetFavorabilityCommand(scopeFactory, _botLog);
        var favorabilityResult = await setFavorabilityCommand.ExecuteAsync(accountEntity, (int)Math.Floor(newFavorability));
        
        if (!favorabilityResult.Success)
        {
            _logger.LogWarning("Failed to set favorability. UserId: {UserId}, Message: {Message}", command.UserId, favorabilityResult.Message);
        }

        var transferResult = await _bankService.TransferToFitz(accountEntity.Id, command.Amount, Reason.Donated);
        
        if (!transferResult.Success)
        {
            _logger.LogError("Failed to transfer beer to Fitz. UserId: {UserId}, Message: {Message}", command.UserId, transferResult.Message);
            throw new InvalidOperationException(transferResult.Message);
        }

        var model = GiveBeerModel.From(command.UserId, command.Amount, (int)Math.Floor(newFavorability), "Thanks for the beer.");

        _logger.LogInformation("GiveBeerModel created successfully. UserId: {UserId}, Amount: {Amount}, NewFavorability: {NewFavorability}", 
            command.UserId, command.Amount, model.NewFavorability);

        return model;
    }
}
