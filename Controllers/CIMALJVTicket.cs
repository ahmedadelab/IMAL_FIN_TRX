using IMAL_FIN_TRX.DLL;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace IMAL_FIN_TRX.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CIMALJVTicket : Controller
    {
        BLL bllCode = new BLL();
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(List<SIMALJVTicket.JVTicketResponse>), StatusCodes.Status200OK)]
        [HttpPost(Name = "SIMALCreateJvTicket")]
        public ActionResult<List<SIMALJVTicket.JVTicketResponse>> Create([FromBody] SIMALJVTicket.JVTicketRequest x)
        {
            var response = bllCode.IMALCreateJVTicket(x, x.UserID, x.Password, x.ChannelName);
            return Ok(response);
        }
    }
}