using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainAssignment.Controllers
{ 

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Authorize(Roles = "Administrator")]
    public class AdminController : ControllerBase
    {
    }
}
