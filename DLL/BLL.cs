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




        public string CreatTrx(string TransactionType, string ToAdditionalRef, string fromAdditionalRef, string TransactionPurpose, string TransactionAmount, string Currency, string TransactionDate,
            string valueDate, string userID, string password, string ChannelName)
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
         <transactionType>" + TransactionType + @"</transactionType>         
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
         <valueDate>" + valueDate + @"</valueDate>   
         <useDate>0</useDate>          
         <useAccount>1</useAccount> 
        <requestContext>
           <requestID>" + RequestID + @"</requestID>
           <coreRequestTimeStamp>" + requesterTimeStamp + @"</coreRequestTimeStamp>
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
                            StatusCode = elemStatusCode[0].InnerXml;
                            XmlNodeList elemStatusCodeDes = xmlDoc.GetElementsByTagName("statusDesc");
                            StatusDesc = elemStatusCodeDes[0].InnerXml;


                            if (StatusCode == "0")
                            {
                     

                                logresponse.Add(new RespCreateTransfer
                                {
                                
                                    StatusCode = StatusCode,
                                    StatusDesc = StatusDesc,

                                });
                            }
                            else
                            {
                                logresponse.Add(new RespCreateTransfer
                                {


                                    StatusCode = StatusCode,
                                    StatusDesc = StatusDesc,

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
                    StatusDesc = ex.Message,

                });

            }
            return JsonConvert.SerializeObject(logresponse, Newtonsoft.Json.Formatting.Indented);

        }
    }
}
