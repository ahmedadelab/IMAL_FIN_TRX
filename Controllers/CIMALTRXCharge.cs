using IMAL_FIN_TRX.DLL;
using Microsoft.AspNetCore.Mvc;

namespace IMAL_FIN_TRX.Controllers
{
    public class CIMALTRXCharge : Controller
    {
        BLL dllCode = new BLL();

        [HttpPost("CIMALTRXCharge")]
        public ActionResult<string> Create([FromBody] SIMALTRXCharge x)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            return dllCode.CreatTrxCharge(x.TransactionType, x.ToAdditionalRef, x.fromAdditionalRef, x.TransactionPurpose, x.TransactionAmount, x.Currency, x.TransactionDate, x.ValueDate, x.UserID, x.Password, x.ChannelName, x.TransferDesc,x.ChargeCode1,x.ChargeCodeAmount1,x.ChargeCode2,x.ChargeCodeAmount2);
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}
