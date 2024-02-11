using IMAL_FIN_TRX.DLL;
using Microsoft.AspNetCore.Mvc;
using static IMAL_FIN_TRX.DLL.PS;
using System.Net.Mime;

namespace IMAL_FIN_TRX.Controllers
{
    public class CIMALTRXReverse : Controller
    {
        BLL dllCode = new BLL();

        [Consumes(MediaTypeNames.Application.Json)]
   
        [ProducesResponseType(typeof(ReverseTransactionResponse), 200)]
        [HttpPost("CIMALTRXReverse")]
        public ActionResult<string> Reverse([FromBody] IMALTRXRevRequest x)
        {
            return Ok(dllCode.ReverseTrx(x.branchCode, x.transactionNumber, x.reason, x.UserID, x.Password, x.ChannelName));
        }
    }
}
