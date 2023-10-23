using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PhonePeIntegrationApp.Models;
using PhonePeIntegrationApp.Settings;
using System.Net.Http;
using System;
using System.Security.Cryptography;
using System.Text;

namespace PhonePeIntegrationApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhonePeController : ControllerBase
    {
        private PhonePeSettings _phonePeSettings;

        public static List<PaymentResponse> TransactionHistory = new List<PaymentResponse>();

        public PhonePeController(IOptions<PhonePeSettings> configuration)
        {
            _phonePeSettings = configuration.Value;
        }

        [HttpPost]
        [Route("CreatePayment")]
        public async Task<JsonResult> CreatePayment(OrderDetailModel order)
        {
            string transactionId = Guid.NewGuid().ToString();

            var data = new Dictionary<string, object>
            {
                { "merchantId", _phonePeSettings.MerchentId },
                { "merchantTransactionId", transactionId },
                { "merchantUserId", "MUID123" },
                { "amount", Convert.ToString(order.OrderAmount * 100) },
                { "redirectUrl", "http://localhost:4200/payment-status"},
                { "redirectMode", "REDIRECT"},
                { "callbackUrl", $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}/PhonePe/Response" },
                { "mobileNumber", "9999999999" },
                { "paymentInstrument", new Dictionary<string, string> { { "type", "PAY_PAGE" } } }
            };

            //            string payload = @"{
            //  ""merchantId"": ""PGTESTPAYUAT"",
            //  ""merchantTransactionId"": ""MT7850590068188104"",
            //  ""merchantUserId"": ""MUID123"",
            //  ""amount"": 10000,
            //  ""redirectUrl"": ""http://localhost:4200/"",
            //  ""redirectMode"": ""REDIRECT"",
            //  ""callbackUrl"": ""http://localhost:4200/"",
            //  ""mobileNumber"": ""9999999999"",
            //  ""paymentInstrument"": {
            //    ""type"": ""PAY_PAGE""
            //  }
            //}";
            var encode = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
            //string k = "{\r\n  \"merchantId\": \"PGTESTPAYUAT\",\r\n  \"merchantTransactionId\": \"MT7850590068188104\",\r\n  \"merchantUserId\": \"MUID123\",\r\n  \"amount\": 10000,\r\n  \"redirectUrl\": \"http://localhost:4200/\",\r\n  \"redirectMode\": \"REDIRECT\",\r\n  \"callbackUrl\": \"http://localhost:4200/\",\r\n  \"mobileNumber\": \"9999999999\",\r\n  \"paymentInstrument\": {\r\n    \"type\": \"PAY_PAGE\"\r\n  }\r\n}";
           // var encode = Convert.ToBase64String(Encoding.UTF8.GetBytes(k));
            var saltKey = _phonePeSettings.MerchentSecretKey;
            var saltIndex = _phonePeSettings.SaltIndex;
            var stringToHash = encode + _phonePeSettings.ApiEndpoint + saltKey;

            var sha256 = ComputeSHA256(stringToHash);
            var finalXHeader = sha256 + "###" + saltIndex;

            using (var client = new HttpClient())
            {
                //client.DefaultRequestHeaders.Add("Content-Type", "application/json");

                //- working
                // client.DefaultRequestHeaders.Add("X-VERIFY", "d8026bc248c1ce642e67894b6e9bcef79e3996fbaf70578edbc2da7db835072f###1");

               // var requestData = new Dictionary<string, string> { { "request", "ewogICJtZXJjaGFudElkIjogIlBHVEVTVFBBWVVBVCIsCiAgIm1lcmNoYW50VHJhbnNhY3Rpb25JZCI6ICJNVDc4NTA1OTAwNjgxODgxMDQiLAogICJtZXJjaGFudFVzZXJJZCI6ICJNVUlEMTIzIiwKICAiYW1vdW50IjogMTAwMDAsCiAgInJlZGlyZWN0VXJsIjogImh0dHA6Ly9sb2NhbGhvc3Q6NDIwMC8iLAogICJyZWRpcmVjdE1vZGUiOiAiUkVESVJFQ1QiLAogICJjYWxsYmFja1VybCI6ICJodHRwOi8vbG9jYWxob3N0OjQyMDAvIiwKICAibW9iaWxlTnVtYmVyIjogIjk5OTk5OTk5OTkiLAogICJwYXltZW50SW5zdHJ1bWVudCI6IHsKICAgICJ0eXBlIjogIlBBWV9QQUdFIgogIH0KfQ==" } };
                //- working
                 client.DefaultRequestHeaders.Add("X-VERIFY", finalXHeader.ToLower());

                 var requestData = new Dictionary<string, string> { { "request", encode } };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_phonePeSettings.PaymentApiUrl+ _phonePeSettings.ApiEndpoint, content);
                //var responseContent = response.Content.ReadAsStringAsync().Result;
                //var rData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                response.EnsureSuccessStatusCode();

                // Read and deserialize the response content
                var responseContent = await response.Content.ReadAsStringAsync();

                PaymentResponse response1 = new PaymentResponse();
                response1.TransactionId = transactionId;
                response1.Status = "PAYMENT_SUCCESS";
                response1.Amount = Convert.ToString(order.OrderAmount);

                TransactionHistory.Add(response1);

                // Return a response
                return new JsonResult(new { Success = true, Message = "Verification successful", phonepeResponse = responseContent });
            }

        }

        [HttpPost]
        [Route("Response")]
        public ActionResult PhonePeResponse(string response)
        {
            return this.Ok(TransactionHistory);
        }

        [HttpGet]
        [Route("TransactionDetail")]
        public ActionResult TransactionDetail()
        {
            return this.Ok(TransactionHistory);
        }

        static string ComputeSHA256(string s)
        {
            string hash = String.Empty;

            // Initialize a SHA256 hash object
            using (SHA256 sha256 = SHA256.Create())
            {
                // Compute the hash of the given string
                byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(s));

                // Convert the byte array to string format
                foreach (byte b in hashValue)
                {
                    hash += $"{b:X2}";
                }
            }

            return hash;
        }
    }
}
