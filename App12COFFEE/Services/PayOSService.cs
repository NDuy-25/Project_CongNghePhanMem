using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace App12COFFEE.Services
{
    public class PayOSService
    {
        private readonly string clientId = "2c6d1f7e-fdeb-47d2-bf68-b384028edfdc";
        private readonly string apiKey = "b8e66781-e268-47ff-9ccf-7f502587d4a0";
        private readonly string checksumKey = "4d746f39a6e477ced7f764e9566c424961a3055719d3c90f135019382b774bfc";

        private readonly string payUrl = "https://api-merchant.payos.vn/v2/payment-requests";
        private readonly string basePublicUrl = "https://elvira-socialistic-illa.ngrok-free.dev";

        public string CreatePayment(int maDH, int amount)
        {
            if (maDH <= 0) throw new Exception("MaDH không hợp lệ.");
            if (amount <= 0) throw new Exception("Amount không hợp lệ (phải là số nguyên > 0).");

            string returnUrl = $"{basePublicUrl}/ThanhToan/ThanhToanThanhCong?maDH={maDH}";
            string cancelUrl = $"{basePublicUrl}/ThanhToan/ThanhToanThatBai?maDH={maDH}";
            string description = $"Thanh toan don hang {maDH}";

            // ✅ SIGNATURE theo chuẩn PayOS:
            // data = key=value&key=value... ; sort key alphabet ; không encode URL
            // keys của create payment link: amount, cancelUrl, description, orderCode, returnUrl
            var signData = new Dictionary<string, object>
            {
                { "amount", amount },
                { "cancelUrl", cancelUrl },
                { "description", description },
                { "orderCode", maDH },
                { "returnUrl", returnUrl }
            };

            string rawSignature = ConvertObjToQueryStrSorted(signData);
            string signature = HmacSha256Hex(rawSignature, checksumKey);

            var client = new RestClient(payUrl);
            var request = new RestRequest("", Method.Post);

            request.AddHeader("x-client-id", clientId);
            request.AddHeader("x-api-key", apiKey);

            // Body gửi lên đúng fields + signature
            request.AddJsonBody(new
            {
                orderCode = maDH,
                amount = amount,
                description = description,
                cancelUrl = cancelUrl,
                returnUrl = returnUrl,
                signature = signature
            });

            var response = client.Execute(request);

            Console.WriteLine("\n================ DEBUG PAYOS CREATE ================");
            Console.WriteLine("RAW_TO_SIGN => " + rawSignature);
            Console.WriteLine("SIGNATURE   => " + signature);
            Console.WriteLine("HTTP        => " + (int)response.StatusCode);
            Console.WriteLine("RESPONSE    => " + response.Content);
            Console.WriteLine("====================================================\n");
            if (!response.IsSuccessful)
                throw new Exception("PayOS lỗi -> " + response.Content);

            var json = JObject.Parse(response.Content);

            if (json["code"]?.ToString() != "00")
                throw new Exception("PayOS lỗi -> " + response.Content);

            string checkoutUrl = json["data"]?["checkoutUrl"]?.ToString();
            if (string.IsNullOrWhiteSpace(checkoutUrl))
                throw new Exception("PayOS không trả checkoutUrl -> " + response.Content);

            return checkoutUrl;
        }

        // sort key alphabet và nối key=value&...
        private string ConvertObjToQueryStrSorted(Dictionary<string, object> data)
        {
            var ordered = data.OrderBy(k => k.Key, StringComparer.Ordinal);

            var parts = new List<string>();
            foreach (var kv in ordered)
            {
                object v = kv.Value;

                // PayOS: null/undefined -> ""
                if (v == null) v = "";

                // bool/int/decimal -> ToString invariant
                string valueStr = v.ToString();

                // IMPORTANT: không URL-encode ở đây
                parts.Add($"{kv.Key}={valueStr}");
            }
            return string.Join("&", parts);
        }

        private string HmacSha256Hex(string data, string key)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
