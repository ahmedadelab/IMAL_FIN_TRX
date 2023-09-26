using IMAL_FIN_TRX.DLL;
using Microsoft.AspNetCore.Mvc;
using static IMAL_FIN_TRX.SIMALTRX;

namespace IMAL_FIN_TRX.Controllers
{
    public class RevTRXController : Controller
    {
        BLL dllCode = new BLL();

        [HttpPost("REVIMALTRX")]
        public ActionResult<string> Reverse([FromBody] SIMALTRXRev x)
        {
            return (dllCode.ReverseTrx(x.branchCode,x.transactionNumber,x.reason,x.UserID,x.Password,x.ChannelName));
        }
    }
}
