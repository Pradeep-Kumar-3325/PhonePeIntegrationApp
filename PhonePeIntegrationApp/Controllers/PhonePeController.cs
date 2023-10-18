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
        public PhonePeController(IOptions<PhonePeSettings> configuration)
        {
            _phonePeSettings = configuration.Value;
        }

        [HttpPost]
        [Route("CreatePayment")]
        public async Task<JsonResult> CreatePayment(OrderDetailModel order)
        {
            var data = new Dictionary<string, object>
            {
                { "merchantId", _phonePeSettings.MerchentId },
                { "merchantTransactionId", Guid.NewGuid().ToString() },
                { "merchantUserId", Convert.ToString(order.UserId) },
                { "merchantOrderId", order.OrderId },
                { "amount", Convert.ToString(order.OrderAmount) },
                { "redirectUrl", "http://localhost:4200/payment-status"},
                { "redirectMode", "REDIRECT"},
                { "callbackUrl", $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}/PhonePe/Response" },
                { "paymentInstrument", new Dictionary<string, string> { { "type", "PAY_PAGE" } } }
            };
            var encode = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
            var saltKey = _phonePeSettings.MerchentSecretKey;
            var saltIndex = _phonePeSettings.SaltIndex;
            var stringToHash = encode + _phonePeSettings.ApiEndpoint + saltKey;

            var sha256 = ComputeSHA256(stringToHash);
            var finalXHeader = sha256 + "###" + saltIndex;

            using (var client = new HttpClient())
            {
                //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
                client.DefaultRequestHeaders.Add("X-VERIFY", finalXHeader);

                var requestData = new Dictionary<string, string> { { "request", encode } };
                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_phonePeSettings.PaymentApiUrl+ _phonePeSettings.ApiEndpoint, content);
                //var responseContent = response.Content.ReadAsStringAsync().Result;
                //var rData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                response.EnsureSuccessStatusCode();

                // Read and deserialize the response content
                var responseContent = await response.Content.ReadAsStringAsync();

                // Return a response
                return new JsonResult(new { Success = true, Message = "Verification successful", phonepeResponse = responseContent });
            }

        }

        [HttpPost]
        [Route("Response")]
        public ActionResult PhonePeResponse(string response)
        {
            return null;
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
