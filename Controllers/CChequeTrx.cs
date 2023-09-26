using IMAL_FIN_TRX.DLL;
using Microsoft.AspNetCore.Mvc;

namespace IMAL_FIN_TRX.Controllers
{
    public class CChequeTrx : Controller
    {
        BLL dllCode = new BLL();

        [HttpPost("CChequeTrx")]
        public ActionResult<string> Create([FromBody] SChequeTRX x)
        {
            return (dllCode.ChequeTransaction(
            x.transactionType,
            x.CreditAdditionalRef,
            x.DebitAdditionalRef,
            x.transactionAmount,
            x.currencyIso,
            x.chequeNumber,
            x.chequeDate,
            x.valueDate,          
            x.UserID,
            x.Password,
            x.ChannelName
            ));
        }
    }
}