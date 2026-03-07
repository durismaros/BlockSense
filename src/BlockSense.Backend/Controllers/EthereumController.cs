using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    [ApiController]
    [Route("api/ethereum")]
    public class EthereumController : ControllerBase
    {
        private readonly ICryptoService _ethereumService;

        public EthereumController([FromKeyedServices("ethereum")] ICryptoService ethereumService)
        {
            _ethereumService = ethereumService
                ?? throw new ArgumentNullException(nameof(ethereumService));
        }

        [HttpGet("{address}/balance")]
        [Authorize]
        public async Task<IActionResult> GetBalance(string address)
        {
            var balance = await _ethereumService.GetBalanceAsync(address);
            return Ok(balance);
        }

        [HttpGet("{address}/next-available-nonce")]
        [Authorize]
        public async Task<IActionResult> GetNextAvailableNonce(string address)
        {
            var nonce = await _ethereumService.GetNextAvailableNonce(address);
            return Ok(nonce);
        }

        [HttpGet("{address}/transactions")]
        [Authorize]
        public async Task<IActionResult> GetTransactions(string address)
        {
            var result = await _ethereumService.GetTransactionsAsync(address);
            return Ok(result);
        }

        [HttpPost("broadcast")]
        [Authorize]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastTransactionRequest request)
        {
            var result = await _ethereumService.BroadcastAsync(request);
            return Ok(result);
        }
    }
}
