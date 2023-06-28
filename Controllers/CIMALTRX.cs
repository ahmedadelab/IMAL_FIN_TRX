using IMAL_FIN_TRX.DLL;
using Microsoft.AspNetCore.Mvc;

namespace IMAL_FIN_TRX.Controllers
{
    public class CIMALTRX : Controller
    {
        BLL dllCode = new BLL();

        [HttpPost("CIMALTRX")]
        public ActionResult<string> Create([FromBody] SIMALTRX x)
        {
            return (dllCode.CreatTrx(x.TransactionType,x.ToAdditionalRef,x.fromAdditionalRef,x.TransactionPurpose,x.TransactionAmount,x.Currency,x.TransactionDate,x.ValueDate,x.UserID,x.Password,x.ChannelName));
        }
    }
}
