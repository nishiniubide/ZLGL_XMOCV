namespace ZLGL_XMOCV.Config
{
    /// <summary>
    /// 凭据提供器 —— 统一管理认证信息
    /// 当前为硬编码（兼容原项目），后续可改为读取配置文件或环境变量
    /// </summary>
    public static class CredentialProvider
    {
        // TODO: 改为从 App.config / 环境变量 / 加密配置文件读取
        private const string UserName = "RFCMESTENPWR";
        private const string Password = "yjXy!ss7";
        private const string AppId = "xm_oem_9312";
        private const string AppKey = "5ce9f42813ddd97e29562bcb5d10ce83";

        public static string GetUserName() => UserName;
        public static string GetPassword() => Password;
        public static string GetAppId() => AppId;
        public static string GetAppKey() => AppKey;

        /// <summary>
        /// 获取完整配置项（凭据 + 接口地址）
        /// </summary>
        public static X5ConfigItem GetConfig(DataDimension dimension)
        {
            return new X5ConfigItem
            {
                UserName = GetUserName(),
                Password = GetPassword(),
                AppId = GetAppId(),
                AppKey = GetAppKey(),
                Url = X5ApiConfig.GetUrl(dimension)
            };
        }

        /// <summary>
        /// 获取测试环境配置项
        /// </summary>
        public static X5ConfigItem GetConfigTest(DataDimension dimension)
        {
            return new X5ConfigItem
            {
                UserName = GetUserName(),
                Password = GetPassword(),
                AppId = GetAppId(),
                AppKey = GetAppKey(),
                Url = X5ApiConfig.GetUrlTest(dimension)
            };
        }
    }
}
