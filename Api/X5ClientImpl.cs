using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using ZLGL_XMOCV.Config;

namespace ZLGL_XMOCV.Api
{
    /// <summary>
    /// X5 协议客户端实现
    /// </summary>
    public class X5ClientImpl : IX5Client
    {
        public X5Result PostData(X5ConfigItem config, string bodyJson)
        {
            try
            {
                // 1. 计算签名：MD5(appid + body + appkey)
                string signRaw = config.AppId + bodyJson + config.AppKey;
                string sign = Md5Encrypt(signRaw).ToUpper();

                // 2. 构造 X5 请求对象
                var x5Request = new X5RequestMessage
                {
                    Header = new X5Header { AppId = config.AppId, Sign = sign },
                    Body = bodyJson
                };
                string x5Json = JsonConvert.SerializeObject(x5Request);

                // 3. Base64 编码 + URL 编码
                string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(x5Json));
                string data = Uri.EscapeDataString(base64);

                // 4. 组装 POST 数据
                string postData = "data=" + data;
                byte[] bytes = Encoding.UTF8.GetBytes(postData);

                // 5. 发起请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(config.Url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bytes.Length;
                request.Timeout = 90000;

                // Basic 认证
                string authInfo = config.UserName + ":" + config.Password;
                string authBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(authInfo));
                request.Headers.Add("Authorization", "Basic " + authBase64);

                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(bytes, 0, bytes.Length);
                }

                // 6. 读取响应
                string responseText = ReadResponse(request);
                return ParseResponse(responseText);
            }
            catch (Exception ex)
            {
                return new X5Result
                {
                    Success = false,
                    Code = "ERROR",
                    Message = ex.Message,
                    RawContent = null
                };
            }
        }

        private static string ReadResponse(HttpWebRequest request)
        {
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                    throw new Exception("网络请求异常: " + ex.Message, ex);

                using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static X5Result ParseResponse(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return new X5Result { Success = false, Code = "EMPTY", Message = "服务器返回内容为空" };
            }

            try
            {
                var result = JsonConvert.DeserializeObject<X5ResponseMessage>(responseText);
                return new X5Result
                {
                    Success = result.Header.Code == "200",
                    Code = result.Header.Code,
                    Message = result.Header.Desc,
                    RawContent = responseText
                };
            }
            catch (JsonReaderException)
            {
                return new X5Result
                {
                    Success = false,
                    Code = "PARSE_ERROR",
                    Message = "服务器返回格式异常",
                    RawContent = responseText
                };
            }
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

    #region X5 协议模型

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
        public string Body { get; set; }
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

    #endregion
}
