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
    public class RouteController : ControllerBase
    {

        private readonly ILogger _logger;
        private readonly RouteQualityService _routeQualityService;
        private readonly UserStatisticsService _userStatisticsService;
        private readonly EnumParser<PointType> _pointTypeParser;
        public RouteController(ILogger<RoadPointDTO> logger, RouteQualityService routeQualityService, UserStatisticsService userStatisticsService)
        {
            _logger = logger;
            _routeQualityService = routeQualityService;
            _userStatisticsService = userStatisticsService;
            _pointTypeParser = new EnumParser<PointType>();

        }

        [HttpPost("point")]
        [Authorize]
        public async Task<IActionResult> Point([FromBody]List<RoadPointDTO> data) {
            if(data.Count > 0)
            {
                _logger.LogInformation("Point: " + data[0].Longitude + " " + data[0].Latitude + " " + data[0].Speed + " " + data[0].Vector);
                foreach(RoadPointDTO point in data)
                {
                    await _routeQualityService.AddPoint(point, User.FindFirstValue(ClaimTypes.Sid));
                    await _userStatisticsService.AddGeoPointToUserStats(point, User.FindFirstValue(ClaimTypes.Sid));
                }
            }
            return Ok();
        }

        [HttpGet("getPointsByGeo")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPointsByGeo([FromQuery] GeoQueryDTO query)
        {
            return Ok(await _routeQualityService.GetPointsByGeo(query, User.FindFirstValue(ClaimTypes.Sid)));
        }

        [HttpGet("getPoints")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPoints([FromQuery] FilterQuery query)
        {
            return Ok(await _routeQualityService.GetPoints(query, User.FindFirstValue(ClaimTypes.Sid)));
        }

        [HttpGet("getAllRawPoints")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllRawPoints()
        {
            return Ok(await _routeQualityService.GetAllRaw());
        }

        [HttpPost("removeAll")]
        [AllowAnonymous]
        public async Task<IActionResult> RemoveAll()
        {
            await _routeQualityService.RemoveAll();
            return Ok();
        }
    }
}
