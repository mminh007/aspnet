using Backend.Authentication.Enums;
using Backend.Authentication.HttpsClient;
using Backend.Authentication.Models;
using Backend.Authentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Net;

namespace Backend.Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserApiService _userApiService;
        private readonly ITokenService _tokenService;

        public AuthController(UserApiService userApiService, ITokenService tokenService)
        {
            _userApiService = userApiService;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CheckUserModel userCheck = new CheckUserModel()
            {
                Email = model.Email,
                Role = model.Role,
            };

            // Get UserID
            var response = await _userApiService.RegisterUserAsync(userCheck);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var items = response.Content.ReadFromJsonAsync<RegisterUserResponseModel>().Result;
                var userId = items.UserId;
                var result = await _tokenService.RegisterAsync(model, userId);

                return result switch
                {
                    OperationResult.Success => Ok(new { message = "Register Successfully!" }),
                    OperationResult.Failed => BadRequest(new { message = "Registration failed" }),
                    OperationResult.NotFound => NotFound(new { message = "User Service unavailable!" }),
                    _ => StatusCode(500, new { message = "Unexpected error!" })
                };
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return Conflict(new { message = "Email already exists!" }); // 409
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(new { message = "Invalid request to user service" }); // 400
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new { message = "User service unavailable" }); // 404
            }
            else
            {
                return StatusCode(500, new { message = "Unexpected error!" }); // 500
            }


        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _tokenService.Authenticate(model);

            return result.Message switch
            {
                OperationResult.Success => Ok(new { message = "Login Successfully!", access_token = result.AccessToken}),
                OperationResult.Failed => BadRequest(new { message = result.ErrorMessage }),
                OperationResult.NotFound => NotFound(new { message = result.ErrorMessage }),
                _ => StatusCode(500, new { message = "Unexpected error!" })
            };




        }
    }
}
