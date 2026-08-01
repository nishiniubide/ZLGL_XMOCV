namespace ZLGL_XMOCV.Api
{
    /// <summary>
    /// X5 接口响应结果
    /// </summary>
    public class X5Result
    {
        public bool Success { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string RawContent { get; set; }
    }

    /// <summary>
    /// X5 协议客户端接口，隔离 HTTP 通信细节
    /// </summary>
    public interface IX5Client
    {
        /// <summary>
        /// 发送数据到 X5 接口
        /// </summary>
        /// <param name="config">接口配置（URL + 凭据）</param>
        /// <param name="bodyJson">业务数据 JSON</param>
        X5Result PostData(Config.X5ConfigItem config, string bodyJson);
    }
}
