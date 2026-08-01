using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ZLGL_XMOCV
{
    public static class DataValidator
    {
        /// <summary>
        /// 校验入口
        /// </summary>
        public static List<string> Validate(DataTable dt, DataDimension dimension)
        {
            List<string> errors = new List<string>();
            if (dt == null || dt.Rows.Count == 0) return errors;

            switch (dimension)
            {
                case DataDimension.IQC:
                    errors.AddRange(ValidateIQC(dt));
                    break;
                // 其他维度 case 可在此扩展
                default:
                    break;
            }
            return errors;
        }

        // IQC 具体校验逻辑
        private static List<string> ValidateIQC(DataTable dt)
        {
            List<string> errors = new List<string>();

            // 列名定义（必须与 X5DataConverter.cs 中读取的列名完全一致）
            string colFieldType = "物料类型(必填)";
            string colResult = "结果判定";
            string colDealMethod = "不良处理方式";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                // Excel 第1行
                int excelRowIndex = i + 1;

                // 1. 物料类型：只能是（电芯、PACK），不能为空
                ValidateColumnValue(row, colFieldType, new[] { "电芯", "PACK" }, false, excelRowIndex, errors);

                // 2. 结果判定：只能是（OK、NG），不能为空
                ValidateColumnValue(row, colResult, new[] { "OK", "NG" }, false, excelRowIndex, errors);

                // 3. 不良处理方式：只能是（退货、返修），允许为空（根据业务通常逻辑，结果为OK时可能为空）
                ValidateColumnValue(row, colDealMethod, new[] { "退货", "返修" }, true, excelRowIndex, errors);
            }
            return errors;
        }

        /// <summary>
        /// 【通用方法】校验列值是否在允许范围内
        /// </summary>
        /// <param name="row">数据行</param>
        /// <param name="columnName">列名称</param>
        /// <param name="allowedValues">允许的枚举值数组</param>
        /// <param name="allowEmpty">是否允许为空</param>
        /// <param name="rowIndex">当前行号</param>
        /// <param name="errorList">错误信息列表</param>
        private static void ValidateColumnValue(DataRow row, string columnName, string[] allowedValues, bool allowEmpty, int rowIndex, List<string> errorList)
        {
            // 检查列是否存在
            if (!row.Table.Columns.Contains(columnName))
            {
                errorList.Add($"第 {rowIndex} 行: 缺少列 [{columnName}]，无法校验。");
                return;
            }

            object valObj = row[columnName];
            string valStr = (valObj == null) ? "" : valObj.ToString().Trim();

            // 空值校验
            if (string.IsNullOrEmpty(valStr))
            {
                if (!allowEmpty)
                {
                    errorList.Add($"第 {rowIndex} 行: [{columnName}] 不能为空，只能是 [{string.Join("、", allowedValues)}]。");
                }
                return;
            }

            // 值范围校验
            if (!allowedValues.Contains(valStr))
            {
                errorList.Add($"第 {rowIndex} 行: [{columnName}] 值 [{valStr}] 不合法，只能是 [{string.Join("、", allowedValues)}]。");
            }
        }
    }
}
