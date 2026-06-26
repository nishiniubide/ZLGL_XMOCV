using ERPWcfClient;
using ERPWcfClient.Common;
using NPOI;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace ZLGL_XMOCV
{
    public partial class FrmZLGL_XMOCV : FrmBase
    {
        private DataTable importedData; // 存储导入的DataTable
        private DataDimension currentDimension; // 当前选中的维度

        public FrmZLGL_XMOCV()
        {
            InitializeComponent();
        }

        private void FrmZLGL_XMOCV_Load(object sender, EventArgs e)
        {
            Utils.LookUpDataBind("XiaomiUpProject", lkpUpProject);
        }

        private void lkpUpProject_EditValueChanged(object sender, EventArgs e)
        {
            if (lkpUpProject.EditValue == null)
            {
                return;
            }
            string tabName = "Tab" + lkpUpProject.EditValue.ToString();
            var xtraTabControl = FindControl<DevExpress.XtraTab.XtraTabControl>(this);
            if (xtraTabControl != null)
            {
                var page = xtraTabControl.TabPages.OfType<DevExpress.XtraTab.XtraTabPage>().FirstOrDefault(p => p.Name == tabName);
                if (page != null)
                {
                    xtraTabControl.SelectedTabPage = page;
                    BindDataToCurrentGrid(page);
                }
                return;
            }
        }

        private DataTable ReadExcelWithNPOI(string fileName, string sheetName, DataDimension dimension = DataDimension.LL)
        {
            try
            {
                IWorkbook workbook;
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (Path.GetExtension(fileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                        workbook = new XSSFWorkbook(fs);
                    else
                        workbook = new HSSFWorkbook(fs);
                }

                ISheet sheet = string.IsNullOrEmpty(sheetName) ? workbook.GetSheetAt(0) : (workbook.GetSheet(sheetName) ?? workbook.GetSheetAt(0));

                // ================================================================
                // ✅ 修改点1：多维度特殊标题行查找策略
                // 不同维度的 Excel 模板有 2~3 行表头，真正的列名在最后一行表头
                // 通过查找只出现在"真正表头行"中的关键列名来定位
                // ================================================================
                IRow headerRow = null;

                // ✅ 各维度的"关键列名"——这些列名只出现在真正的表头行中，不会出现在上级合并表头中
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

                        // 检查该行是否同时包含所有关键列名
                        bool allKeywordsFound = true;
                        foreach (string keyword in keywords)
                        {
                            bool keywordFound = false;
                            for (int j = 0; j < r.LastCellNum; j++)
                            {
                                ICell cell = GetCellIncludingMerged(sheet, r, j, i);
                                if (cell != null)
                                {
                                    string val = cell.ToString()?.Trim();
                                    if (!string.IsNullOrEmpty(val) && val.Contains(keyword))
                                    {
                                        keywordFound = true;
                                        break;
                                    }
                                }
                            }
                            if (!keywordFound)
                            {
                                allKeywordsFound = false;
                                break;
                            }
                        }

                        if (allKeywordsFound)
                        {
                            headerRow = r;
                            break;
                        }
                    }
                }

                // 通用策略：在前5行中，查找包含 "(必填)" 关键字最多的行
                if (headerRow == null)
                {
                    int maxRequiredCount = 0;
                    for (int i = 0; i <= Math.Min(4, sheet.LastRowNum); i++)
                    {
                        var r = sheet.GetRow(i);
                        if (r == null) continue;
                        var cellStrings = r.Cells
                            .Where(c => c.CellType == CellType.String)
                            .Select(c => c.StringCellValue?.Trim())
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();

                        int requiredCount = cellStrings.Count(s => s.Contains("(必填)"));
                        if (requiredCount > maxRequiredCount)
                        {
                            maxRequiredCount = requiredCount;
                            headerRow = r;
                        }
                    }
                }

                // 回退：如果没有任何行包含 "(必填)"，默认取第2行（索引1）
                if (headerRow == null)
                    headerRow = sheet.GetRow(1) ?? sheet.GetRow(0);

                int headerRowIndex = headerRow.RowNum;

                // ================================================================
                // ✅ 修改点2：使用完整的列范围创建列
                // ================================================================
                int firstCol = headerRow.FirstCellNum;
                int lastCol = headerRow.LastCellNum;
                int colCount = lastCol - firstCol;

                DataTable dt = new DataTable();

                // 创建列（处理空列名和重名列）
                for (int colIndex = firstCol; colIndex < lastCol; colIndex++)
                {
                    ICell cell = headerRow.GetCell(colIndex);
                    string colName;
                    if (cell != null)
                    {
                        colName = cell.CellType == CellType.String ? cell.StringCellValue.Trim() : cell.ToString().Trim();
                    }
                    else
                    {
                        colName = $"列{colIndex}";
                    }
                    if (string.IsNullOrEmpty(colName))
                        colName = $"列{colIndex}";

                    // 处理重名列
                    string uniqueName = colName;
                    int suffix = 1;
                    while (dt.Columns.Contains(uniqueName))
                        uniqueName = $"{colName}_{suffix++}";

                    dt.Columns.Add(uniqueName);
                }

                // ================================================================
                // ✅ 修改点3：数据行读取 + 多维度安全过滤
                // ================================================================

                // ✅ 各维度需要过滤掉的"标题关键字"（防止多行表头被当作数据读入）
                var headerFilterKeywords = new Dictionary<DataDimension, string[]>
                {
                    { DataDimension.OQC, new[] { "不良名称", "不良数量", "不良明细", "OQC批通率" } },
                    { DataDimension.LL,  new[] { "不良类型", "不良详细描述", "不良明细", "工站良率数据" } },
                };

                for (int rowIndex = headerRowIndex + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    IRow row = sheet.GetRow(rowIndex);
                    if (row == null) continue;

                    // ✅ 安全检查：跳过包含标题关键字的行（防止多行表头被当作数据读入）
                    if (headerFilterKeywords.ContainsKey(dimension))
                    {
                        string[] filterKeywords = headerFilterKeywords[dimension];
                        bool isHeaderLike = false;
                        for (int i = 0; i < colCount; i++)
                        {
                            ICell checkCell = GetCellIncludingMerged(sheet, row, firstCol + i, rowIndex);
                            if (checkCell != null)
                            {
                                string val = checkCell.ToString()?.Trim();
                                if (!string.IsNullOrEmpty(val) &&
                                    filterKeywords.Any(kw => val.Contains(kw)))
                                {
                                    isHeaderLike = true;
                                    break;
                                }
                            }
                        }
                        if (isHeaderLike) continue;
                    }

                    // 检查是否为空行
                    bool isEmptyRow = true;
                    for (int i = 0; i < colCount; i++)
                    {
                        ICell cell = GetCellIncludingMerged(sheet, row, firstCol + i, rowIndex);
                        if (cell != null && !string.IsNullOrWhiteSpace(cell.ToString()))
                        {
                            isEmptyRow = false;
                            break;
                        }
                    }
                    if (isEmptyRow) continue;

                    DataRow dataRow = dt.NewRow();
                    for (int colIndex = 0; colIndex < colCount; colIndex++)
                    {
                        ICell cell = GetCellIncludingMerged(sheet, row, firstCol + colIndex, rowIndex);
                        if (cell != null)
                        {
                            switch (cell.CellType)
                            {
                                case CellType.Numeric:
                                    HandleNumericCell(cell, dataRow, colIndex);
                                    break;
                                case CellType.Formula:
                                    switch (cell.CachedFormulaResultType)
                                    {
                                        case CellType.Numeric:
                                            HandleNumericCell(cell, dataRow, colIndex);
                                            break;
                                        case CellType.Boolean:
                                            dataRow[colIndex] = cell.BooleanCellValue;
                                            break;
                                        default:
                                            dataRow[colIndex] = cell.ToString().Trim();
                                            break;
                                    }
                                    break;
                                case CellType.Boolean:
                                    dataRow[colIndex] = cell.BooleanCellValue;
                                    break;
                                default:
                                    dataRow[colIndex] = cell.ToString().Trim();
                                    break;
                            }
                        }
                    }
                    dt.Rows.Add(dataRow);
                }
                return dt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "EXCEL文件(*.xlsx)|*.xlsx|EXCEL文件(*.xls)|*.xls";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 获取当前选择的维度，以便在读取Excel时进行特殊处理
                    object editVal = lkpUpProject.EditValue;
                    DataDimension importDimension = DataDimension.LL; // 默认值
                    if (editVal is DataDimension dim)
                    {
                        importDimension = dim;
                    }
                    else if (editVal is int intVal)
                    {
                        importDimension = (DataDimension)intVal;
                    }
                    else if (editVal != null && Enum.TryParse(editVal.ToString(), out DataDimension parsed))
                    {
                        importDimension = parsed;
                    }

                    importedData = ReadExcelWithNPOI(ofd.FileName, null, importDimension);
                    Control activePage = GetCurrentActiveTabPage();
                    BindDataToCurrentGrid(activePage);
                    Msg.ShowInformation($"导入成功，共 {importedData.Rows.Count} 行数据");
                }
                catch (Exception ex)
                {
                    Msg.ShowException(ex);
                }
            }
        }

        private void btnUpload_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (importedData == null || importedData.Rows.Count == 0)
            {
                Msg.ShowInformation("请先导入Excel数据");
                return;
            }

            // ====== 当前维度 ======
            object editVal = lkpUpProject.EditValue;
            if (editVal is DataDimension dim)
            {
                currentDimension = dim;
            }
            else if (editVal is int intVal)
            {
                currentDimension = (DataDimension)intVal;
            }
            else if (editVal != null && Enum.TryParse(editVal.ToString(), out DataDimension parsed))
            {
                currentDimension = parsed;
            }
            else
            {
                Msg.ShowInformation("请先选择上传维度");
                return;
            }

            var package = X5DataConverter.BuildRequestPackage(importedData, currentDimension);
            if (package == null)
            {
                Msg.ShowInformation("数据转换失败");
                return;
            }

            string userName;
            string password;
            string url;
            string appId;
            string appKey;
            try
            {
                var cfg = X5Config.GetConfig(currentDimension);
                userName = cfg.UserName;
                password = cfg.Password;
                appId = cfg.AppId;
                appKey = cfg.AppKey;
                url = cfg.Url;
            }
            catch (Exception ex)
            {
                Msg.ShowException(new Exception("读取 X5 配置失败: " + ex.Message));
                return;
            }

            // 空值校验
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(url) || string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appKey))
            {
                Msg.ShowError("X5Config 中存在空字段，请检查！");
                return;
            }

            // 调用 X5Client 发送请求
            X5ResponseMessage response = X5Client.PostData(userName, password, url, appId, appKey, package.BodyJson);
            if (response.Header.Code == "200")
            {
                ShowMessage($"上传成功！{response.Header.Desc}", true);
            }
            else
            {
                ShowMessage($"上传失败：{response.Header.Code} - {response.Header.Desc}", false);
            }
        }

        #region 辅助方法

        // 提取数值类型单元格（包括公式计算后的数值）的处理逻辑
        private void HandleNumericCell(ICell cell, DataRow dataRow, int colIndex)
        {
            if (IsPercentageFormat(cell))
            {
                double percentageValue = cell.NumericCellValue * 100;
                dataRow[colIndex] = string.Format("{0:F2}%", percentageValue);
            }
            else
            {
                if (HSSFDateUtil.IsCellDateFormatted(cell))
                {
                    dataRow[colIndex] = cell.DateCellValue.ToString();
                }
                else
                {
                    dataRow[colIndex] = cell.NumericCellValue.ToString();
                }
            }
        }

        // 判断单元格是否为百分比格式
        private bool IsPercentageFormat(ICell cell)
        {
            IDataFormat dataFormat = cell.Sheet.Workbook.CreateDataFormat();
            string formatString = dataFormat.GetFormat(cell.CellStyle.DataFormat);
            return !string.IsNullOrEmpty(formatString) && formatString.Contains("%");
        }

        // 获取单元格的值，如果当前单元格为空，检查它是否在合并单元格区域内，如果是则返回合并区域左上角的单元格。
        private ICell GetCellIncludingMerged(ISheet sheet, IRow row, int colIndex, int rowIndex)
        {
            // 先尝试直接获取当前行当前列的单元格
            ICell cell = row.GetCell(colIndex);

            // 如果单元格有值，直接返回
            if (cell != null && !string.IsNullOrWhiteSpace(cell.ToString()))
            {
                return cell;
            }

            // 如果为空，检查是否在合并单元格区域内
            for (int i = 0; i < sheet.NumMergedRegions; i++)
            {
                var region = sheet.GetMergedRegion(i);

                // 判断当前坐标是否在合并区域内
                if (rowIndex >= region.FirstRow && rowIndex <= region.LastRow &&
                    colIndex >= region.FirstColumn && colIndex <= region.LastColumn)
                {
                    // 返回合并区域左上角的单元格（该单元格保存了合并区域的值）
                    IRow firstRowOfRegion = sheet.GetRow(region.FirstRow);
                    if (firstRowOfRegion != null)
                    {
                        return firstRowOfRegion.GetCell(region.FirstColumn);
                    }
                    break;
                }
            }
            return cell; // 不在任何合并区域内，返回原始空 cell
        }

        // 递归查找指定类型的控件（解决控件嵌套在容器中找不到的问题）
        private T FindControl<T>(Control parent) where T : Control
        {
            if (parent == null) return null;
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is T result) return result;
                var found = FindControl<T>(ctrl);
                if (found != null) return found;
            }
            return null;
        }

        // 获取当前选中的 TabPage
        private Control GetCurrentActiveTabPage()
        {
            var xtraTabControl = FindControl<DevExpress.XtraTab.XtraTabControl>(this);
            if (xtraTabControl != null)
            {
                return xtraTabControl.SelectedTabPage;
            }
            return null;
        }

        // 将 importedData 绑定到指定页面中的 GridControl/DataGridView
        private void BindDataToCurrentGrid(Control activePage)
        {
            if (importedData == null || activePage == null) return;

            // 1. 尝试找 DevExpress 的 GridControl
            var gridControl = FindControl<DevExpress.XtraGrid.GridControl>(activePage);
            if (gridControl != null)
            {
                gridControl.DataSource = importedData;
                // 自动生成列（如果界面没有预先配置好列）
                var gridView = gridControl.MainView as DevExpress.XtraGrid.Views.Grid.GridView;
                gridView?.PopulateColumns();
                return;
            }

            // 2. 兼容找原生的 DataGridView
            var dgv = FindControl<DataGridView>(activePage);
            if (dgv != null)
            {
                dgv.DataSource = importedData;
            }
        }

        // 将信息进行显示 richTextBox1
        private void ShowMessage(string message, bool flag)
        {
            if (string.IsNullOrEmpty(richTextBox1.Text))
            {
                richTextBox1.Text = message + Environment.NewLine;
                richTextBox1.Select(0, message.Length + Environment.NewLine.Length);
            }
            else
            {
                this.richTextBox1.AppendText(message + "\n");
                int startpos = Math.Abs(this.richTextBox1.Text.Length - message.Length);
                this.richTextBox1.Select(startpos - 1, message.Length);
            }
            if (flag)
            {
                this.richTextBox1.SelectionColor = Color.Blue;
            }
            else
            {
                this.richTextBox1.SelectionColor = Color.Red;
            }
        }

        #endregion
    }
}
