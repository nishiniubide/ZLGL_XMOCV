using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using ZLGL_XMOCV.entity;

namespace ZLGL_XMOCV
{
    public enum DataDimension
    {
        Yield,
        Process,
        Oqc,
        Ort,
        Iqc,
        Ipqc
    }

    public class X5RequestPackage
    {
        public string FactoryCode { get; set; }      // 供应商代码
        public int BusinessLineId { get; set; }      // 业务线，固定4
        public int DataLines { get; set; }           // 数据条数（data数组长度）
        public string PushTime { get; set; }         // 请求推送时间
        public string BodyJson { get; set; }         // data序列化后的JSON字符串
    }

    public static class X5DataConverter
    {
        /// <summary>
        /// 从DataTable构建完整的X5请求包（含外层参数和body json）
        /// </summary>
        public static X5RequestPackage BuildRequestPackage(DataTable dt, DataDimension dimension)
        {
            var package = new X5RequestPackage();

            // 从第一行提取公共参数（所有行应相同）
            if (dt.Rows.Count == 0) return null;

            DataRow firstRow = dt.Rows[0];
            package.FactoryCode = firstRow["供应商代码(必填)"]?.ToString() ?? firstRow["工厂编码(必填)"]?.ToString();
            package.BusinessLineId = Convert.ToInt32(firstRow["业务线(必填)"] ?? "4");
            package.PushTime = ConvertToDateTimeString(firstRow["请求推送的时间(必填)"] ?? firstRow["数据推送时间(必填)"]);

            // 根据维度转换 data 列表
            List<object> dataList;
            switch (dimension)
            {
                case DataDimension.Yield:
                    dataList = ConvertToYieldDataList(dt).Cast<object>().ToList();
                    break;
                case DataDimension.Process:
                    dataList = ConvertToProcessDataList(dt).Cast<object>().ToList();
                    break;
                case DataDimension.Oqc:
                    dataList = ConvertToOqcDataList(dt).Cast<object>().ToList();
                    break;
                case DataDimension.Ort:
                    dataList = ConvertToOrtDataList(dt).Cast<object>().ToList();
                    break;
                case DataDimension.Iqc:
                    dataList = ConvertToIqcDataList(dt).Cast<object>().ToList();
                    break;
                case DataDimension.Ipqc:
                    dataList = ConvertToIpqcDataList(dt).Cast<object>().ToList();
                    break;
                default:
                    throw new ArgumentException("不支持的维度");
            }

            package.DataLines = dataList.Count;
            package.BodyJson = JsonConvert.SerializeObject(dataList, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy-MM-dd HH:mm:ss"
            });
            return package;
        }

        #region 各维度转换（内层实体，不含外层公共字段）

        private static List<YieldData> ConvertToYieldDataList(DataTable dt)
        {
            // 良率表：以“接口主键”分组，将多行不良合并到 DefectList
            var groups = dt.AsEnumerable().GroupBy(row => row["接口主键(必填)"].ToString());
            var result = new List<YieldData>();

            foreach (var group in groups)
            {
                var firstRow = group.First();
                var yield = new YieldData
                {
                    KeyCode = firstRow["接口主键(必填)"].ToString(),
                    CheckType = firstRow["检验方式"] == DBNull.Value ? (int?)null : Convert.ToInt32(firstRow["检验方式"]),
                    MaterialType = firstRow["物料类型(必填)"].ToString(),
                    FieldType = firstRow["领域(必填)"].ToString(),
                    SupplierModel = firstRow["供应商型号(必填)"].ToString(),
                    ProductCode = firstRow["物料编码(必填)"].ToString(),
                    ProductBatch = firstRow["产品批次(必填)"].ToString(),
                    Process = firstRow["制程(必填)"].ToString(),
                    LineId = firstRow["线体编号(必填)"].ToString(),
                    StartTime = ConvertToDateTimeString(firstRow["良率统计开始时间(必填)"]),
                    EndTime = ConvertToDateTimeString(firstRow["良率统计结束时间(必填)"]),
                    DowntimeDetail = firstRow["停机明细"].ToString(),
                    QtyInput = Convert.ToInt32(firstRow["工站投入(必填)"]),
                    QtyOutput = Convert.ToInt32(firstRow["工站产出(必填)"]),
                    ActualFpy = firstRow["实际一次良率"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(firstRow["实际一次良率"]),
                    TargetFpy = firstRow["一次良率目标"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(firstRow["一次良率目标"]),
                    ActualPy = Convert.ToDecimal(firstRow["实际最终良率(必填)"]),
                    TargetPy = Convert.ToDecimal(firstRow["最终良率目标(必填)"]),
                    QtyDefect = Convert.ToInt32(firstRow["不良数量(必填)"]),
                    QtyScrap = Convert.ToInt32(firstRow["报废数量(必填)"]),
                    DefectList = new List<YieldDefect>()
                };

                foreach (DataRow row in group)
                {
                    if (row["不良类型"] != DBNull.Value || row["不良分类"] != DBNull.Value)
                    {
                        yield.DefectList.Add(new YieldDefect
                        {
                            YieldType = row["不良类型"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["不良类型"]),
                            InsCls = row["不良分类"].ToString(),
                            InsItm = row["不良详细描述"].ToString(),
                            QtyIns = row["检验数"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["检验数"]),
                            QtyDef = row["不良数"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["不良数"])
                        });
                    }
                }
                result.Add(yield);
            }
            return result;
        }

        private static List<ProcessData> ConvertToProcessDataList(DataTable dt)
        {
            var list = new List<ProcessData>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ProcessData
                {
                    MaterialType = row["物料类型(必填)"].ToString(),
                    SupplierModel = row["供应商型号(必填)"].ToString(),
                    ProductCode = row["物料代码(必填)"].ToString(),
                    ProductBatch = row["产品批次(必填)"].ToString(),
                    Process = row["制程(必填)"].ToString(),
                    LineId = row["线体编号(必填)"].ToString(),
                    Sn = row["SN码(必填)"].ToString(),
                    MachineName = row["设备名称"].ToString(),
                    KeyValue = row["参数名(必填)"].ToString(),
                    Value = row["参数值(必填)"].ToString(),
                    Unit = row["单位"].ToString(),
                    UpperLevel = row["上限"].ToString(),
                    FloorLevel = row["下限"].ToString(),
                    Result = row["结果判定"].ToString(),
                    OpTime = ConvertToDateTimeString(row["工厂数据采集的时间(必填)"])
                });
            }
            return list;
        }

        private static List<OqcData> ConvertToOqcDataList(DataTable dt)
        {
            // OQC 唯一键组合（供应商代码+业务线+领域+型号+料号+工站+出货批次+检验日期）
            var groups = dt.AsEnumerable().GroupBy(row =>
                $"{row["供应商代码(必填)"]}_{row["业务线(必填)"]}_{row["领域(必填)"]}_{row["供应商型号(必填)"]}_{row["物料代码(必填)"]}_{row["工站(必填)"]}_{row["出货批次(必填)"]}_{Convert.ToDateTime(row["检验日期(必填)"]):yyyy-MM-dd}");
            var result = new List<OqcData>();

            foreach (var group in groups)
            {
                var firstRow = group.First();
                var oqc = new OqcData
                {
                    ProductCode = firstRow["物料代码(必填)"].ToString(),
                    FieldType = firstRow["领域(必填)"].ToString(),
                    SupplierModel = firstRow["供应商型号(必填)"].ToString(),
                    LineId = firstRow["线体编号"].ToString(),
                    ShipmentBatch = firstRow["出货批次(必填)"].ToString(),
                    SiteName = firstRow["工站(必填)"].ToString(),
                    CheckDate = ConvertToDateString(firstRow["检验日期(必填)"]),
                    ShipmentQty = Convert.ToInt32(firstRow["出货数量(必填)"]),
                    CheckBatchQty = Convert.ToInt32(firstRow["抽检批数(必填)"]),
                    PassBatchQty = Convert.ToInt32(firstRow["合格批数(必填)"]),
                    CheckQty = Convert.ToInt32(firstRow["抽检数量(必填)"]),
                    AcceptStandard = firstRow["允收标准(必填)"].ToString(),
                    FailDetail = new List<OqcDefect>()
                };
                foreach (DataRow row in group)
                {
                    if (row["不良名称"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["不良名称"].ToString()))
                    {
                        oqc.FailDetail.Add(new OqcDefect
                        {
                            FailName = row["不良名称"].ToString(),
                            FailQty = Convert.ToInt32(row["不良数量"])
                        });
                    }
                }
                result.Add(oqc);
            }
            return result;
        }

        private static List<OrtData> ConvertToOrtDataList(DataTable dt)
        {
            var list = new List<OrtData>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new OrtData
                {
                    FieldType = row["领域(必填)"].ToString(),
                    SupplierModel = row["供应商型号(必填)"].ToString(),
                    ProductCode = row["物料代码(必填)"].ToString(),
                    ProductBatch = row["产品批次(必填)"].ToString(),
                    MonitoringMonth = row["ORT监控截止月份(必填)"].ToString(),
                    ProductionDate = ConvertToDateString(row["样品生产时间(必填)"]),
                    TestItem = row["测试项目(必填)"].ToString(),
                    TestDate = ConvertToDateString(row["检验日期(必填)"]),
                    InputQty = Convert.ToInt32(row["投入数量(必填)"]),
                    OkQty = Convert.ToInt32(row["测试总通过数(必填)"]),
                    TestingQty = Convert.ToInt32(row["测试中数量(必填)"]),
                    NgQty = Convert.ToInt32(row["测试总NG数(必填)"]),
                    ProgressDesc = row["测试进度描述"].ToString(),
                    Result = Convert.ToInt32(row["结果判定:1PASS/2FAIL(必填)"])
                });
            }
            return list;
        }

        private static List<IqcData> ConvertToIqcDataList(DataTable dt)
        {
            var list = new List<IqcData>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new IqcData
                {
                    InspectNo = row["检验单号(必填)"].ToString(),
                    MaterialType = row["物料类型(必填)"].ToString(),
                    SupplierModel = row["供应商型号(必填)"].ToString(),
                    ProductCode = row["物料代码"].ToString(),
                    ProductBatch = row["产品批次"].ToString(),
                    MaterialNo = row["原材料料号(必填)"].ToString(),
                    MaterialName = row["原材料名称(必填)"].ToString(),
                    VendorName = row["原材料供应商名称(必填)"].ToString(),
                    IncomingBatch = row["来料批次(必填)"].ToString(),
                    DateCode = ConvertToDateString(row["原材料生产日期(必填)"]),
                    ValidityPeriod = row["有效期"].ToString(),
                    InspectItem = row["检验项(必填)"].ToString(),
                    InspectValue = row["检测值(必填)"].ToString(),
                    Unit = row["单位"].ToString(),
                    UpperLevel = row["上限"].ToString(),
                    FloorLevel = row["下限"].ToString(),
                    Result = row["结果判定"].ToString(),
                    DisposalMethod = row["不良处理方式"].ToString(),
                    InspectDate = ConvertToDateString(row["检验日期(必填)"])
                });
            }
            return list;
        }

        private static List<IpqcData> ConvertToIpqcDataList(DataTable dt)
        {
            var list = new List<IpqcData>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new IpqcData
                {
                    InspectNo = row["检验单号(必填)"].ToString(),
                    MaterialType = row["物料类型(必填)"].ToString(),
                    SupplierModel = row["供应商型号(必填)"].ToString(),
                    ProductCode = row["物料代码(必填)"].ToString(),
                    ProductBatch = row["产品批次(必填)"].ToString(),
                    Process = row["制程(必填)"].ToString(),
                    LineId = row["线体编号(必填)"].ToString(),
                    ProdStartTime = ConvertToDateTimeString(row["生产开始时间(必填)"]),
                    ProdEndTime = ConvertToDateTimeString(row["生产结束时间(必填)"]),
                    SampFreq = row["抽检频次(必填)"].ToString(),
                    InspectItem = row["检验项(必填)"].ToString(),
                    InspectTool = row["检测工具"].ToString(),
                    InspectQty = Convert.ToInt32(row["检测数量(必填)"]),
                    SampleNo = row["样品编号(必填)"].ToString(),
                    InspectValue = row["检测值(必填)"].ToString(),
                    Unit = row["单位"].ToString(),
                    UpperLevel = row["上限"].ToString(),
                    FloorLevel = row["下限"].ToString(),
                    InspectResult = row["结果判定"].ToString(),
                    InspectTime = ConvertToDateString(row["检验日期(必填)"])
                });
            }
            return list;
        }

        #endregion

        #region 辅助方法

        private static string ConvertToDateTimeString(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            if (value is DateTime dt) return dt.ToString("yyyy-MM-dd HH:mm:ss");
            if (DateTime.TryParse(value.ToString(), out dt)) return dt.ToString("yyyy-MM-dd HH:mm:ss");
            return value.ToString();
        }

        private static string ConvertToDateString(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            if (value is DateTime dt) return dt.ToString("yyyy-MM-dd");
            if (DateTime.TryParse(value.ToString(), out dt)) return dt.ToString("yyyy-MM-dd");
            return value.ToString();
        }

        #endregion
    }
}