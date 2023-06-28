namespace IMAL_FIN_TRX.DLL
{
    public class PS
    {
        public class RespCreateTransfer
        {

            public string? transactionAmount { get; set; }
            public string? transactionDate { get; set; }
            public string? transactionNumber { get; set; }
            public string? transactionPurpose { get; set; }
            public string? transactionType { get; set; }
            public string? StatusCode { get; set; }
            public string? StatusDesc { get; set; }

        }

        public class ReqCreateTransfer
        {
            public string? TransactionType { get; set; }
            public string? ToAdditionalRef { get; set; }
            public string? fromAdditionalRef { get; set; }
            public string? TransactionPurpose { get; set; }
            public string? TransactionAmount { get; set; }
            public string? Currency { get; set; }
            public string? TransactionDate { get; set; }
            public string? ValueDate { get; set; }
            public string? UserID { get; set; }
            public string? Password { get; set; }
            public string? ChannelName { get; set; }
        }
    }
}
