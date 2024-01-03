namespace IMAL_FIN_TRX
{
    public class SIMALJVTicket
    {
        public class JVTicketList
        {
            public string? LineNo { get; set; }
            public string? BranchCode { get; set; }
            public string? Currency { get; set; }
            public string? AccGL { get; set; }
            public string? SerialNo { get; set; }
            public string? Cif { get; set; }
            public string? CVAmount { get; set; }
            public string? FCAmount { get; set; }
            public string? ExchangeRate { get; set; }
            public string? JVDescription { get; set; }
            public string? TransactionTypeCode { get; set; }
            public string? JVTypeNumber { get; set; }
            public string? ValueDate { get; set; }
            public string? TransactionDate { get; set; }
            public string? Description { get; set; }

            public JVTicketList(string lineNo, string branchCode, string currency, string accGL, string serialNO, string cif, string cvAmount, string fcAmount, string exchangeRate, string jvDescription, string transactionTypeCode, string jvTypeNumber, string valueDate, string transactionDate, string description)
            {
                LineNo = lineNo;
                BranchCode = branchCode;
                Currency = currency;
                AccGL = accGL;
                SerialNo = serialNO;
                Cif = cif;
                CVAmount = cvAmount;
                FCAmount = fcAmount;
                ExchangeRate = exchangeRate;
                JVDescription = jvDescription;
                TransactionTypeCode = transactionTypeCode;
                JVTypeNumber = jvTypeNumber;
                ValueDate = valueDate;
                TransactionDate = transactionDate;
                Description = description;
            }
        }
        public class JVTicketRequest
        {
            public List<JVTicketList> JVTicketLists { get; set; }
            public string? UserID { get; set; }
            public string? Password { get; set; }
            public string? ChannelName { get; set; }

            public JVTicketRequest()
            {
                JVTicketLists = new List<JVTicketList>();
            }
        }
        public class JVTicketResponse
        {
            public string? AccGL { get; set; }
            public string? Branch { get; set; }
            public string? Cif { get; set; }
            public string? Currency { get; set; }
            public string? SerialNo { get; set; }
            public string? BatchRef { get; set; }
            public string? CVAmount { get; set; }
            public string? Description { get; set; }
            public string? JVTypeNo { get; set; }
            public string? LineNo { get; set; }
            public string? Processed { get; set; }
            public string? InnerStatusCode { get; set; }
            public string? InnerStatusDesc { get; set; }
            public string? TransactionCode { get; set; }
            public string? TransactionDate { get; set; }
            public string? TransactionTypeCode { get; set; }
            public string? ValueDate { get; set; }
            public string? StatusCode { get; set; }
            public string? StatusDesc { get; set; }
        }
    }
}
