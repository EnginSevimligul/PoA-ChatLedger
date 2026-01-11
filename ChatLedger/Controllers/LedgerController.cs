using ChatLedger.Models;
using ChatLedger.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatLedger.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LedgerController : Controller
    {
        private readonly BlockchainService _blockchain;

        public LedgerController(BlockchainService blockchain)
        {
            _blockchain = blockchain;
        }

        [HttpPost("sign")]
        public async Task<IActionResult> SignFile([FromBody] ChatLog request)
        {
            try
            {
                var block = await _blockchain.AddMessageAsync(request);
                return Ok(new
                {
                    Message = "Kayıt Başarılı",
                    BlockIndex = block.Index,
                    BlockHash = block.Hash
                });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(409, new { Error = "CHAIN_CORRUPTED", Detail = ex.Message });
            }
        }

        [HttpGet("audit")]
        public async Task<IActionResult> Audit()
        {
            var result = await _blockchain.ValidateChainAsync();
            return result.IsValid ? Ok(new { Status = "OK", Msg = result.Message })
                : StatusCode(500, new { Status = "ERROR", Msg = result.Message });
        }
    }
}
