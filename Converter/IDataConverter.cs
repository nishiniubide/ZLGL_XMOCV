using System.Data;

namespace ZLGL_XMOCV.Converter
{
    /// <summary>
    /// 数据转换结果
    /// </summary>
    public class ConvertResult
    {
        /// <summary>
        /// 外层公共参数序列化后的 body JSON（包含 factory_code, data 等）
        /// </summary>
        public string BodyJson { get; set; }

        public bool Success => !string.IsNullOrEmpty(BodyJson);
    }

    /// <summary>
    /// DataTable → X5 请求体 JSON 转换器接口，每个维度实现一个
    /// </summary>
    public interface IDataConverter
    {
        /// <summary>
        /// 将 DataTable 转换为 X5 请求包
        /// </summary>
        ConvertResult Convert(DataTable dt);
    }
}
