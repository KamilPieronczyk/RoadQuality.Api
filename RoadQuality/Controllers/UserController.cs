using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoadQuality.Models;
using RoadQuality.Services;
using RoadQuality.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoadQuality.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public class AuthenticateRequest
        {
            [Required]
            public string IdToken { get; set; }
        }

        public class AuthenticateRequestMobile
        {
            [Required]
            public string accessToken { get; set; }
        }


        private readonly JwtGenerator _jwtGenerator;
        private readonly UserService _userService;
        private readonly ILogger _logger;

        public UserController(IConfiguration configuration, UserService userService, ILogger<String> logger)
        {
            _jwtGenerator = new JwtGenerator(configuration.GetValue<string>("JwtPrivateSigningKey"));
            _userService = userService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<ActionResult<User>> Authenticate([FromBody] AuthenticateRequest data)
        {
            GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings();
            // Change this to your google client ID
            settings.Audience = new List<string>() {
                "718142606472-4rioofpfvdeso0lg2aik720n57s2msli.apps.googleusercontent.com",
                "718142606472-et0c0q9m85s59p7u0hh450hvhp751ad2.apps.googleusercontent.com",
                "718142606472-c8b7930rotlplst1hi7jabb62caq2c9q.apps.googleusercontent.com",
            };

            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(data.IdToken, settings);
            var user = _userService.Create(new User() { Email = payload.Email});
            var token = _jwtGenerator.CreateUserAuthToken(user.Id, user.Email);
            user.JWTToken = token;
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("authenticateMobile")]
        public ActionResult<User> AuthenticateMobile([FromBody] AuthenticateRequestMobile data)
        {
            string json = (new WebClient()).DownloadString("https://www.googleapis.com/oauth2/v3/tokeninfo?access_token=" + data.accessToken);
            var responseContent = JsonConvert.DeserializeObject<GoogleAccessTokenResponse>(json);

            if (responseContent.Email != null)
            {
                var user = _userService.Create(new User() { Email = responseContent.Email });
                var token = _jwtGenerator.CreateUserAuthToken(user.Id, user.Email);
                user.JWTToken = token;
                return Ok(user);
            }
            else
            {
                return BadRequest(new Dictionary<string, string>
                    {
                        { "AccessToken", "Access token invalid" } 
                    }
                );
            }
        }

        [HttpGet("getUserProfile")]
        [Authorize]
        public IActionResult GetUserProfile()
        {
            User user = _userService.Get(User.FindFirstValue(ClaimTypes.Sid));

            return Ok(user);
        }

        [HttpPost("updateUserProfile")]
        [Authorize]
        public IActionResult UpdateUserProfile([FromBody] User newUser)
        {
            User user = _userService.Get(User.FindFirstValue(ClaimTypes.Sid));
            
            newUser.Id = user.Id;

            User updatedUser = _userService.UpdateUserProfile(newUser);

            return Ok(updatedUser);
        }
    }
}
