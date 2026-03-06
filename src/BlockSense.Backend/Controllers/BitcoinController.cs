using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    [ApiController]
    [Route("api/bitcoin")]
    public class BitcoinController : ControllerBase
    {
        private readonly ICryptoService _bitcoinService;

        public BitcoinController([FromKeyedServices("bitcoin")] ICryptoService bitcoinService)
        {
            _bitcoinService = bitcoinService
                ?? throw new ArgumentNullException(nameof(bitcoinService));
        }

        [HttpGet("{address}/balance")]
        [Authorize]
        public async Task<IActionResult> GetBalance(string address)
        {
            var balance = await _bitcoinService.GetBalanceAsync(address);
            return Ok(balance);
        }

        [HttpGet("{address}/transactions")]
        [Authorize]
        public async Task<IActionResult> GetTransactions(string address)
        {
            var result = await _bitcoinService.GetTransactionsAsync(address);
            return Ok(result);
        }

        [HttpPost("broadcast")]
        [Authorize]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastTransactionRequest request)
        {
            var result = await _bitcoinService.BroadcastAsync(request);
            return Ok(result);
        }
    }
}
