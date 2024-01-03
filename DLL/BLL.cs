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
            string? soapResult = string.Empty;
            string? StatusDesc = string.Empty;
            string? StatusCode = string.Empty;
            string? transactionNum = string.Empty;
            string? branchcode = string.Empty;
            List<ReqReverseTransaction> logrequest1 = new List<ReqReverseTransaction>();
            List<ReverseTransactionResponse> logresponse1 = new List<ReverseTransactionResponse>();
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
                                string? branchCode = elembranchCode[0]?.InnerXml;

                                logresponse1.Add(new ReverseTransactionResponse
                                {
                                    transactionNumber = transactionNum,
                                    branchCode = branchCode,
                                    IMALRequestID = RequestID,
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
                                logresponse1.Add(new ReverseTransactionResponse
                                {
                                    branchCode = BrancheCode,
                                    transactionNumber = TransactionNumber,
                                    IMALRequestID = RequestID,
                                    StatusCode = StatusCode,
                                    StatusDesc = StatusDesc,


                                });
                            }


                        }

                    }

                }
                else
                {
                    logresponse1.Add(new ReverseTransactionResponse
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
                logresponse1.Add(new ReverseTransactionResponse
                {


                    StatusCode = "-999",
                    StatusDesc = "Techical Error " + "\n" + ex.Message + "\n" + ex.InnerException,

                });

            }
            return JsonConvert.SerializeObject(logresponse1, Newtonsoft.Json.Formatting.Indented);

        }
        public string IMALCreateJVTicket(SIMALJVTicket.JVTicketRequest ListRequest, string UserID, string Password, string ChannelName)
        {
            string accGl = string.Empty;
            string branch = string.Empty;
            string cif = string.Empty;
            string currency = string.Empty;
            string serialNo = string.Empty;
            string batchRef = string.Empty;
            string cvAmount = string.Empty;
            string description = string.Empty;
            string jvTypeNumber = string.Empty;
            string lineNo = string.Empty;
            string processed = string.Empty;
            string innerStatusCode = string.Empty;
            string innerStatusDesc = string.Empty;
            string transactionCode = string.Empty;
            string transactionDate = string.Empty;
            string transactionTypeCode = string.Empty;
            string valueDate = string.Empty;
            string statusLog = string.Empty;
            string statusCode = string.Empty;
            string statusDesc = string.Empty;
            string soapResult = string.Empty;
            List<SIMALJVTicket.JVTicketRequest> jvTicketRequests = new List<SIMALJVTicket.JVTicketRequest>();
            List<SIMALJVTicket.JVTicketResponse> jVTicketResponses = new List<SIMALJVTicket.JVTicketResponse>();
            String requestID = "MW-JVCreate" + UserID + "-" + DateTime.Now.ToString("ddMMyyyyHHmmssff");
            string requestTimeStamp = System.DateTime.Now.ToString("yyyy-MM-dd" + "T" + "HH:mm:ss");

            try
            {
                List<SIMALJVTicket.JVTicketList> jvTicketLists = ListRequest.JVTicketLists;

                jvTicketRequests.Add(new SIMALJVTicket.JVTicketRequest
                {
                    JVTicketLists = jvTicketLists,
                    UserID = UserID,
                    Password = "******",
                    ChannelName = ChannelName,
                });

                string clientRequest = JsonConvert.SerializeObject(jvTicketRequests, Newtonsoft.Json.Formatting.Indented);
                DalCode.InsertLog("CreateJvTicket", Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")), clientRequest, "Pending", ChannelName, requestID);
                string status = CheckChannel(ChannelName, UserID, "Statment");
                if (status == "Enabled")
                {
                    HttpWebRequest request = HTTPS.CreateJVTicketClient();
                    XmlDocument soapEnvelopeXml = new XmlDocument();
                    soapEnvelopeXml.LoadXml(@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:jour=""journalVoucharWs"">
   <soapenv:Header/>
   <soapenv:Body>
      <jour:createJvTicket>
         <serviceContext>
            <businessArea>Accounting</businessArea>
            <businessDomain>transactions</businessDomain>
            <operationName>createJvTicket</operationName>
            <serviceDomain>JournalVouchar</serviceDomain>
            <serviceID>6701</serviceID>
            <version>1.0</version>
         </serviceContext>
         <companyCode>1</companyCode>
         <branchCode>5599</branchCode>
         <approveCreatedJV>1</approveCreatedJV>
         <jvTicketList>
                     </jvTicketList>
         <useDate>1</useDate>
         <requestContext>
             <requestID>" + requestID + @"</requestID>
            <coreRequestTimeStamp>" + requestTimeStamp + @"</coreRequestTimeStamp> 
         </requestContext>
         <requesterContext>
            <channelID>1</channelID>
            <hashKey>1</hashKey>
            <langId>EN</langId>
            <password>" + Password + @"</password>
            <requesterTimeStamp>" + requestTimeStamp + @"</requesterTimeStamp>
            <userID>" + UserID + @"</userID>
         </requesterContext>
         <vendorContext>
            <license>Copyright 2018 Path Solutions. All Rights Reserved</license>
            <providerCompanyName>Path Solutions</providerCompanyName>
            <providerID>IMAL</providerID>
         </vendorContext>
      </jour:createJvTicket>
   </soapenv:Body>
</soapenv:Envelope>");
                    XmlElement jvTicketListElements = soapEnvelopeXml.DocumentElement;
                    XmlNode parentNode = soapEnvelopeXml.SelectSingleNode("//jvTicketList");
                    foreach (var jvTicketDetailedItems in jvTicketLists)
                    {
                        XmlElement jvTicketElement = soapEnvelopeXml.CreateElement("jvTicket");

                        XmlElement lineNoElement = soapEnvelopeXml.CreateElement("lineNumber");
                        lineNoElement.InnerText = jvTicketDetailedItems.LineNo;
                        jvTicketElement.AppendChild(lineNoElement);

                        XmlElement accountListElement = soapEnvelopeXml.CreateElement("account");

                        XmlElement branchCodeElement = soapEnvelopeXml.CreateElement("branch");
                        branchCodeElement.InnerText = jvTicketDetailedItems.BranchCode;
                        accountListElement.AppendChild(branchCodeElement);

                        XmlElement currencyElement = soapEnvelopeXml.CreateElement("currency");
                        currencyElement.InnerText = jvTicketDetailedItems.Currency;
                        accountListElement.AppendChild(currencyElement);

                        XmlElement accGlElement = soapEnvelopeXml.CreateElement("accGl");
                        accGlElement.InnerText = jvTicketDetailedItems.AccGL;
                        accountListElement.AppendChild(accGlElement);

                        XmlElement serialNoElement = soapEnvelopeXml.CreateElement("serialNo");
                        serialNoElement.InnerText = jvTicketDetailedItems.SerialNo;
                        accountListElement.AppendChild(serialNoElement);

                        XmlElement cifEelement = soapEnvelopeXml.CreateElement("cif");
                        cifEelement.InnerText = jvTicketDetailedItems.Cif;
                        accountListElement.AppendChild(cifEelement);


                        jvTicketElement.AppendChild(accountListElement);

                        if (jvTicketDetailedItems.Currency == "818")
                        {

                            XmlElement cvAmountElement = soapEnvelopeXml.CreateElement("cvAmount");
                            cvAmountElement.InnerText = jvTicketDetailedItems.CVAmount;
                            jvTicketElement.AppendChild(cvAmountElement);
                        }
                        else
                        {
                            XmlElement cvAmountElement = soapEnvelopeXml.CreateElement("cvAmount");
                            cvAmountElement.InnerText = jvTicketDetailedItems.CVAmount;
                            jvTicketElement.AppendChild(cvAmountElement);

                            XmlElement fcAmountElement = soapEnvelopeXml.CreateElement("fcAmount");
                            fcAmountElement.InnerText = jvTicketDetailedItems.FCAmount;
                            jvTicketElement.AppendChild(fcAmountElement);

                            XmlElement exchangeElement = soapEnvelopeXml.CreateElement("exchangeRate");
                            exchangeElement.InnerText = jvTicketDetailedItems.ExchangeRate;
                            jvTicketElement.AppendChild(exchangeElement);

                        }
                        XmlElement jvDescriptionElement = soapEnvelopeXml.CreateElement("jvDescription");
                        jvDescriptionElement.InnerText = jvTicketDetailedItems.JVDescription;
                        jvTicketElement.AppendChild(jvDescriptionElement);

                        XmlElement transactionTypeCodeElement = soapEnvelopeXml.CreateElement("transactionTypeCode");
                        transactionTypeCodeElement.InnerText = jvTicketDetailedItems.TransactionTypeCode;
                        jvTicketElement.AppendChild(transactionTypeCodeElement);

                        XmlElement jvTypeNoElement = soapEnvelopeXml.CreateElement("jvTypeNumber");
                        jvTypeNoElement.InnerText = jvTicketDetailedItems.JVTypeNumber;
                        jvTicketElement.AppendChild(jvTypeNoElement);

                        XmlElement valueDateElement = soapEnvelopeXml.CreateElement("valueDate");
                        valueDateElement.InnerText = jvTicketDetailedItems.ValueDate;
                        jvTicketElement.AppendChild(valueDateElement);

                        XmlElement transactionDateElement = soapEnvelopeXml.CreateElement("transactionDate");
                        transactionDateElement.InnerText = jvTicketDetailedItems.TransactionDate;
                        jvTicketElement.AppendChild(valueDateElement);

                        XmlElement descriptionElement = soapEnvelopeXml.CreateElement("description");
                        descriptionElement.InnerText = jvTicketDetailedItems.Description;
                        jvTicketElement.AppendChild(descriptionElement);

                        parentNode.AppendChild(jvTicketElement);
                    }

                    using (Stream stream = request.GetRequestStream())
                    {
                        soapEnvelopeXml.Save(stream);
                    }

                    //Console.WriteLine(soapEnvelopeXml.ToString());

                    using (WebResponse response = request.GetResponse())
                    {
                        using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                        {
                            soapResult = rd.ReadToEnd();
                            var str = XElement.Parse(soapResult);
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(soapResult);
                            XmlNodeList elemStatusCode = xmlDoc.GetElementsByTagName("statusCode");
                            statusCode = elemStatusCode[0]?.InnerXml;
                            XmlNodeList elemStatusDesc = xmlDoc.GetElementsByTagName("statusDesc");
                            statusDesc = elemStatusCode[0]?.InnerXml;
                            if (statusCode == "0")
                            {
                                XmlNodeList elemJVTicketListResponse = xmlDoc.GetElementsByTagName("jvTicketResponse");
                                foreach (XmlNode jvTicketListResponses in elemJVTicketListResponse)
                                {
                                    accGl = jvTicketListResponses.SelectSingleNode("account/accGl")?.InnerXml;
                                    branch = jvTicketListResponses.SelectSingleNode("account/branch")?.InnerXml;
                                    cif = jvTicketListResponses.SelectSingleNode("account/cif")?.InnerXml;
                                    currency = jvTicketListResponses.SelectSingleNode("account/currency")?.InnerXml;
                                    serialNo = jvTicketListResponses.SelectSingleNode("account/serialNo")?.InnerXml;
                                    batchRef = jvTicketListResponses.SelectSingleNode("batchReference")?.InnerXml;
                                    cvAmount = jvTicketListResponses.SelectSingleNode("cvAmount")?.InnerXml;
                                    description = jvTicketListResponses.SelectSingleNode("description")?.InnerXml;
                                    jvTypeNumber = jvTicketListResponses.SelectSingleNode("jvTypeNumber")?.InnerXml;
                                    lineNo = jvTicketListResponses.SelectSingleNode("lineNumber")?.InnerXml;
                                    processed = jvTicketListResponses.SelectSingleNode("lineNumber")?.InnerXml;
                                    innerStatusCode = jvTicketListResponses.SelectSingleNode("statusCode")?.InnerXml;
                                    innerStatusDesc = jvTicketListResponses.SelectSingleNode("statusDesc")?.InnerXml;
                                    transactionCode = jvTicketListResponses.SelectSingleNode("transactionCode")?.InnerXml;
                                    transactionDate = jvTicketListResponses.SelectSingleNode("transactionDate")?.InnerXml;
                                    transactionTypeCode = jvTicketListResponses.SelectSingleNode("transactionTypeCode")?.InnerXml;
                                    valueDate = jvTicketListResponses.SelectSingleNode("valueDate")?.InnerXml;

                                    jVTicketResponses.Add(new SIMALJVTicket.JVTicketResponse
                                    {
                                        AccGL = accGl,
                                        Branch = branch,
                                        Cif = cif,
                                        Currency = currency,
                                        SerialNo = serialNo,
                                        BatchRef = batchRef,
                                        CVAmount = cvAmount,
                                        Description = description,
                                        JVTypeNo = jvTypeNumber,
                                        LineNo = lineNo,
                                        Processed = processed,
                                        InnerStatusCode = innerStatusCode,
                                        InnerStatusDesc = innerStatusDesc,
                                        TransactionCode = transactionCode,
                                        TransactionDate = transactionDate,
                                        TransactionTypeCode = transactionTypeCode,
                                        ValueDate = valueDate,
                                        StatusCode = statusCode,
                                        StatusDesc = statusDesc,

                                    });


                                }

                            }
                            else
                            {
                                if (statusCode != null)
                                {
                                    XmlNodeList elemJVTicketListResponse = xmlDoc.GetElementsByTagName("jvTicketResponse");
                                    foreach (XmlNode jvTicketListResponses in elemJVTicketListResponse)
                                    {
                                        innerStatusCode = jvTicketListResponses.SelectSingleNode("statusCode")?.InnerXml;
                                        innerStatusDesc = jvTicketListResponses.SelectSingleNode("statusDesc")?.InnerXml;
                                        jVTicketResponses.Add(new SIMALJVTicket.JVTicketResponse
                                        {
                                            InnerStatusCode = innerStatusCode,
                                            InnerStatusDesc = innerStatusDesc,
                                        });


                                    }
                                }
                                else
                                {
                                    XmlNodeList elemErrorCode = xmlDoc.GetElementsByTagName("errorCode");
                                    string errorCode = elemErrorCode[0]?.InnerXml;
                                    XmlNodeList elemErrorDesc = xmlDoc.GetElementsByTagName("errorDesc");
                                    string errorDesc = elemErrorCode[0]?.InnerXml;
                                    jVTicketResponses.Add(new SIMALJVTicket.JVTicketResponse
                                    {
                                        StatusCode = errorCode,
                                        StatusDesc = errorDesc,
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    jVTicketResponses.Add(new SIMALJVTicket.JVTicketResponse
                    {
                        StatusCode = "-998",
                        StatusDesc = "Channel Not Authorized",
                    });
                }
                if (statusCode == "0")
                {
                    statusLog = "Success";
                }
                else
                {
                    statusLog = "Failed";
                }

                DalCode.UpdateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), clientRequest, statusLog, ChannelName, requestID);
            }
            catch (Exception ex)
            {
                jVTicketResponses.Add(new SIMALJVTicket.JVTicketResponse
                {
                    StatusCode = " - 999",
                    StatusDesc = "Techical Error " + "\n" + ex.Message + "\n" + ex.InnerException,
                });
            }
            return JsonConvert.SerializeObject(jVTicketResponses, Newtonsoft.Json.Formatting.Indented);
        }


        public string CreatTrx(string TransactionType, string ToAdditionalRef, string fromAdditionalRef, string TransactionPurpose, string TransactionAmount, string Currency, string TransactionDate,
            string valueDate, string userID, string password, string ChannelName,string TransferDesc)
        {
            string? soapResult = string.Empty;
            string? StatusDesc = string.Empty;
            string? StatusCode = string.Empty;
            List<ReqCreateTransfer> logrequest = new List<ReqCreateTransfer>();
            List<CreateTransferResponse> logresponse = new List<CreateTransferResponse>();
            string? RequestID = "MW-CTRX-" + TransactionType + "-" + DateTime.Now.ToString("ddMMyyyyHHmmssff");
            string? requesterTimeStamp = System.DateTime.Now.ToString("yyyy-MM-dd" + "T" + "HH:mm:ss");
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
                          string? transactionNumber = elemtransactionNumber[0]?.InnerXml;

                                XmlNodeList elembranchCode = xmlDoc.GetElementsByTagName("branchCode");
                                string? branchCode = elembranchCode[0]?.InnerXml;

                                logresponse.Add(new CreateTransferResponse
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
                                logresponse.Add(new CreateTransferResponse
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
                    logresponse.Add(new CreateTransferResponse
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
                logresponse.Add(new CreateTransferResponse
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
            string? soapResult = string.Empty;
            string? StatusDesc = string.Empty;
            string? StatusCode = string.Empty;
            List<ReqCreateTransfer> logrequest = new List<ReqCreateTransfer>();
            List<CreateTransferResponse> logresponse = new List<CreateTransferResponse>();
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
                                string? transactionNumber = elemtransactionNumber[0]?.InnerXml;

                                XmlNodeList elembranchCode = xmlDoc.GetElementsByTagName("branchCode");
                                string? branchCode = elembranchCode[0]?.InnerXml;

                                logresponse.Add(new CreateTransferResponse
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
                                logresponse.Add(new CreateTransferResponse
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
                    logresponse.Add(new CreateTransferResponse
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
                logresponse.Add(new CreateTransferResponse
                {


                    StatusCode = "-999",
                    StatusDesc = "Techical Error " + "\n" + ex.Message + "\n" + ex.InnerException,

                });

            }
            return JsonConvert.SerializeObject(logresponse, Newtonsoft.Json.Formatting.Indented);

        }


        public string ChequeTransaction(string transactionType, string CreditAdditionalRef, string DebitAdditionalRef, string transactionAmount, string currencyIso, string chequeNumber, string chequeDate,  string valueDate, string UserID, string Password, string ChannelName)
        {

            string? soapResult = string.Empty;
            string? statusDesc = string.Empty;
            string? statusCode = string.Empty;
            string? TRXno = string.Empty;
            string? BranchNo = string.Empty;
            
            List<ChequeReq> logrequest = new List<ChequeReq>();
            List<ChequeTRXResponse> logresponse = new List<ChequeTRXResponse>();
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
                                logresponse.Add(new ChequeTRXResponse
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
                                    logresponse.Add(new ChequeTRXResponse
                                    {
                                        statusCode = statusCode,
                                        statusDesc = statusDesc,

                                    });
                                }
                                else
                                {
                                    XmlNodeList eerrorCode = xmlDoc.GetElementsByTagName("errorCode");
                                    string? errorCode = eerrorCode[0]?.InnerXml;
                                    XmlNodeList eerrorDesc = xmlDoc.GetElementsByTagName("errorDesc");
                                    string? errorDesc = eerrorDesc[0]?.InnerXml;
                                    logresponse.Add(new ChequeTRXResponse
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
                    logresponse.Add(new ChequeTRXResponse
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
                logresponse.Add(new ChequeTRXResponse
                {


                    statusCode = "-999",
                    statusDesc = "Techical Error " + "\n" + ex.Message + "\n" + ex.InnerException,

                });

            }
            return JsonConvert.SerializeObject(logresponse, Newtonsoft.Json.Formatting.Indented);
        }
    
}
}