using System;
using System.Linq;
using System.Threading.Tasks;
using Boa.Sample.Data;
using Boa.Sample.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Options;
using Boa.Sample.Services;

namespace Boa.Sample.Controllers
{
    public class BoaController : Controller
    {
        public const string EN = "en";
        private const string IT = "it";
        private const string PL = "pl";
        private const string CN = "zh-CN";
        private const string PT = "pt";
        private readonly Dictionary<BrandApiErrorCode, Dictionary<string, string>> BrandErrorCodes = new Dictionary<BrandApiErrorCode, Dictionary<string, string>>() {
            { BrandApiErrorCode.InvalidPlayerId, new Dictionary<string,string>(){
                { EN, "Invalid player ID" },
                { PT, "ID do jogador inválido" },
                { IT, "ID giocatore non valido" },
                { PL, "Nieprawidłowy identyfikator gracza" },
                { CN, "无效的玩家" }
            }},{ BrandApiErrorCode.InvalidCurrencyCodeForPlayer, new Dictionary<string,string>(){
                { EN, "Invalid currency code for player" },
                { PT, "Código de moeda inválido para o jogador" },
                { IT,"Codice valuta non valido per il giocatore" },
                { PL, "Nieprawidłowy kod waluty dla gracza" },
                { CN, "播放器的货币代码无效" }
            }},{ BrandApiErrorCode.InsufficientFunds, new Dictionary<string,string>(){
                { EN, "Insufficient funds"},
                { PT, "Fundos insuficientes"},
                { IT,"Fondi insufficienti"},
                { PL, "Niewystarczające środki" },
                { CN, "不充足的资金" }
            }},{ BrandApiErrorCode.BetExceedsPlayerLimit, new Dictionary<string,string>(){
                { EN, "Bet exceeds player limit"},
                { PT, "A aposta excede o limite do jogador"},
                { IT,"La scommessa supera il limite del giocatore"},
                { PL, "Zakład przekracza limit gracza" },
                { CN, "投注超过玩家限制" }
            }},{ BrandApiErrorCode.InvalidToken, new Dictionary<string,string>(){
                { EN, "Invalid token"},
                { PT, "Token inválido"},
                { IT,"Gettone non valido"},
                { PL, "Nieprawidłowy Token" },
                { CN, "令牌无效" }
            }},{ BrandApiErrorCode.PlayerAccountLockedOrInactive, new Dictionary<string,string>(){
                { EN, "Player account is locked/inactive"},
                { PT, "A conta do jogador está bloqueada / inativa"},
                { IT,"L'account del giocatore è bloccato / non attivo"},
                { PL, "Konto gracza jest zablokowane / nieaktywne" },
                { CN, "玩家帐户被锁定/不活动" }
            }}
        };
        private readonly BoaIntegrationDbContext _context;
        private readonly BoaOptions _boaOptions;
        private readonly EmailSettings _emailSettings;
        private readonly IEmailService _emailSender;

        public BoaController(BoaIntegrationDbContext context,
            IOptions<BoaOptions> boaOptions, IEmailService emailSender, IOptions<EmailSettings> emailSettings)
        {
            _context = context;
            _boaOptions = boaOptions.Value;
            _emailSender = emailSender;
            _emailSettings = emailSettings.Value;
        }

        private string GetLanguage(User user)
        {
            return user?.LanguageCode ?? Request.Cookies["lang"] ?? EN;
        }

        [HttpPost]
        public Task<AuthenticatePlayerResponse> AuthenticatePlayer([FromBody] AuthenticatePlayerRequest request)
        {
            AuthenticatePlayerResponse response = null;
            var result = GetPlayer<AuthenticatePlayerResponse>(request.PlayerToken);
            if (result.Item2 != null)
                response = result.Item2;
            else
                response = new AuthenticatePlayerResponse()
                {
                    CurrencyCode = result.Item1.CurrencyCode,
                    LanguageCode = result.Item1.LanguageCode,
                    PlayerLimit = result.Item1.PlayerLimit,
                    Email = result.Item1.Email,
                    NickName = $"{result.Item1.Name} {result.Item1.Surname}",
                    PlayerId = result.Item1.Id,
                    Code = BrandApiErrorCode.Ok
                };
            return Task.FromResult(response);
        }

        [HttpGet]
        public Task<GetPlayerBalanceResponse> GetPlayerBalance([Bind] GetPlayerBalanceRequest request)
        {
            GetPlayerBalanceResponse response = null;

            var result = GetPlayer<GetPlayerBalanceResponse>(request);
            if (result.Item2 != null)
                response = result.Item2;
            else if (result.Item1.Id != request.PlayerId)
                response = new GetPlayerBalanceResponse()
                {
                    Code = BrandApiErrorCode.InvalidPlayerId,
                    Status = BrandErrorCodes[BrandApiErrorCode.InvalidPlayerId][GetLanguage(result.Item1)]
                };
            else
                response = new GetPlayerBalanceResponse()
                {
                    Code = BrandApiErrorCode.Ok,
                    Amount = result.Item1.Amount
                };

            return Task.FromResult(response);
        }

        [HttpPost]
        public Task<DebitPlayerResponse> DebitPlayer([FromBody] DebitPlayerRequest request)
        {
            DebitPlayerResponse response = null;

            var result = GetPlayer<DebitPlayerResponse>(request);
            if (result.Item2 != null)
                response = result.Item2;

            else if (result.Item1.Id != request.PlayerId)
                response = new DebitPlayerResponse()
                {
                    Code = BrandApiErrorCode.InvalidPlayerId,
                    Status = BrandErrorCodes[BrandApiErrorCode.InvalidPlayerId][GetLanguage(result.Item1)]
                };
            else if (result.Item1.CurrencyCode != request.CurrencyCode)
                response = new DebitPlayerResponse()
                {
                    Code = BrandApiErrorCode.InvalidCurrencyCodeForPlayer,
                    Status = BrandErrorCodes[BrandApiErrorCode.InvalidCurrencyCodeForPlayer][GetLanguage(result.Item1)]
                };
            else if (result.Item1.Amount < request.Amount)
                response = new DebitPlayerResponse()
                {
                    Code = BrandApiErrorCode.InsufficientFunds,
                    Status = BrandErrorCodes[BrandApiErrorCode.InsufficientFunds][GetLanguage(result.Item1)]
                };
            else if (result.Item1.PlayerLimit < request.Amount)
                response = new DebitPlayerResponse()
                {
                    Code = BrandApiErrorCode.BetExceedsPlayerLimit,
                    Status = BrandErrorCodes[BrandApiErrorCode.BetExceedsPlayerLimit][GetLanguage(result.Item1)]
                };
            else
            {
                result.Item1.Amount -= request.Amount;
                _context.SaveChanges();

                response = new DebitPlayerResponse()
                {
                    Code = BrandApiErrorCode.Ok,
                    TransactionId = Guid.NewGuid().ToString()
                };
            }
            return Task.FromResult(response);
        }

        [HttpPost]
        public Task<CreditPlayerResponse> CreditPlayer([FromBody] CreditPlayerRequest request)
        {
            CreditPlayerResponse response = null;

            var result = GetPlayer<CreditPlayerResponse>(request);
            if (result.Item2 != null)
                response = result.Item2;
            else if (result.Item1.Id != request.PlayerId)
                response = new CreditPlayerResponse()
                {
                    Code = BrandApiErrorCode.InvalidPlayerId,
                    Status = BrandErrorCodes[BrandApiErrorCode.InvalidPlayerId][GetLanguage(result.Item1)]
                };
            else if (result.Item1.CurrencyCode != request.CurrencyCode)
                response = new CreditPlayerResponse()
                {
                    Code = BrandApiErrorCode.InvalidCurrencyCodeForPlayer,
                    Status = BrandErrorCodes[BrandApiErrorCode.InvalidCurrencyCodeForPlayer][GetLanguage(result.Item1)]
                };
            else
            {
                result.Item1.Amount += request.Amount;
                _context.SaveChanges();

                response = new CreditPlayerResponse()
                {
                    Code = BrandApiErrorCode.Ok,
                    TransactionId = Guid.NewGuid().ToString()
                };
            }

            return Task.FromResult(response);
        }

        [HttpPost]
        public Task<BetPlacedResponse> BetPlaced([FromBody] BetPlacedRequest request)
        {
            var response = new BetPlacedResponse()
            {
                Code = BrandApiErrorCode.Ok
            };
            return Task.FromResult(response);
        }

        [HttpPost]
        public Task<CancelBetResponse> CancelBet([FromBody] CancelBetRequest request)
        {
            var response = new CancelBetResponse()
            {
                Code = BrandApiErrorCode.Ok
            };
            return Task.FromResult(response);
        }


        [HttpPost]
        public async Task<JackpotEventSettledResponse> JackpotEventSettled([FromBody] JackpotEventSettledRequest request)
        {
            return new JackpotEventSettledResponse()
            {
                Code = BrandApiErrorCode.Ok
            };
        }


        private Tuple<User, T> GetPlayer<T>(BaseBrandApiRequest request)
            where T : BaseBrandApiResponse, new()
        {
            return GetPlayer<T>(request.PlayerToken);
        }

        private Tuple<User, T> GetPlayer<T>(string token) where T : BaseBrandApiResponse, new()
        {
            var player = _context.Users.FirstOrDefault(x => x.Token == token);
            if (player == null)
                return Tuple.Create(player, new T()
                {
                    Code = BrandApiErrorCode.InvalidToken,
                    Status = BrandErrorCodes[BrandApiErrorCode.InvalidToken][GetLanguage(player)]
                });

            if (!player.IsActive)
                return Tuple.Create(player, new T()
                {
                    Code = BrandApiErrorCode.PlayerAccountLockedOrInactive,
                    Status = BrandErrorCodes[BrandApiErrorCode.PlayerAccountLockedOrInactive][GetLanguage(player)]
                });

            return Tuple.Create(player, default(T));
        }
    }
}