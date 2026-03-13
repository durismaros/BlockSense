using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    /// <summary>
    /// Provides endpoints for interacting with the Ethereum blockchain,
    /// including balance lookups, nonce resolution, transaction history, and broadcasting.
    /// </summary>
    [ApiController]
    [Route("api/ethereum")]
    public class EthereumController : ControllerBase
    {
        private readonly ICryptoService _ethereumService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EthereumController"/> class.
        /// </summary>
        /// <param name="ethereumService">The Ethereum-specific crypto service resolved by key.</param>
        public EthereumController([FromKeyedServices("ethereum")] ICryptoService ethereumService)
        {
            _ethereumService = ethereumService
                ?? throw new ArgumentNullException(nameof(ethereumService));
        }

        /// <summary>
        /// Returns the current balance for the specified Ethereum address.
        /// </summary>
        /// <param name="address">The Ethereum address to query.</param>
        /// <returns>The current balance of the given address.</returns>
        [HttpGet("{address}/balance")]
        [Authorize]
        public async Task<IActionResult> GetBalance(string address)
        {
            var balance = await _ethereumService.GetBalanceAsync(address);

            return Ok(balance);
        }

        /// <summary>
        /// Returns the next available nonce for the specified Ethereum address.
        /// </summary>
        /// <param name="address">The Ethereum address to query.</param>
        /// <returns>The next available transaction nonce for the given address.</returns>
        [HttpGet("{address}/next-available-nonce")]
        [Authorize]
        public async Task<IActionResult> GetNextAvailableNonce(string address)
        {
            var nonce = await _ethereumService.GetNextAvailableNonce(address);

            return Ok(nonce);
        }

        /// <summary>
        /// Returns the transaction history for the specified Ethereum address.
        /// </summary>
        /// <param name="address">The Ethereum address to query.</param>
        /// <returns>A list of transactions associated with the given address.</returns>
        [HttpGet("{address}/transactions")]
        [Authorize]
        public async Task<IActionResult> GetTransactions(string address)
        {
            var result = await _ethereumService.GetTransactionsAsync(address);

            return Ok(result);
        }

        /// <summary>
        /// Broadcasts a signed Ethereum transaction to the network.
        /// </summary>
        /// <param name="request">The request containing the signed raw transaction.</param>
        /// <returns>The broadcast result including the transaction hash.</returns>
        [HttpPost("broadcast")]
        [Authorize]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastTransactionRequest request)
        {
            var result = await _ethereumService.BroadcastAsync(request);

            return Ok(result);
        }
    }
}