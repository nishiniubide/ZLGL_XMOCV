using System.Data;


namespace ZLGL_XMOCV.Excel
{
    /// <summary>
    /// Excel 读取抽象接口，隔离 NPOI 依赖
    /// </summary>
    public interface IExcelReader
    {
        /// <summary>
        /// 读取 Excel 文件并返回 DataTable
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="sheetName">工作表名（null 则取第一个）</param>
        /// <param name="dimension">数据维度，用于表头识别策略</param>
        DataTable Read(string filePath, string sheetName, DataDimension dimension);
    }
}
