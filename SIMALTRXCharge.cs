﻿namespace IMAL_FIN_TRX
{
    public class IMALTRXChargeRequest
    {
        public string TransactionType { get; set; }
        public string ToAdditionalRef { get; set; }
        public string fromAdditionalRef { get; set; }
        public string TransactionPurpose { get; set; }
        public string TransactionAmount { get; set; }
        public string Currency { get; set; }
        public string TransactionDate { get; set; }
        public string ValueDate { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
        public string ChannelName { get; set; }

        public string TransferDesc { get; set; }

        public string ChargeCode1 { get; set; }

        public string ChargeCodeAmount1 { get; set; }

        public string ChargeCode2 { get; set; }

        public string ChargeCodeAmount2 { get; set; }


    }
}
