using Commons.Enums;
using Commons.Models.Requests;
using Commons.Models.Responses;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;
        private readonly ILogger<StoreController> _logger;

        public StoreController(IStoreService storeService, ILogger<StoreController> logger)
        {
            _storeService = storeService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "system")]
        public async Task<IActionResult> CreateStore([FromBody] RegisterStoreModel model)
        {
            var result = await _storeService.CreateStoreAsync(model);
            return HandleResponse(result);
        }


        [HttpGet("{userId:guid}")]
        [Authorize(Roles = "seller, system")]
        public async Task<IActionResult> GetStoreById(Guid userId)
        {
            var result = await _storeService.GetStoreByIdAsync(userId);
            return HandleResponse(result);
        }

        [HttpPut]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> UpdateStore([FromBody] UpdateStoreModel model)
        {
            var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);            
            model.UserId = userId;

            var result = await _storeService.UpdateStoreAsync(model);
            return HandleResponse(result);
        }

        [HttpDelete("{userId:guid}")]
        [Authorize(Roles = "seller, admin")]
        public async Task<IActionResult> DeleteStore(Guid userId)
        {
            var result = await _storeService.DeleteStoreAsync(userId);
            return HandleResponse(result);
        }

        [HttpPatch("change-active")]
        [Authorize(Roles = "seller, system")]
        public async Task<IActionResult> ChangeActive([FromBody] StoreActiveModel model)
        {
            if (User.IsInRole("seller"))
            {
                var userIdFromToken = User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value;
                //var userIdFromToken = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                model.UserId = Guid.Parse(userIdFromToken!);
            }

            var result = await _storeService.ChangeActive(model);
            return HandleResponse(result);
        }

        // ---------------------------
        // Buyer / Public APIs
        // ---------------------------

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllStores()
        {
            var result = await _storeService.GetAllActiveStoresAsync();
            return HandleResponse(result);
        }

        [HttpGet("detail/{storeId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStoreDetail(Guid storeId)
        {
            var result = await _storeService.GetStoreDetailByIdAsync(storeId);
            return HandleResponse(result);
        }

        private IActionResult HandleResponse<T>(StoreResponseModel<T> response)
        {
            return response.Message switch
            {
                OperationResult.Success => Ok(new
                {
                    data = response.Data,
                    message = "Operation successful"
                }),
                OperationResult.NotFound => NotFound(new { message = response.ErrorMessage ?? "Not found" }),
                OperationResult.Failed => BadRequest(new { message = response.ErrorMessage ?? "Operation failed" }),
                OperationResult.Error => StatusCode(500, new { message = response.ErrorMessage ?? "Internal error!" }),
                _ => StatusCode(500, new { message = "Unexpected error!" })
            };
        }

    }
}
