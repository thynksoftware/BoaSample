
namespace Boa.Sample.Models
{
    public enum BrandApiErrorCode
    {
        Ok = 0,
        InvalidToken = 100,
        PlayerAccountLockedOrInactive = 101,
        InvalidPlayerId = 110,
        InvalidCurrencyCodeForPlayer = 120,
        InsufficientFunds = 121,
        BetExceedsPlayerLimit = 122,
        TransactionAlreadyProcessed = 123,
        OriginalTransactionNotFound = 131
    }
}