using System;
using System.Collections.Generic;

namespace ZLGL_XMOCV.Config
{
    /// <summary>
    /// X5 配置项承载类
    /// </summary>
    public class X5ConfigItem
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AppId { get; set; }
        public string AppKey { get; set; }
        public string Url { get; set; }
    }

    /// <summary>
    /// 接口地址配置（不含凭据）
    /// 凭据由 CredentialProvider 管理
    /// </summary>
    public static class X5ApiConfig
    {
        private const string BaseUrl =
            "https://mipoq.p.mi.com/HttpAdapter/HttpMessageServlet" +
            "?interfaceNamespace=http://xiaomi.com/oem/dummy" +
            "&interface={0}" +
            "&senderService=TEN_POWER_QAS" +
            "&qos=BE";

        /// <summary>
        /// 维度 -> 小米接口名称映射
        /// </summary>
        private static readonly Dictionary<DataDimension, string> InterfaceNames =
            new Dictionary<DataDimension, string>
            {
                { DataDimension.LL,   "SI_MI_OEM_IF082_SITE_YIELD_S_OUT" },
                { DataDimension.OQC,  "SI_MI_OEM_IF236_PUSH_QMS_OQC_S_OUT" },
                { DataDimension.ORT,  "SI_MI_OEM_IF237_PUSH_QMS_ORT_S_OUT" },
                { DataDimension.IQC,  "SI_MI_OEM_IF251_QMS_VENDOR_IQC_S_OUT" },
                { DataDimension.IPQC, "SI_MI_OEM_IF252_QMS_VENDOR_IPQC_S_OUT" },
                { DataDimension.ZC,   "SI_MI_OEM_IF253_QMS_PROCESS_PARAMETER_S_OUT" },
            };

        private static readonly Dictionary<DataDimension, string> TestInterfaceUrls =
            new Dictionary<DataDimension, string>
            {
                { DataDimension.IQC, "http://report.scms.test.b2c.srv/qms/x5/material/quality/iqc" }
            };

        public static string GetUrl(DataDimension dimension)
        {
            if (!InterfaceNames.TryGetValue(dimension, out string iface))
                throw new ArgumentException($"未配置维度 {dimension} 对应的接口名称", nameof(dimension));
            return string.Format(BaseUrl, iface);
        }

        public static string GetUrlTest(DataDimension dimension)
        {
            if (TestInterfaceUrls.TryGetValue(dimension, out string url))
                return url;
            throw new ArgumentException($"未配置维度 {dimension} 对应的测试地址", nameof(dimension));
        }
    }
}
