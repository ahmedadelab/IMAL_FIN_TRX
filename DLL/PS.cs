﻿namespace IMAL_FIN_TRX.DLL
{
    public class PS
    {
        public class RespCreateTransfer
        {

            public string? branchCode { get; set; }

            public string? transactionNumber { get; set; }
            public string? StatusCode { get; set; }
            public string? StatusDesc { get; set; }

        }
        public class ReqReverseTransaction
        {
            public string? branchCode { get; set; }
            public string? transactionNumber { get; set; }
            public string? reason { get; set; }
            public string? UserID { get; set; }
            public string? Password { get; set; }
            public string? ChannelName { get; set; }
        }

        public class RespReverseTransaction
        {
            public string? branchCode { get; set; }
            public string? transactionNumber { get; set; }
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

            public string? ChargeCodeAmount2 { get; set; }

            public string? ChargeCode2 { get; set; }
            public string? ChargeCode1 { get; set; }
            public string? ChargeCodeAmount1 { get; set; }
            public string? TransactionDate { get; set; }
            public string? ValueDate { get; set; }
            public string? UserID { get; set; }
            public string? Password { get; set; }
            public string? ChannelName { get; set; }
        }
    }
}
