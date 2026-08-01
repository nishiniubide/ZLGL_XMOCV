using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;


namespace ZLGL_XMOCV.Excel
{
    /// <summary>
    /// 基于 NPOI 的 Excel 读取器
    /// 从 FrmZLGL_XMOCV.ReadExcelWithNPOI 提取而来
    /// </summary>
    public class NpoiExcelReader : IExcelReader
    {
        public DataTable Read(string filePath, string sheetName, DataDimension dimension)
        {
            IWorkbook workbook;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (Path.GetExtension(filePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                    workbook = new XSSFWorkbook(fs);
                else
                    workbook = new HSSFWorkbook(fs);
            }

            ISheet sheet = string.IsNullOrEmpty(sheetName)
                ? workbook.GetSheetAt(0)
                : (workbook.GetSheet(sheetName) ?? workbook.GetSheetAt(0));

            IRow headerRow = FindHeaderRow(sheet, dimension);
            int headerRowIndex = headerRow.RowNum;

            int firstCol = headerRow.FirstCellNum;
            int lastCol = headerRow.LastCellNum;
            int colCount = lastCol - firstCol;

            DataTable dt = CreateColumns(headerRow, firstCol, lastCol);
            ReadDataRows(sheet, dt, dimension, headerRowIndex, firstCol, colCount);

            return dt;
        }

        #region 表头识别

        /// <summary>
        /// 多维度表头识别策略
        /// </summary>
        private static IRow FindHeaderRow(ISheet sheet, DataDimension dimension)
        {
            // 策略1：按关键字精确匹配
            var dimensionKeywords = new Dictionary<DataDimension, string[]>
            {
                { DataDimension.OQC, new[] { "不良名称", "不良数量" } },
                { DataDimension.LL,  new[] { "不良类型", "不良详细描述" } },
            };

            if (dimensionKeywords.ContainsKey(dimension))
            {
                string[] keywords = dimensionKeywords[dimension];
                for (int i = 0; i <= sheet.LastRowNum; i++)
                {
                    var r = sheet.GetRow(i);
                    if (r == null) continue;

                    bool allFound = keywords.All(kw =>
                    {
                        for (int j = 0; j < r.LastCellNum; j++)
                        {
                            ICell cell = GetCellIncludingMerged(sheet, r, j, i);
                            if (cell != null && !string.IsNullOrEmpty(cell.ToString()?.Trim()) && cell.ToString().Trim().Contains(kw))
                                return true;
                        }
                        return false;
                    });

                    if (allFound) return r;
                }
            }

            // 策略2：查找包含"(必填)"最多的行
            IRow bestRow = null;
            int maxRequiredCount = 0;
            for (int i = 0; i <= Math.Min(4, sheet.LastRowNum); i++)
            {
                var r = sheet.GetRow(i);
                if (r == null) continue;
                int count = r.Cells
                    .Where(c => c.CellType == CellType.String)
                    .Select(c => c.StringCellValue?.Trim())
                    .Count(s => !string.IsNullOrEmpty(s) && s.Contains("(必填)"));
                if (count > maxRequiredCount)
                {
                    maxRequiredCount = count;
                    bestRow = r;
                }
            }

            // 策略3：回退到第2行
            return bestRow ?? sheet.GetRow(1) ?? sheet.GetRow(0);
        }

        #endregion

        #region 列创建

        private static DataTable CreateColumns(IRow headerRow, int firstCol, int lastCol)
        {
            DataTable dt = new DataTable();
            for (int colIndex = firstCol; colIndex < lastCol; colIndex++)
            {
                ICell cell = headerRow.GetCell(colIndex);
                string colName = (cell != null && cell.CellType == CellType.String)
                    ? cell.StringCellValue.Trim()
                    : cell?.ToString()?.Trim() ?? "";

                if (string.IsNullOrEmpty(colName))
                    colName = $"列{colIndex}";

                // 处理重名列
                string uniqueName = colName;
                int suffix = 1;
                while (dt.Columns.Contains(uniqueName))
                    uniqueName = $"{colName}_{suffix++}";

                dt.Columns.Add(uniqueName);
            }
            return dt;
        }

        #endregion

        #region 数据行读取

        private static readonly Dictionary<DataDimension, string[]> HeaderFilterKeywords =
            new Dictionary<DataDimension, string[]>
            {
                { DataDimension.OQC, new[] { "不良名称", "不良数量", "不良明细", "OQC批通率" } },
                { DataDimension.LL,  new[] { "不良类型", "不良详细描述", "不良明细", "工站良率数据" } },
            };

        private static void ReadDataRows(ISheet sheet, DataTable dt, DataDimension dimension, int headerRowIndex, int firstCol, int colCount)
        {
            for (int rowIndex = headerRowIndex + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                if (row == null) continue;

                // 跳过表头残余行
                if (IsHeaderLikeRow(sheet, row, dimension, firstCol, colCount, rowIndex))
                    continue;

                // 跳过空行
                if (IsEmptyRow(sheet, row, firstCol, colCount, rowIndex))
                    continue;

                DataRow dataRow = dt.NewRow();
                for (int colIndex = 0; colIndex < colCount; colIndex++)
                {
                    ICell cell = GetCellIncludingMerged(sheet, row, firstCol + colIndex, rowIndex);
                    if (cell != null)
                    {
                        dataRow[colIndex] = ReadCellValue(cell);
                    }
                }
                dt.Rows.Add(dataRow);
            }
        }

        private static bool IsHeaderLikeRow(ISheet sheet, IRow row, DataDimension dimension, int firstCol, int colCount, int rowIndex)
        {
            if (!HeaderFilterKeywords.ContainsKey(dimension)) return false;

            string[] keywords = HeaderFilterKeywords[dimension];
            for (int i = 0; i < colCount; i++)
            {
                ICell checkCell = GetCellIncludingMerged(sheet, row, firstCol + i, rowIndex);
                if (checkCell != null)
                {
                    string val = checkCell.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(val) && keywords.Any(kw => val.Contains(kw)))
                        return true;
                }
            }
            return false;
        }

        private static bool IsEmptyRow(ISheet sheet, IRow row, int firstCol, int colCount, int rowIndex)
        {
            for (int i = 0; i < colCount; i++)
            {
                ICell cell = GetCellIncludingMerged(sheet, row, firstCol + i, rowIndex);
                if (cell != null && !string.IsNullOrWhiteSpace(cell.ToString()))
                    return false;
            }
            return true;
        }

        private static object ReadCellValue(ICell cell)
        {
            switch (cell.CellType)
            {
                case CellType.Numeric:
                    return HandleNumericCell(cell);
                case CellType.Formula:
                    switch (cell.CachedFormulaResultType)
                    {
                        case CellType.Numeric:
                            return HandleNumericCell(cell);
                        case CellType.Boolean:
                            return cell.BooleanCellValue;
                        default:
                            return cell.ToString().Trim();
                    }
                case CellType.Boolean:
                    return cell.BooleanCellValue;
                default:
                    return cell.ToString().Trim();
            }
        }

        private static object HandleNumericCell(ICell cell)
        {
            if (IsPercentageFormat(cell))
            {
                double percentageValue = cell.NumericCellValue * 100;
                return string.Format("{0:F2}%", percentageValue);
            }

            if (HSSFDateUtil.IsCellDateFormatted(cell))
                return cell.DateCellValue.ToString();

            return cell.NumericCellValue.ToString();
        }

        private static bool IsPercentageFormat(ICell cell)
        {
            IDataFormat dataFormat = cell.Sheet.Workbook.CreateDataFormat();
            string formatString = dataFormat.GetFormat(cell.CellStyle.DataFormat);
            return !string.IsNullOrEmpty(formatString) && formatString.Contains("%");
        }

        #endregion

        #region 合并单元格处理

        /// <summary>
        /// 获取单元格值，如果为空则回退到合并区域左上角
        /// </summary>
        private static ICell GetCellIncludingMerged(ISheet sheet, IRow row, int colIndex, int rowIndex)
        {
            ICell cell = row.GetCell(colIndex);
            if (cell != null && !string.IsNullOrWhiteSpace(cell.ToString()))
                return cell;

            for (int i = 0; i < sheet.NumMergedRegions; i++)
            {
                var region = sheet.GetMergedRegion(i);
                if (rowIndex >= region.FirstRow && rowIndex <= region.LastRow &&
                    colIndex >= region.FirstColumn && colIndex <= region.LastColumn)
                {
                    IRow firstRowOfRegion = sheet.GetRow(region.FirstRow);
                    return firstRowOfRegion?.GetCell(region.FirstColumn);
                }
            }
            return cell;
        }

        #endregion
    }
}
