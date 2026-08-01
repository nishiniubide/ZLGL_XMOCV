using System;
using System.Data;

namespace ZLGL_XMOCV.Converter
{
    /// <summary>
    /// 转换器工厂，按维度分发转换器
    /// 新增维度只需：1) 创建 XxxConverter 实现 IDataConverter  2) 在此注册
    /// </summary>
    public static class ConverterFactory
    {
        public static IDataConverter GetConverter(DataDimension dimension)
        {
            switch (dimension)
            {
                case DataDimension.LL:
                    return new YieldConverter();
                case DataDimension.OQC:
                    return new OqcConverter();
                case DataDimension.ORT:
                    return new OrtConverter();
                case DataDimension.IQC:
                    return new IqcConverter();
                case DataDimension.IPQC:
                    return new IpqcConverter();
                case DataDimension.ZC:
                    return new ProcessConverter();
                default:
                    throw new ArgumentException($"未实现维度 {dimension} 的转换器", nameof(dimension));
            }
        }
    }
}
