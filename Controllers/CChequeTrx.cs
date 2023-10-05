using IMAL_FIN_TRX.DLL;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using static IMAL_FIN_TRX.DLL.PS;

namespace IMAL_FIN_TRX.Controllers
{
    public class CChequeTrx : Controller
    {
        BLL dllCode = new BLL();

        [HttpPost("CChequeTrx")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ChequeTRXResponse), 200)]
        public ActionResult<string> Create([FromBody] ChequeTRXRequest x)
        {
            return Ok(dllCode.ChequeTransaction(
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