using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoadQuality.DTO;
using RoadQuality.Enums;
using RoadQuality.Models;
using RoadQuality.Services;
using RoadQuality.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RoadQuality.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly UserStatisticsService _userStatisticsService;

        public StatisticsController(ILogger<RoadPointDTO> logger, UserStatisticsService userStatisticsService)
        {
            _logger = logger;
            _userStatisticsService = userStatisticsService;
        }

        [HttpGet("getUserStats")]
        [Authorize]
        public async Task<IActionResult> GetUserStats([FromQuery] DateRangeDTO dateRange)
        {
            return Ok( await _userStatisticsService.GetUserStats(User.FindFirstValue(ClaimTypes.Sid), dateRange));
        }

        [HttpGet("getUserOverallStats")]
        [Authorize]
        public async Task<IActionResult> GetUserOverallStats()
        {
            return Ok(await _userStatisticsService.GetOverallUserStats(User.FindFirstValue(ClaimTypes.Sid)));
        }

        [HttpGet("getOverallStats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOverallStats()
        {
            return Ok(await _userStatisticsService.GetOverallStats());
        }
    }
}
