using DevExpress.XtraPrinting.HtmlExport.Native;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ZLGL_XMOCV
{
    public class X5Header
    {
        [JsonProperty("appid")]
        public string AppId { get; set; }

        [JsonProperty("sign")]
        public string Sign { get; set; }
    }

    public class X5RequestMessage
    {
        [JsonProperty("header")]
        public X5Header Header { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }   // 已经是 JSON 字符串
    }

    public class X5ResponseHeader
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }
    }

    public class X5ResponseMessage
    {
        [JsonProperty("header")]
        public X5ResponseHeader Header { get; set; }

        [JsonProperty("body")]
        public object Body { get; set; }
    }

    public static class X5Client
    {
        /// <summary>
        /// 发送 X5 请求
        /// </summary>
        /// <param name="userName">Basic 认证用户名</param>
        /// <param name="password">Basic 认证密码</param>
        /// <param name="url">接口地址</param>
        /// <param name="appId">应用ID</param>
        /// <param name="appKey">应用密钥</param>
        /// <param name="bodyJson">业务数据 JSON 字符串（即 data 数组序列化后的结果）</param>
        /// <returns>响应报文对象</returns>
        public static X5ResponseMessage PostData(string userName, string password, string url, string appId, string appKey, string bodyJson)
        {
            // 1. 计算签名：MD5(appid + body + appkey)
            string signRaw = appId + bodyJson + appKey;
            string sign = Md5Encrypt(signRaw).ToUpper();

            // 2. 构造 X5 请求对象
            var x5Request = new X5RequestMessage
            {
                Header = new X5Header { AppId = appId, Sign = sign },
                Body = bodyJson
            };
            string x5Json = JsonConvert.SerializeObject(x5Request);

            // 3. Base64 编码 + URL 编码 (使用原生的 HttpUtility 替代 DXHttpUtility)
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(x5Json));
            string data = DXHttpUtility.UrlEncode(base64, Encoding.UTF8);

            // 4. 组装 POST 数据
            string postData = "data=" + data;
            byte[] bytes = Encoding.UTF8.GetBytes(postData);

            // 5. 发起请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = bytes.Length;
            request.Timeout = 90000;

            // Basic 认证
            string authInfo = userName + ":" + password;
            string authBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(authInfo));
            request.Headers.Add("Authorization", "Basic " + authBase64);

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bytes, 0, bytes.Length);
            }

            // 6. 读取响应 (增加空引用保护)
            string responseText;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseText = reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                // ✅ 关键修改：如果 ex.Response 为 null（如超时、DNS错误），避免抛出 NullReferenceException
                if (ex.Response == null)
                {
                    throw new Exception($"网络请求失败，无响应返回: {ex.Message}", ex);
                }
                using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    responseText = reader.ReadToEnd();
                }
            }

            // 7. 反序列化响应
            return JsonConvert.DeserializeObject<X5ResponseMessage>(responseText);
        }

        private static string Md5Encrypt(string source)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] input = Encoding.UTF8.GetBytes(source);
                byte[] hash = md5.ComputeHash(input);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}