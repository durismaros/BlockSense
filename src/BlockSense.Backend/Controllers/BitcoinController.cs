using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    /// <summary>
    /// Provides endpoints for interacting with the Bitcoin blockchain,
    /// including balance lookups, transaction history, and transaction broadcasting.
    /// </summary>
    [ApiController]
    [Route("api/bitcoin")]
    public class BitcoinController : ControllerBase
    {
        private readonly ICryptoService _bitcoinService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BitcoinController"/> class.
        /// </summary>
        /// <param name="bitcoinService">The Bitcoin-specific crypto service resolved by key.</param>
        public BitcoinController([FromKeyedServices("bitcoin")] ICryptoService bitcoinService)
        {
            _bitcoinService = bitcoinService
                ?? throw new ArgumentNullException(nameof(bitcoinService));
        }

        /// <summary>
        /// Returns the current balance for the specified Bitcoin address.
        /// </summary>
        /// <param name="address">The Bitcoin address to query.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>The current balance of the given address.</returns>
        [HttpGet("{address}/balance")]
        [Authorize]
        public async Task<IActionResult> GetBalance(string address, CancellationToken cancellationToken)
        {
            var balance = await _bitcoinService.GetBalanceAsync(address);

            return Ok(balance);
        }

        /// <summary>
        /// Returns the transaction history for the specified Bitcoin address.
        /// </summary>
        /// <param name="address">The Bitcoin address to query.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>A list of transactions associated with the given address.</returns>
        [HttpGet("{address}/transactions")]
        [Authorize]
        public async Task<IActionResult> GetTransactions(string address, CancellationToken cancellationToken)
        {
            var result = await _bitcoinService.GetTransactionsAsync(address);

            return Ok(result);
        }

        /// <summary>
        /// Broadcasts a signed Bitcoin transaction to the network.
        /// </summary>
        /// <param name="request">The request containing the signed raw transaction.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>The broadcast result including the transaction ID.</returns>
        [HttpPost("broadcast")]
        [Authorize]
        public async Task<IActionResult> Broadcast(
            [FromBody] BroadcastTransactionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _bitcoinService.BroadcastAsync(request);

            return Ok(result);
        }
    }
}