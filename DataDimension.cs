namespace ZLGL_XMOCV
{
    /// <summary>
    /// 数据维度枚举
    /// 每个维度对应小米的一个质量数据接口
    /// </summary>
    public enum DataDimension
    {
        /// <summary>工站良率信息 (IF082)</summary>
        LL,

        /// <summary>制程参数信息 (IF253)</summary>
        ZC,

        /// <summary>OQC 批通率 (IF236)</summary>
        OQC,

        /// <summary>ORT 检验数据 (IF237)</summary>
        ORT,

        /// <summary>IQC 检验信息 (IF251)</summary>
        IQC,

        /// <summary>IPQC 检验信息 (IF252)</summary>
        IPQC,

        /// <summary>未知数据</summary>
        UNKNOWN
    }
}
