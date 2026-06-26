using System;
using System.Collections.Generic;

namespace ZLGL_XMOCV
{
    // X5 接口配置类

    public static class X5Config
    {
        // ============================================================
        // 1. Basic 认证（HTTP Authorization 头）
        //    文档中说明：HTTP 访问需要 Basic Auth，账户为通用账户
        // ============================================================
        public const string UserName = "RFCMESTENPWR";
        public const string Password = "yjXy!ss7";

        // ============================================================
        // 2. X5 协议的 AppId / AppKey
        //    文档中说明：测试环境的 app_id / app_key（生产环境会不同）
        // ============================================================
        public const string AppId = "xm_oem_9312";
        public const string AppKey = "5ce9f42813ddd97e29562bcb5d10ce83";

        // ============================================================
        // 3. 接口 URL
        //    所有接口共享同一基础地址，仅 interface 参数不同
        // ============================================================
        private const string BaseUrl =
            "https://mipoq.p.mi.com/HttpAdapter/HttpMessageServlet" +
            "?interfaceNamespace=http://xiaomi.com/oem/dummy" +
            "&interface={0}" +
            "&senderService=TEN_POWER_QAS" +
            "&qos=BE";

        /// <summary>
        /// 维度 -> 小米接口名称 的映射
        /// 对应文档中 6 个接口：
        ///   LL   工站良率信息     IF082
        ///   OQC  OQC 批通率       IF236
        ///   ORT  ORT 检验数据     IF237
        ///   IQC  IQC 检验信息     IF251
        ///   IPQC IPQC 检验信息    IF252
        ///   ZC   制程参数信息     IF253
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

        /// <summary>
        /// 根据维度获取对应接口的完整 URL。
        /// </summary>
        public static string GetUrl(DataDimension dimension)
        {
            if (!InterfaceNames.TryGetValue(dimension, out string iface))
            {
                throw new ArgumentException($"未配置维度 {dimension} 对应的接口名称", nameof(dimension));
            }
                
            return string.Format(BaseUrl, iface);
        }

        /// <summary>
        /// 一次性返回该维度所需的全部配置项，
        /// 方便调用方一次性取走所有字段。
        /// </summary>
        public static X5ConfigItem GetConfig(DataDimension dimension)
        {
            return new X5ConfigItem
            {
                UserName = UserName,
                Password = Password,
                AppId = AppId,
                AppKey = AppKey,
                Url = GetUrl(dimension),
            };
        }
    }

    /// <summary>
    /// X5 配置项承载类。
    /// </summary>
    public class X5ConfigItem
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AppId { get; set; }
        public string AppKey { get; set; }
        public string Url { get; set; }
    }
}
