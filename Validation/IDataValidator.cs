using System.Collections.Generic;
using System.Data;

namespace ZLGL_XMOCV.Validation
{
    /// <summary>
    /// 数据校验结果
    /// </summary>
    public class ValidationResult
    {
        public List<string> Errors { get; } = new List<string>();
        public bool IsValid => Errors.Count == 0;

        public void AddError(string error)
        {
            Errors.Add(error);
        }
    }

    /// <summary>
    /// 数据校验器接口，每个维度实现一个
    /// </summary>
    public interface IDataValidator
    {
        /// <summary>
        /// 校验 DataTable，返回校验结果
        /// </summary>
        ValidationResult Validate(DataTable dt);
    }
}
