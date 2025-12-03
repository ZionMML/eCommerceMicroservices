using eCommerce.Core.DTO;
using eCommerce.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUsersService usersService) : ControllerBase
    {
        private readonly IUsersService _usersService = usersService;

        // GET /api/Users/{userID}
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserByUserID(Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest("Invalid User ID");

            UserDTO? response = await _usersService.GetUserByUserID(userId);

            if (response == null)
                return NotFound();

            return Ok(response);
        }

    }



}
