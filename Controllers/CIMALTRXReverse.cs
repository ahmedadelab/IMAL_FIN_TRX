using IMAL_FIN_TRX.DLL;
using Microsoft.AspNetCore.Mvc;

namespace IMAL_FIN_TRX.Controllers
{
    public class CIMALTRXReverse : Controller
    {
        BLL dllCode = new BLL();
        [HttpPost("CIMALTRXReverse")]
        public ActionResult<string> Reverse([FromBody] SIMALTRXRev x)
        {
            return (dllCode.ReverseTrx(x.branchCode, x.transactionNumber, x.reason, x.UserID, x.Password, x.ChannelName));
        }
    }
}
