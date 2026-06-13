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

namespace ZLGL_XMOCV
{
    public partial class FrmZLGL_XMOCV : FrmBase
    {
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
            if (lkpUpProject.EditValue == null) return;

            string tabName = "Tab" + lkpUpProject.EditValue.ToString();

            var tabControl = this.Controls.OfType<TabControl>().FirstOrDefault();
            if (tabControl != null && tabControl.TabPages.ContainsKey(tabName))
            {
                tabControl.SelectedTab = tabControl.TabPages[tabName];
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            // 打开 excel 文件
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "EXCEL文件(*.xlsx)|*.xlsx|EXCEL文件(*.xls)|*.xls";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DataTable dt = ReadExcelWithNPOI(ofd.FileName, "0");

                }
                catch (Exception ex)
                {
                    Msg.ShowException(ex);
                }
                finally
                {

                }
            }
        }

        private void btnUpload_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        #region 导入Excel
        // 增加百分比数据格式转换
        private DataTable ReadExcelWithNPOI(string fileName, string sheetName)
        {
            try
            {
                IWorkbook workbook;
                // 非独占式访问
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (Path.GetExtension(fileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                        workbook = new XSSFWorkbook(fs); // 2007版本
                    else
                        workbook = new HSSFWorkbook(fs); // 2003版本
                }

                // 如果 sheetName 为空，获取第一个表 ；不为空，获取 sheetName
                ISheet sheet = string.IsNullOrEmpty(sheetName) ? workbook.GetSheetAt(0) : workbook.GetSheet(sheetName);

                // 如果 sheetName 不为空，但获取不到，获取第一个表
                if (sheet == null) { sheet = workbook.GetSheetAt(0); }
                ;

                DataTable dt = new DataTable();

                // 获取标题行内容
                // 标题行在第二行
                IRow headerRow = sheet.GetRow(1);

                // 创建列
                foreach (ICell cell in headerRow.Cells)
                {
                    dt.Columns.Add(cell.StringCellValue.Trim());
                }

                // 逐行读取数据，第三行
                for (int rowIndex = 2; rowIndex <= sheet.LastRowNum; rowIndex++)
                {

                    IRow row = sheet.GetRow(rowIndex);
                    if (row == null) { continue; }

                    // 按照行进行校验，如果此行全部为 null 或 空，则跳过。
                    // 有一个不为空，则继续。
                    bool isEmptyRow = true;
                    for (int i = 0; i < headerRow.Cells.Count; i++)
                    {
                        ICell cell = row.GetCell(i);
                        if (cell != null && !string.IsNullOrWhiteSpace(cell.ToString()))
                        {
                            isEmptyRow = false;
                            continue;
                        }
                    }
                    if (isEmptyRow) continue;

                    // 读取每一行的数据
                    DataRow dataRow = dt.NewRow();
                    for (int colIndex = 0; colIndex < headerRow.Cells.Count; colIndex++)
                    {
                        ICell cell = row.GetCell(colIndex);
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
        #endregion

    }
}
