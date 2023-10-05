namespace IMAL_FIN_TRX
{
    public class ChequeTRXRequest
    {
      
            public string transactionType { get; set; }
            public string CreditAdditionalRef { get; set; }
            public string DebitAdditionalRef { get; set; }
            public string transactionAmount { get; set; }
            public string currencyIso { get; set; }
            public string chequeNumber { get; set; }
            public string chequeDate { get; set; }
            public string TransactionDate { get; set; }
            public string valueDate { get; set; }
          
            public string UserID { get; set; }
            public string Password { get; set; }
            public string ChannelName { get; set; }
       
    }
}