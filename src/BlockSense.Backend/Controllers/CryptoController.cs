using BlockSense.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    /// <summary>
    /// Provides endpoints for general cryptocurrency data, such as exchange rates.
    /// </summary>
    [ApiController]
    [Route("api/crypto")]
    public class CryptoController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoController"/> class.
        /// </summary>
        /// <param name="exchangeRateService">Service used to retrieve asset exchange rates.</param>
        public CryptoController(IExchangeRateService exchangeRateService)
        {
            _exchangeRateService = exchangeRateService
                ?? throw new ArgumentNullException(nameof(exchangeRateService));
        }

        /// <summary>
        /// Returns the current exchange rate between two assets.
        /// </summary>
        /// <param name="fromAssetSymbol">The symbol of the source asset (e.g., <c>BTC</c>).</param>
        /// <param name="toAssetSymbol">The symbol of the target asset (e.g., <c>USD</c>).</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>The current exchange rate from <paramref name="fromAssetSymbol"/> to <paramref name="toAssetSymbol"/>.</returns>
        [HttpGet("exchange-rate/{fromAssetSymbol}/{toAssetSymbol}")]
        [Authorize]
        public async Task<IActionResult> GetExchangeRate(
            string fromAssetSymbol,
            string toAssetSymbol,
            CancellationToken cancellationToken)
        {
            var rate = await _exchangeRateService.GetRateAsync(fromAssetSymbol, toAssetSymbol, cancellationToken);

            return Ok(rate);
        }
    }
}