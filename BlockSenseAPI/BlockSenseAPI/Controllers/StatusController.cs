using BlockSenseAPI.Models;
using BlockSenseAPI.Services;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;

namespace BlockSenseAPI.Controllers
{
    [ApiController]
    [Route("api/status")]
    public class StatusController : Controller
    {
        private readonly MySqlConnection _dbConnection;

        public StatusController(MySqlConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        [HttpGet("check")]
        public async Task<ActionResult<ServerStatusModel>> CheckStatus()
        {
            try
            {
                // Basic server availability
                var serverStatus = "Online";

                // MySQL check
                var dbStatus = await CheckDatabaseAsync();

                return Ok(new ServerStatusModel
                {
                    Status = serverStatus,
                    DbStatus = dbStatus ? "Healthy" : "Unavailable",
                    TimeStamp = DateTime.UtcNow.ToString("o")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Unavailable",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow.ToString("o")
                });
            }
        }

        private async Task<bool> CheckDatabaseAsync()
        {
            try
            {
                await _dbConnection.OpenAsync();
                await _dbConnection.CloseAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
