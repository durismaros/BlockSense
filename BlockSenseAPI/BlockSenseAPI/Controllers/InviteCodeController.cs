using BlockSenseAPI.Models;
using BlockSenseAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace BlockSenseAPI.Controllers
{
    [ApiController]
    [Route("api/invites")]
    public class InviteCodeController : ControllerBase
    {
        private readonly IInviteCodeService _inviteCodeService;

        public InviteCodeController(IInviteCodeService inviteCodeService)
        {
            _inviteCodeService = inviteCodeService;
        }

        [HttpGet("fetch-all")]
        [Authorize]
        public async Task<ActionResult<List<InviteInfoModel>>> FetchInviteInfoEndpoint()
        {
            try
            {
                if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int userId))
                    return Unauthorized("User ID not found in token");

                var invitesInfo = await _inviteCodeService.FetchAllInvites(userId);
                return Ok(invitesInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
