﻿using System.Net;

namespace IMAL_FIN_TRX.DLL
{
    public class HTTPS
    {
        public static HttpWebRequest CreateWebRequestTransfer()
        {
            var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var CreateTransfereUrl = MyConfig.GetValue<string>("AppSettings:CreateTransfer");
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(CreateTransfereUrl);
            webRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            webRequest.KeepAlive = false;
            return webRequest;
        }

        public static HttpWebRequest CreateChequeTransaction()
        {
            var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var ChequeTransaction = MyConfig.GetValue<string>("AppSettings:ChequeTransaction");
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(ChequeTransaction);
            webRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            webRequest.KeepAlive= false;
            return webRequest;
        }
        public static HttpWebRequest CreateJVTicketClient()
        {
            var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var createJVTicketURL = MyConfig.GetValue<string>("AppSettings:CreateJVTicket");
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(createJVTicketURL);
            webRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            webRequest.KeepAlive = false;
            return webRequest;
        }
        public static HttpWebRequest CreateWebReverseTransaction()
        {
            var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var CreateTransfereUrl = MyConfig.GetValue<string>("AppSettings:ReverseTransaction");
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(CreateTransfereUrl);
            webRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            webRequest.KeepAlive = false;
            return webRequest;
        }
    }
}
