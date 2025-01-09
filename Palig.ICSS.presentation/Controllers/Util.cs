using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Palig.ICSS.support.Services.Services;


namespace Palig.ICSS.presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class Util : ControllerBase
    {


        [HttpGet("KeyAes")]
        public async Task<IActionResult> KeyAes()
        {

            var result = AESHelper.GenerateKey();

            return Ok(result);
        }

    }
}
