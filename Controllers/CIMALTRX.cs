using IMAL_FIN_TRX.DLL;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Xml.Schema;
using System.Xml.Serialization;
using static IMAL_FIN_TRX.DLL.PS;


namespace IMAL_FIN_TRX.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class CIMALTRX : Controller
    {
        BLL dllCode = new BLL();
  
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [HttpPost]
        [ProducesResponseType(typeof(CreateTransferResponse), 200)]
        public ActionResult<string> Create([FromBody] IMALTRXRequest x)
        {
            return Ok(dllCode.CreatTrx(x.TransactionType,x.ToAdditionalRef,x.fromAdditionalRef,x.TransactionPurpose,x.TransactionAmount,x.Currency,x.TransactionDate,x.ValueDate,x.UserID,x.Password,x.ChannelName,x.TransferDesc));
        }
    }
}
