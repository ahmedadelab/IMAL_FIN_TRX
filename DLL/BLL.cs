using System.Data;
using static System.Net.WebRequestMethods;
using System.Net;
using System.Xml.Linq;
using System.Xml;
using Newtonsoft.Json;
using static IMAL_FIN_TRX.DLL.PS;
using Microsoft.AspNetCore.Http;

namespace IMAL_FIN_TRX.DLL
{
    public class BLL
    {
        DAL DalCode = new DAL();
        public string CheckChannel(String ChannelName, string username, string ServiceName)
        {
            string ChannelIP = "";
            string statusChannel = "";
            string EnableChannel = "";
            DataTable dt_Channel = DalCode.IMALChannelstatus(ChannelName, username, ChannelIP, ServiceName);
            BLL[] BR_Channel = new BLL[dt_Channel.Rows.Count];
            if (BR_Channel.Length != 0)
            {
                int ii;
                for (ii = 0; ii < dt_Channel.Rows.Count; ii++)
                {

                    EnableChannel = dt_Channel.Rows[ii]["EnableChannel"].ToString().Trim();
                }
            }
            if (EnableChannel == "1")
            {
                statusChannel = "Enabled";
            }
            else
            {
                statusChannel = "Disabled";
            }
            return statusChannel;
        }

        public string ReverseTrx(string BrancheCode, string TransactionNumber, string reason, string userID, string password, string ChannelName)
        {
            string soapResult = string.Empty;
            string StatusDesc = string.Empty;
            string StatusCode = string.Empty;
            string transactionNum = string.Empty;
            string branchcode = string.Empty;
            List<ReqReverseTransaction> logrequest1 = new List<ReqReverseTransaction>();
            List<RespReverseTransaction> logresponse1 = new List<RespReverseTransaction>();
            string RequestID = "MW-RVRX-" + TransactionNumber+"-"+"-"+ BrancheCode + DateTime.Now.ToString("ddMMyyyyHHmmssff");
            string requesterTimeStamp = System.DateTime.Now.ToString("yyyy-MM-dd" + "T" + "HH:mm:ss");
            try
            {
                logrequest1.Add(new ReqReverseTransaction
                {
                    branchCode = BrancheCode,
                    transactionNumber = TransactionNumber,
                    reason = reason,
                    UserID = userID,
                    Password = "******",
                    ChannelName = ChannelName
                });
                string ClientRequest = JsonConvert.SerializeObject(logrequest1, Newtonsoft.Json.Formatting.Indented);
                DalCode.InsertLog("TRXReverse", Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")), ClientRequest, "Pending", ChannelName, RequestID);
                string status = CheckChannel(ChannelName, userID, "TRXReverse");
                if (status == "Enabled")
                {
                    HttpWebRequest request = HTTPS.CreateWebReverseTransaction();
                    XmlDocument soapEnvelopeXml = new XmlDocument();
                    soapEnvelopeXml.LoadXml(@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tran=""transactionWs"">
   <soapenv:Header/>
   <soapenv:Body>
      <tran:approveReverseTransaction>
         <serviceContext>
            <businessArea>Retail</businessArea>
            <businessDomain>PaymentsOperationsManagement</businessDomain>
            <operationName>approveReverseTransaction</operationName>
            <serviceDomain>Transaction</serviceDomain>
            <serviceID>5807</serviceID>
            <version>1.0</version>
         </serviceContext>
         <companyCode>1</companyCode>
         <branchCode>" + BrancheCode + @"</branchCode>
         <transactionNumber>" + TransactionNumber + @"</transactionNumber>
         <reason>" + reason + @"</reason>
         <requestContext>
            <coreRequestTimeStamp>" + requesterTimeStamp + @"</coreRequestTimeStamp>
            <requestID>" + RequestID + @"</requestID>
         </requestContext>
         <requesterContext>
           <channelID>1</channelID>
           <hashKey>1</hashKey>
           <langId>EN</langId>
           <password>" + password + @"</password>
           <requesterTimeStamp>" + requesterTimeStamp + @"</requesterTimeStamp>
           <userID>" + userID + @"</userID>
         </requesterContext>
         <vendorContext>
            <license>Copyright 2018 Path Solutions. All Rights Reserved</license>
            <providerCompanyName>Path Solutions</providerCompanyName>
            <providerID>IMAL</providerID>
         </vendorContext>
      </tran:approveReverseTransaction>
   </soapenv:Body>
</soapenv:Envelope>
");

                    using (Stream stream = request.GetRequestStream())
                    {
                        soapEnvelopeXml.Save(stream);
                    }


                    using (WebResponse response = request.GetResponse())
                    {

                        using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                        {
                            soapResult = rd.ReadToEnd();
                            //Console.WriteLine(soapResult);
                            var str = XElement.Parse(soapResult);
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(soapResult);
                            XmlNodeList elemStatusCode = xmlDoc.GetElementsByTagName("statusCode");
                            StatusCode = elemStatusCode[0]?.InnerXml;
                            XmlNodeList elemStatusCodeDes = xmlDoc.GetElementsByTagName("statusDesc");
                            StatusDesc = elemStatusCodeDes[0]?.InnerXml;

                            XmlNodeList elemtransactionNumber = xmlDoc.GetElementsByTagName("transactionNumber");

                            transactionNum = elemtransactionNumber[0]?.InnerXml;


                            XmlNodeList elembranchcode = xmlDoc.GetElementsByTagName("branchCode");

                            branchcode = elemtransactionNumber[0]?.InnerXml;
                            if (StatusCode == "0")
                            {

                                elemtransactionNumber = xmlDoc.GetElementsByTagName("transactionNumber");
                                transactionNum = elemtransactionNumber[0]?.InnerXml;

                                XmlNodeList elembranchCode = xmlDoc.GetElementsByTagName("branchCode");
                                string branchCode = elembranchCode[0].InnerXml;

                                logresponse1.Add(new RespReverseTransaction
                                {
                                    transactionNumber = transactionNum,
                                    branchCode = branchCode,
                                    StatusCode = StatusCode,
                                    StatusDesc = StatusDesc,

                                });
                            }
                            else
                            { 
                                if(StatusCode == null)
                                {
                                    XmlNodeList elemerrorCode = xmlDoc.GetElementsByTagName("errorCode");
                                    StatusCode = elemerrorCode[0]?.InnerXml;
                                    XmlNodeList elemerrorDesc = xmlDoc.GetElementsByTagName("errorDesc");
                                    StatusDesc = elemerrorDesc[0]?.InnerXml;
                                }
                                logresponse1.Add(new RespReverseTransaction
                                {
                                    branchCode = BrancheCode,
                                    transactionNumber = TransactionNumber,
                                    StatusCode = StatusCode,
                                    StatusDesc = StatusDesc,


                                });
                            }


                        }

                    }

                }
                else
                {
                    logresponse1.Add(new RespReverseTransaction
                    {


                        StatusCode = "-998",
                        StatusDesc = "Channel Not Authorized",

                    });
                }

                string statuslog = "";
                if (StatusCode == "0")
                {
                    statuslog = "Success";
                }
                else
                {
                    statuslog = "Failed";
                }
                DalCode.UpdateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), JsonConvert.SerializeObject(logresponse1, Newtonsoft.Json.Formatting.Indented), statuslog, ChannelName, RequestID);



            }
            catch (Exception ex)
            {
                logresponse1.Add(new RespReverseTransaction
                {


                    StatusCode = "-999",
                    StatusDesc = "Techical Error " + "\n" + ex.Message + "\n" + ex.InnerException,

                });

            }
            return JsonConvert.SerializeObject(logresponse1, Newtonsoft.Json.Formatting.Indented);

        }


        public string CreatTrx(string TransactionType, string ToAdditionalRef, string fromAdditionalRef, string TransactionPurpose, string TransactionAmount, string Currency, string TransactionDate,
            string valueDate, string userID, string password, string ChannelName,string TransferDesc)
        {
            string soapResult = string.Empty;
            string StatusDesc = string.Empty;
            string StatusCode = string.Empty;
            List<ReqCreateTransfer> logrequest = new List<ReqCreateTransfer>();
            List<RespCreateTransfer> logresponse = new List<RespCreateTransfer>();
            string RequestID = "MW-CTRX-" + TransactionType + "-" + DateTime.Now.ToString("ddMMyyyyHHmmssff");
            string requesterTimeStamp = System.DateTime.Now.ToString("yyyy-MM-dd" + "T" + "HH:mm:ss");
            try
            {
                logrequest.Add(new ReqCreateTransfer
                {
                    TransactionType = TransactionType,
                    ToAdditionalRef = ToAdditionalRef,
                    fromAdditionalRef = fromAdditionalRef,
                    TransactionPurpose = TransactionPurpose,
                    TransactionAmount = TransactionAmount,
                    Currency = Currency,
                    TransactionDate = TransactionDate,
                    ValueDate = valueDate,
                    UserID = userID,
                    Password = "******",
                    ChannelName = ChannelName
                });
                string ClientRequest = JsonConvert.SerializeObject(logrequest, Newtonsoft.Json.Formatting.Indented);
                DalCode.InsertLog("TRXTransfer", Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")), ClientRequest, "Pending", ChannelName, RequestID);
                string status = CheckChannel(ChannelName, userID, "TRXTransfer");
                if (status == "Enabled")
                {
                    HttpWebRequest request = HTTPS.CreateWebRequestTransfer();
                    XmlDocument soapEnvelopeXml = new XmlDocument();
                    soapEnvelopeXml.LoadXml(@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tran=""transferWs"">
   <soapenv:Header/>
   <soapenv:Body>
      <tran:createTransfer>
         <serviceContext>
            <businessArea>Retail</businessArea>
            <businessDomain>PaymentsOperationsManagement</businessDomain>
            <operationName>createTransfer</operationName>
            <serviceDomain>Transfer</serviceDomain>
            <serviceID>4801</serviceID>
            <version>1.0</version>
         </serviceContext>       
         <companyCode>1</companyCode>
         <branchCode>5599</branchCode>
        <transactionType>"+TransactionType+@"</transactionType>
         <fromAccount>
       <additionalRef>" + fromAdditionalRef + @"</additionalRef>
         </fromAccount>         
         <toAccounts>
         		<multiAccount>
         			<account>
         				<additionalRef>" + ToAdditionalRef + @"</additionalRef>
         			</account>
         		</multiAccount>
         </toAccounts>        
        <transactionPurpose>" + TransactionPurpose + @"</transactionPurpose>
         <transactionAmount>" + TransactionAmount + @"</transactionAmount>
         <currencyIso>" + Currency + @"</currencyIso> 
         <transactionDate>" + TransactionDate + @"</transactionDate>  
         <dofDescription>"+TransferDesc +@"</dofDescription>
         <valueDate>" + valueDate + @"</valueDate>   
         <transactionStatus>1</transactionStatus>
         <useDate>0</useDate>  
         <useAccount>1</useAccount> 
        <requestContext>
        <coreRequestTimeStamp> " + requesterTimeStamp + @"</coreRequestTimeStamp>
         <requestID>" + RequestID + @"</requestID>
         </requestContext>         
         <requesterContext>
         		<channelID>1</channelID>
         		<hashKey>1</hashKey>
         		<langId>EN</langId>
         		<password>" + password + @"</password>
         		<requesterTimeStamp>" + requesterTimeStamp + @"</requesterTimeStamp>
         		<userID>" + userID + @"</userID>
         </requesterContext>
         <vendorContext>
            <license>Copyright 2018 Path Solutions. All Rights Reserved</license>
            <providerCompanyName>Path Solutions</providerCompanyName>
            <providerID>IMAL</providerID>
         </vendorContext>
      </tran:createTransfer>
   </soapenv:Body>
</soapenv:Envelope>
");

                    using (Stream stream = request.GetRequestStream())
                    {
                        soapEnvelopeXml.Save(stream);
                    }


                    using (WebResponse response = request.GetResponse())
                    {

                        using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                        {
                            soapResult = rd.ReadToEnd();
                            //Console.WriteLine(soapResult);
                            var str = XElement.Parse(soapResult);
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(soapResult);
                            XmlNodeList elemStatusCode = xmlDoc.GetElementsByTagName("statusCode");
                            StatusCode = elemStatusCode[0]?.InnerXml;
                            XmlNodeList elemStatusCodeDes = xmlDoc.GetElementsByTagName("statusDesc");
                            StatusDesc = elemStatusCodeDes[0]?.InnerXml;


                            if (StatusCode == "0")
                            {

                                XmlNodeList elemtransactionNumber = xmlDoc.GetElementsByTagName("transactionNumber");
                          string transactionNumber = elemtransactionNumber[0].InnerXml;

                                XmlNodeList elembranchCode = xmlDoc.GetElementsByTagName("branchCode");
                                string branchCode = elembranchCode[0].InnerXml;

                                logresponse.Add(new RespCreateTransfer
                                {
                                    transactionNumber = transactionNumber,
                                    branchCode = branchCode,
                                    StatusCode = StatusCode,
                                    StatusDesc = StatusDesc,
                                    IMALRequestID = RequestID

                                }) ;
                            }
                            else
                            {
                                if(StatusCode == null)
                                {
                                    XmlNodeList elemerrorCode = xmlDoc.GetElementsByTagName("errorCode");
                                    StatusCode = elemerrorCode[0]?.InnerXml;
                                    XmlNodeList elemerrorDesc = xmlDoc.GetElementsByTagName("errorDesc");
                                    StatusDesc = elemerrorDesc[0]?.InnerXml;
                                }
                                logresponse.Add(new RespCreateTransfer
                                {


                                    StatusCode = StatusCode,
                                    StatusDesc = StatusDesc,
                                    IMALRequestID = RequestID

                                });
                            }


                        }

                    }

                }
                else
                {
                    logresponse.Add(new RespCreateTransfer
                    {


                        StatusCode = "-998",
                        StatusDesc = "Channel Not Authorized",

                    });
                }

                string statuslog = "";
                if (StatusCode == "0")
                {
                    statuslog = "Success";
                }
                else
                {
                    statuslog = "Failed";
                }
                DalCode.UpdateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), JsonConvert.SerializeObject(logresponse, Newtonsoft.Json.Formatting.Indented), statuslog, ChannelName, RequestID);



            }
            catch (Exception ex)
            {
                logresponse.Add(new RespCreateTransfer
                {


                    StatusCode = "-999",
                    StatusDesc = "Techical Error "+"\n"+ex.Message+"\n"+ex.InnerException,

                });

            }
            return JsonConvert.SerializeObject(logresponse, Newtonsoft.Json.Formatting.Indented);

        }

        public string CreatTrxCharge(string TransactionType, string ToAdditionalRef, string fromAdditionalRef, string TransactionPurpose, string TransactionAmount, string Currency, string TransactionDate,
          string valueDate, string userID, string password, string ChannelName, string TransferDesc,string ChargeCode1,string ChargeCodeAmount1,string ChargeCode2,string ChargeCodeAmount2)
        {
            string soapResult = string.Empty;
            string StatusDesc = string.Empty;
            string StatusCode = string.Empty;
            List<ReqCreateTransfer> logrequest = new List<ReqCreateTransfer>();
            List<RespCreateTransfer> logresponse = new List<RespCreateTransfer>();
            string RequestID = "MW-CTRXCharge-" + TransactionType + "-" + DateTime.Now.ToString("ddMMyyyyHHmmssff");
            string requesterTimeStamp = System.DateTime.Now.ToString("yyyy-MM-dd" + "T" + "HH:mm:ss");
            try
            {
                logrequest.Add(new ReqCreateTransfer
                {
                    TransactionType = TransactionType,
                    ToAdditionalRef = ToAdditionalRef,
                    fromAdditionalRef = fromAdditionalRef,
                    TransactionPurpose = TransactionPurpose,
                    TransactionAmount = TransactionAmount,
                    Currency = Currency,
                    ChargeCode1 = ChargeCode1,
                    ChargeCodeAmount1 = ChargeCodeAmount1,
                    ChargeCode2 = ChargeCode2,
                    ChargeCodeAmount2 = ChargeCodeAmount2,
                    TransactionDate = TransactionDate,
                    ValueDate = valueDate,
                    UserID = userID,
                    Password = "******",
                    ChannelName = ChannelName
                });
                string ClientRequest = JsonConvert.SerializeObject(logrequest, Newtonsoft.Json.Formatting.Indented);
                DalCode.InsertLog("TRXTransferCharge", Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")), ClientRequest, "Pending", ChannelName, RequestID);
                string status = CheckChannel(ChannelName, userID, "TRXTransferCharge");
                if (status == "Enabled")
                {
                    HttpWebRequest request = HTTPS.CreateWebRequestTransfer();
                    XmlDocument soapEnvelopeXml = new XmlDocument();
                    soapEnvelopeXml.LoadXml(@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tran=""transferWs"">
   <soapenv:Header/>
   <soapenv:Body>
      <tran:createTransfer>
           <serviceContext>
            <businessArea>Retail</businessArea>
            <businessDomain>PaymentsOperationsManagement</businessDomain>
            <operationName>createTransfer</operationName>
            <serviceDomain>Transfer</serviceDomain>
            <serviceID>4801</serviceID>
            <version>1.0</version>
         </serviceContext>      
         <companyCode>1</companyCode>
         <branchCode>5599</branchCode>
         <transactionType>"+ TransactionType +@"</transactionType>
         <fromAccount>
       <additionalRef>" + fromAdditionalRef + @"</additionalRef>
         </fromAccount>         
         <toAccounts>
         		<multiAccount>
         			<account>
         				<additionalRef>" + ToAdditionalRef + @"</additionalRef>
         			</account>
         		</multiAccount>
         </toAccounts>   
         <chargesList>
           <charges>
            <chargeCode>" + ChargeCode1 +@"</chargeCode>
            <newAmount>" + ChargeCodeAmount1 + @"</newAmount>
            </charges>                
          <charges>
             <chargeCode>" + ChargeCode2 +@"</chargeCode>
            <newAmount>" + ChargeCodeAmount2 + @"</newAmount>
           </charges> 
         </chargesList>
         <transactionPurpose>" + TransactionPurpose + @"</transactionPurpose>
         <transactionAmount>" + TransactionAmount + @"</transactionAmount>
         <currencyIso>" + Currency + @"</currencyIso> 
         <transactionDate>" + TransactionDate + @"</transactionDate>  
         <dofDescription>" + TransferDesc + @"</dofDescription>
         <valueDate>" + valueDate + @"</valueDate>   
         <transactionStatus>1</transactionStatus>
         <useDate>0</useDate>          
         <useAccount>1</useAccount> 
        <requestContext>
        <coreRequestTimeStamp> " + requesterTimeStamp + @"</coreRequestTimeStamp>
         <requestID>" + RequestID + @"</requestID>
         </requestContext>         
         <requesterContext>
         		<channelID>1</channelID>
         		<hashKey>1</hashKey>
         		<langId>EN</langId>
         		<password>" + password + @"</password>
         		<requesterTimeStamp>" + requesterTimeStamp + @"</requesterTimeStamp>
         		<userID>" + userID + @"</userID>
         </requesterContext>
         <vendorContext>
            <license>Copyright 2018 Path Solutions. All Rights Reserved</license>
            <providerCompanyName>Path Solutions</providerCompanyName>
            <providerID>IMAL</providerID>
         </vendorContext>
      </tran:createTransfer>
   </soapenv:Body>
</soapenv:Envelope>
");

                    using (Stream stream = request.GetRequestStream())
                    {
                        soapEnvelopeXml.Save(stream);
                    }


                    using (WebResponse response = request.GetResponse())
                    {

                        using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                        {
                            soapResult = rd.ReadToEnd();
                            //Console.WriteLine(soapResult);
                            var str = XElement.Parse(soapResult);
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(soapResult);
                            XmlNodeList elemStatusCode = xmlDoc.GetElementsByTagName("statusCode");
                            StatusCode = elemStatusCode[0]?.InnerXml;
                            XmlNodeList elemStatusCodeDes = xmlDoc.GetElementsByTagName("statusDesc");
                            StatusDesc = elemStatusCodeDes[0]?.InnerXml;


                            if (StatusCode == "0")
                            {

                                XmlNodeList elemtransactionNumber = xmlDoc.GetElementsByTagName("transactionNumber");
                                string transactionNumber = elemtransactionNumber[0].InnerXml;

                                XmlNodeList elembranchCode = xmlDoc.GetElementsByTagName("branchCode");
                                string branchCode = elembranchCode[0].InnerXml;

                                logresponse.Add(new RespCreateTransfer
                                {
                                    transactionNumber = transactionNumber,
                                    branchCode = branchCode,
                                    StatusCode = StatusCode,
                                    StatusDesc = StatusDesc,
                                    IMALRequestID = RequestID

                                });
                            }
                            else
                            {
                                if (StatusCode == null)
                                {
                                    XmlNodeList elemerrorCode = xmlDoc.GetElementsByTagName("errorCode");
                                    StatusCode = elemerrorCode[0]?.InnerXml;
                                    XmlNodeList elemerrorDesc = xmlDoc.GetElementsByTagName("errorDesc");
                                    StatusDesc = elemerrorDesc[0]?.InnerXml;
                                }
                                logresponse.Add(new RespCreateTransfer
                                {


                                    StatusCode = StatusCode,
                                    StatusDesc = StatusDesc,
                                    IMALRequestID = RequestID

                                });
                            }


                        }

                    }

                }
                else
                {
                    logresponse.Add(new RespCreateTransfer
                    {


                        StatusCode = "-998",
                        StatusDesc = "Channel Not Authorized",

                    });
                }

                string statuslog = "";
                if (StatusCode == "0")
                {
                    statuslog = "Success";
                }
                else
                {
                    statuslog = "Failed";
                }
                DalCode.UpdateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), JsonConvert.SerializeObject(logresponse, Newtonsoft.Json.Formatting.Indented), statuslog, ChannelName, RequestID);



            }
            catch (Exception ex)
            {
                logresponse.Add(new RespCreateTransfer
                {


                    StatusCode = "-999",
                    StatusDesc = "Techical Error " + "\n" + ex.Message + "\n" + ex.InnerException,

                });

            }
            return JsonConvert.SerializeObject(logresponse, Newtonsoft.Json.Formatting.Indented);

        }


        public string ChequeTransaction(string transactionType, string CreditAdditionalRef, string DebitAdditionalRef, string transactionAmount, string currencyIso, string chequeNumber, string chequeDate,  string valueDate, string UserID, string Password, string ChannelName)
        {

            string soapResult = string.Empty;
            string statusDesc = string.Empty;
            string statusCode = string.Empty;
            string TRXno = string.Empty;
            string BranchNo = string.Empty;
            
            List<ChequeReq> logrequest = new List<ChequeReq>();
            List<ChequeRes> logresponse = new List<ChequeRes>();
            string RequestID = "MW-CHEQUETRX-" + transactionType + "-" + DateTime.Now.ToString("ddMMyyyyHHmmssff");
            string requesterTimeStamp = System.DateTime.Now.ToString("yyyy-MM-dd" + "T" + "HH:mm:ss");
            string TransactionDate = System.DateTime.Now.ToString("yyyy-MM-dd");
            try
            {
                logrequest.Add(new ChequeReq
                {
                    transactionType = transactionType,
                    CreditAdditionalRef = CreditAdditionalRef,
                    DebitAdditionalRef = DebitAdditionalRef,
                    transactionAmount = transactionAmount,
                    currencyIso = currencyIso,
                    chequeNumber = chequeNumber,
                    chequeDate = chequeDate,
                    transactionDate =TransactionDate,
                    valueDate = valueDate,                   
                    UserID = UserID,
                    Password = "******",
                    ChannelName = ChannelName,
                });
                string ClientRequest = JsonConvert.SerializeObject(logrequest, Newtonsoft.Json.Formatting.Indented);
                DalCode.InsertLog("CHEQUETransfer", Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")), ClientRequest, "Pending", ChannelName, RequestID);
                string status = CheckChannel(ChannelName, UserID, "CHEQUETransfer");
                if (status == "Enabled")
                {
                    HttpWebRequest request = HTTPS.CreateChequeTransaction();
                    XmlDocument soapEnvelopeXml = new XmlDocument();
                    soapEnvelopeXml.LoadXml(@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:cheq=""chequeTransactionWs"">
   <soapenv:Header/>
   <soapenv:Body>
      <cheq:createChequeDeposit>
         <serviceContext>
            <businessArea>Retail</businessArea>
            <businessDomain>PaymentsOperationsManagement</businessDomain>
            <operationName>createChequeDeposit</operationName>
            <serviceDomain>ChequeTransaction</serviceDomain>
            <serviceID>5101</serviceID>
            <version>1.0</version>
         </serviceContext>
        <companyCode>1</companyCode>
         <branchCode>5599</branchCode>
         <transactionType>"+ transactionType + @"</transactionType>
         <creditAccount>     
            <additionalRef>" + CreditAdditionalRef + @"</additionalRef>
         </creditAccount>         
         <debitAccount>     
            <additionalRef>" + DebitAdditionalRef + @"</additionalRef>
         </debitAccount>         
         <transactionAmount>" + transactionAmount + @"</transactionAmount>
         <currencyIso>" + currencyIso + @"</currencyIso>
         <chequeNumber>" + chequeNumber + @"</chequeNumber>
         <chequeDate>" + chequeDate + @"</chequeDate>        
         <transactionDate>" + TransactionDate + @"</transactionDate>
         <valueDate>" + valueDate + @"</valueDate>
         <transactionStatus>1</transactionStatus>
        <requestContext>
            <coreRequestTimeStamp>" + requesterTimeStamp + @"</coreRequestTimeStamp>  
            <requestID>" + RequestID + @"</requestID>      
         </requestContext>         
         <requesterContext>
            <channelID>1</channelID>
            <hashKey>1</hashKey>
              <langId>EN</langId>
            <password>" + Password + @"</password>
            <requesterTimeStamp>" + requesterTimeStamp + @"</requesterTimeStamp>
            <userID>" + UserID + @"</userID>
         </requesterContext>
         <vendorContext>
            <license>Copyright 2018 Path Solutions. All Rights Reserved</license>
            <providerCompanyName>Path Solutions</providerCompanyName>
            <providerID>IMAL</providerID>
         </vendorContext>
      </cheq:createChequeDeposit>
   </soapenv:Body>
</soapenv:Envelope>
");


                    using (Stream stream = request.GetRequestStream())
                    {
                        soapEnvelopeXml.Save(stream);
                    }
                    using (WebResponse response = request.GetResponse())
                    {

                        using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                        {
                            soapResult = rd.ReadToEnd();
                            //Console.WriteLine(soapResult);
                            var str = XElement.Parse(soapResult);

                            XmlDocument xmlDoc = new XmlDocument();

                            xmlDoc.LoadXml(soapResult);

                            XmlNodeList elemStatusCode = xmlDoc.GetElementsByTagName("statusCode");
                            statusCode = elemStatusCode[0]?.InnerXml;
                            XmlNodeList elemStatusCodeDes = xmlDoc.GetElementsByTagName("statusDesc");
                            statusDesc = elemStatusCodeDes[0]?.InnerXml;


                            if (statusCode == "0")
                            {


                                XmlNodeList elBranchNo = xmlDoc.GetElementsByTagName("BranchNo");
                                BranchNo = elBranchNo[0]?.InnerXml;
                                XmlNodeList elTRXno = xmlDoc.GetElementsByTagName("TRXno");
                                TRXno = elemStatusCodeDes[0]?.InnerXml;
                                logresponse.Add(new ChequeRes
                                {
                                    statusCode = statusCode,
                                    statusDesc = statusDesc,
                                    TRXno = TRXno,
                                    BranchNo = BranchNo
                                });

                            }
                            else
                            {
                                if (statusCode != null)
                                {
                                    logresponse.Add(new ChequeRes
                                    {
                                        statusCode = statusCode,
                                        statusDesc = statusDesc,

                                    });
                                }
                                else
                                {
                                    XmlNodeList eerrorCode = xmlDoc.GetElementsByTagName("errorCode");
                                    string errorCode = eerrorCode[0]?.InnerXml;
                                    XmlNodeList eerrorDesc = xmlDoc.GetElementsByTagName("errorDesc");
                                    string errorDesc = eerrorDesc[0]?.InnerXml;
                                    logresponse.Add(new ChequeRes
                                    {
                                        statusCode = errorCode,
                                        statusDesc = errorDesc,

                                    });
                                }


                            }

                        }
                    }

                }
                else
                {
                    logresponse.Add(new ChequeRes
                    {


                        statusCode = "-998",
                        statusDesc = "Channel Not Authorized",

                    });
                }

                string statuslog = "";
                if (statusCode == "0")
                {
                    statuslog = "Success";
                }
                else
                {
                    statuslog = "Failed";
                }
                DalCode.UpdateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), JsonConvert.SerializeObject(logresponse, Newtonsoft.Json.Formatting.Indented), statuslog, ChannelName, RequestID);



            }
            catch (Exception ex)
            {
                logresponse.Add(new ChequeRes
                {


                    statusCode = "-999",
                    statusDesc = "Techical Error " + "\n" + ex.Message + "\n" + ex.InnerException,

                });

            }
            return JsonConvert.SerializeObject(logresponse, Newtonsoft.Json.Formatting.Indented);
        }
    
}
}