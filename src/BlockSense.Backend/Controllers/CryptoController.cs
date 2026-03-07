using BlockSense.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    [ApiController]
    [Route("api/crypto")]
    public class CryptoController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService;

        public CryptoController(IExchangeRateService exchangeRateService)
        {
            _exchangeRateService = exchangeRateService
                ?? throw new ArgumentNullException(nameof(exchangeRateService));
        }

        [HttpGet("exchange-rate/{fromAssetSymbol}/{toAssetSymbol}")]
        [Authorize]
        public async Task<IActionResult> GetBalance(string fromAssetSymbol, string toAssetSymbol, CancellationToken cancellationToken)
        {
            var balance = await _exchangeRateService.GetRateAsync(fromAssetSymbol, toAssetSymbol, cancellationToken);
            return Ok(balance);
        }
    }
}
