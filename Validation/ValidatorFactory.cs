using System;
using System.Data;


namespace ZLGL_XMOCV.Validation
{
    /// <summary>
    /// 校验器工厂，按维度分发校验器
    /// 新增维度只需：1) 创建 XxxValidator 实现 IDataValidator  2) 在此注册
    /// </summary>
    public static class ValidatorFactory
    {
        /// <summary>
        /// 根据维度获取对应的校验器，未注册则返回空校验器（不报错）
        /// </summary>
        public static IDataValidator GetValidator(DataDimension dimension)
        {
            switch (dimension)
            {
                case DataDimension.IQC:
                    return new IqcValidator();
                // TODO: 其他维度在此扩展
                // case DataDimension.OQC:
                //     return new OqcValidator();
                default:
                    return new PassThroughValidator();
            }
        }
    }

    /// <summary>
    /// 空校验器 —— 未实现校验逻辑的维度默认通过
    /// </summary>
    internal class PassThroughValidator : IDataValidator
    {
        public ValidationResult Validate(DataTable dt)
        {
            return new ValidationResult();
        }
    }
}
