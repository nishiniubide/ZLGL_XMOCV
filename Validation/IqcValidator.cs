using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ZLGL_XMOCV.Validation
{
    /// <summary>
    /// IQC 维度数据校验器
    /// </summary>
    public class IqcValidator : IDataValidator
    {
        public ValidationResult Validate(DataTable dt)
        {
            var result = new ValidationResult();

            string colFieldType = "物料类型(必填)";
            string colResult = "结果判定";
            string colDealMethod = "不良处理方式";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                int excelRowIndex = i + 1;

                ValidateColumnValue(row, colFieldType, new[] { "电芯", "PACK" }, false, excelRowIndex, result.Errors);
                ValidateColumnValue(row, colResult, new[] { "OK", "NG" }, false, excelRowIndex, result.Errors);
                ValidateColumnValue(row, colDealMethod, new[] { "退货", "返修" }, true, excelRowIndex, result.Errors);
            }
            return result;
        }

        private static void ValidateColumnValue(DataRow row, string columnName, string[] allowedValues, bool allowEmpty, int rowIndex, List<string> errorList)
        {
            if (!row.Table.Columns.Contains(columnName))
            {
                errorList.Add($"第 {rowIndex} 行: 缺少列 [{columnName}]，无法校验。");
                return;
            }

            string valStr = (row[columnName] == null) ? "" : row[columnName].ToString().Trim();

            if (string.IsNullOrEmpty(valStr))
            {
                if (!allowEmpty)
                {
                    errorList.Add($"第 {rowIndex} 行: [{columnName}] 不能为空，只能是 [{string.Join("、", allowedValues)}]。");
                }
                return;
            }

            if (!allowedValues.Contains(valStr))
            {
                errorList.Add($"第 {rowIndex} 行: [{columnName}] 值 [{valStr}] 不合法，只能是 [{string.Join("、", allowedValues)}]。");
            }
        }
    }
}
