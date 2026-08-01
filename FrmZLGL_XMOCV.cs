using ERPWcfClient;
using ERPWcfClient.Common;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZLGL_XMOCV.Api;

using ZLGL_XMOCV.Excel;

namespace ZLGL_XMOCV
{
    /// <summary>
    /// 主窗体 —— 只负责 UI 交互，业务逻辑委托给 DataUploadService
    /// </summary>
    public partial class FrmZLGL_XMOCV : FrmBase
    {
        private readonly DataUploadService _uploadService;
        private DataTable importedData;
        private DataDimension currentDimension;

        public FrmZLGL_XMOCV()
        {
            InitializeComponent();

            // 注入依赖
            _uploadService = new DataUploadService(new NpoiExcelReader(), new X5ClientImpl());

            InitGridViews();
        }

        #region 初始化

        private void InitGridViews()
        {
            var gridViews = new[]
            {
                gridView1, gridView2, gridView3,
                gridView4, gridView5, gridView6
            };

            foreach (var gv in gridViews)
            {
                gv.OptionsBehavior.Editable = false;
                gv.OptionsView.ShowIndicator = false;
                gv.OptionsView.ColumnAutoWidth = false;
                gv.BestFitColumns();
            }
        }

        private void FrmZLGL_XMOCV_Load(object sender, EventArgs e)
        {
            Utils.LookUpDataBind("XiaomiUpProject", lkpUpProject);
        }

        #endregion

        #region 维度切换

        private void lkpUpProject_EditValueChanged(object sender, EventArgs e)
        {
            if (lkpUpProject.EditValue == null) return;

            importedData = null;

            string tabName = "Tab" + lkpUpProject.EditValue.ToString();
            var xtraTabControl = FindControl<DevExpress.XtraTab.XtraTabControl>(this);
            if (xtraTabControl == null) return;

            var page = xtraTabControl.TabPages
                .OfType<DevExpress.XtraTab.XtraTabPage>()
                .FirstOrDefault(p => p.Name == tabName);

            if (page == null) return;

            xtraTabControl.SelectedTabPage = page;
            ClearGrid(page);
        }

        private void ClearGrid(Control page)
        {
            var gridControl = FindControl<DevExpress.XtraGrid.GridControl>(page);
            if (gridControl != null)
            {
                gridControl.DataSource = null;
                (gridControl.MainView as DevExpress.XtraGrid.Views.Grid.GridView)?.Columns.Clear();
            }

            var dgv = FindControl<DataGridView>(page);
            if (dgv != null) dgv.DataSource = null;
        }

        #endregion

        #region 导入

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "EXCEL文件(*.xlsx)|*.xlsx|EXCEL文件(*.xls)|*.xls";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                DataDimension importDimension = ResolveCurrentDimension();

                if (importDimension.ToString() == "UNKNOWN")
                {
                    MessageBox.Show("无法识别的维度，请检查选择的项目。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 调用服务层：读取 + 校验
                var result = _uploadService.ImportAndValidate(ofd.FileName, importDimension);

                if (!result.Success)
                {
                    if (result.Validation != null && result.Validation.Errors.Count > 0)
                    {
                        string errorMsg = $"数据校验失败，共 {result.Validation.Errors.Count} 处错误：" + Environment.NewLine;
                        foreach (string err in result.Validation.Errors.Take(10))
                            errorMsg += err + Environment.NewLine;
                        if (result.Validation.Errors.Count > 10) errorMsg += "..." + Environment.NewLine;

                        ShowMessage(errorMsg, false);
                        Msg.ShowError("导入数据存在校验错误，请查看界面下方日志！");
                    }
                    else
                    {
                        ShowMessage($"导入失败：{result.ErrorMessage}", false);
                        Msg.ShowError(result.ErrorMessage);
                    }
                    return;
                }

                // 成功：绑定数据到界面
                importedData = result.Data;
                Control activePage = GetCurrentActiveTabPage();
                BindDataToGrid(activePage);
                ShowMessage($"导入成功，共 {result.RowCount} 行数据，且数据校验通过。", true);
            }
            catch (Exception ex)
            {
                Msg.ShowException(ex);
            }
        }

        #endregion

        #region 上传

        private void btnUpload_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (importedData == null || importedData.Rows.Count == 0)
            {
                Msg.ShowInformation("请先导入Excel数据");
                return;
            }

            DataDimension dimension = ResolveCurrentDimension();

            // 调用服务层：转换 + 上传
            var result = _uploadService.ConvertAndUpload(importedData, dimension);

            if (result.Success)
            {
                ShowMessage(result.Message, true);
            }
            else
            {
                ShowMessage(result.Message, false);
                if (!string.IsNullOrEmpty(result.RawContent))
                    Msg.ShowError($"服务器返回内容：\n{result.RawContent}");
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 从 LookUpEdit 解析当前选中的维度
        /// </summary>
        private DataDimension ResolveCurrentDimension()
        {
            object editVal = lkpUpProject.EditValue;

            if (editVal is DataDimension dim) return dim;
            if (editVal is int intVal) return (DataDimension)intVal;
            if (editVal != null && Enum.TryParse(editVal.ToString(), out DataDimension parsed)) return parsed;

            return DataDimension.UNKNOWN; // 默认值
        }

        private Control GetCurrentActiveTabPage()
        {
            var xtraTabControl = FindControl<DevExpress.XtraTab.XtraTabControl>(this);
            return xtraTabControl?.SelectedTabPage;
        }

        private void BindDataToGrid(Control activePage)
        {
            if (importedData == null || activePage == null) return;

            var gridControl = FindControl<DevExpress.XtraGrid.GridControl>(activePage);
            if (gridControl != null)
            {
                gridControl.DataSource = importedData;
                (gridControl.MainView as DevExpress.XtraGrid.Views.Grid.GridView)?.PopulateColumns();
                return;
            }

            var dgv = FindControl<DataGridView>(activePage);
            if (dgv != null) dgv.DataSource = importedData;
        }

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

        private void ShowMessage(string message, bool isSuccess)
        {
            if (string.IsNullOrEmpty(richTextBox1.Text))
            {
                richTextBox1.Text = message + Environment.NewLine;
                richTextBox1.Select(0, message.Length + Environment.NewLine.Length);
            }
            else
            {
                richTextBox1.AppendText(message + "\n");
                int startpos = Math.Abs(richTextBox1.Text.Length - message.Length);
                richTextBox1.Select(startpos - 1, message.Length);
            }
            richTextBox1.SelectionColor = isSuccess ? Color.Blue : Color.Red;
        }

        #endregion
    }
}
