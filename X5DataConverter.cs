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
        LL,
        ZC,
        OQC,
        ORT,
        IQC,
        IPQC
    }

    public class X5RequestPackage
    {
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

            DataRow firstRow = dt.Rows[0];

            // 1. 先提取外层公共参数
            string factoryCode = "";
            string pushTime = "";
            int bizLine = 4;
            int supplier_type = 0;

            
            factoryCode = SafeToStr(firstRow["供应商代码(必填)"]);
            pushTime = SafeToStr(firstRow["请求推送的时间(必填)"]);
            bizLine = SafeToInt32(firstRow["业务线(必填)"], 4);
            

            if (dimension == DataDimension.LL)
            {
                supplier_type = SafeToInt32(firstRow["供应商类型(必填)"], 1);
            }

            // 根据维度转换 data 列表
            List<object> dataList;
            switch (dimension)
            {
                case DataDimension.LL:
                    dataList = ConvertToYieldDataList(dt).Cast<object>().ToList();
                    break;
                case DataDimension.ZC:
                    dataList = ConvertToProcessDataList(dt).Cast<object>().ToList();
                    break;
                case DataDimension.OQC:
                    dataList = ConvertToOqcDataList(dt).Cast<object>().ToList();
                    break;
                case DataDimension.ORT:
                    dataList = ConvertToOrtDataList(dt).Cast<object>().ToList();
                    break;
                case DataDimension.IQC:
                    dataList = ConvertToIqcDataList(dt).Cast<object>().ToList();
                    break;
                case DataDimension.IPQC:
                    dataList = ConvertToIpqcDataList(dt).Cast<object>().ToList();
                    break;
                default:
                    throw new ArgumentException("不支持的维度");
            }

            // 3. 构建完整的请求体对象（包含外层参数和内层 data）
            if (dimension == DataDimension.LL)
            {
                var fullBodyObject = new
                {
                    factory_code = factoryCode,
                    supplier_type = supplier_type,
                    businessline_id = bizLine,
                    data_lines = dataList.Count,
                    request_time = ConvertToDateTimeString(pushTime), // 确保时间格式正确
                    data = dataList
                };

                // 4. 序列化完整的对象
                package.BodyJson = JsonConvert.SerializeObject(fullBodyObject, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateFormatString = "yyyy-MM-dd HH:mm:ss"
                });
            }
            else
            {
                var fullBodyObject = new
                {
                    factory_code = factoryCode,
                    businessline_id = bizLine,
                    data_lines = dataList.Count,
                    request_time = ConvertToDateTimeString(pushTime), // 确保时间格式正确
                    data = dataList
                };

                // 4. 序列化完整的对象
                package.BodyJson = JsonConvert.SerializeObject(fullBodyObject, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateFormatString = "yyyy-MM-dd HH:mm:ss"
                });
            }

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
                    // ✅ 业务字段（全部使用安全转换）
                    KeyCode = SafeToStr(firstRow["接口主键(必填)"]),
                    CheckType = SafeToIntNullable(firstRow["检验方式"]),
                    MaterialType = SafeToStr(firstRow["物料类型(必填)"]),
                    FieldType = SafeToStr(firstRow["领域(必填)"]),
                    SupplierModel = SafeToStr(firstRow["供应商型号(必填)"]),
                    ProductCode = SafeToStr(firstRow["物料编码(必填)"]),
                    ProductBatch = SafeToStr(firstRow["产品批次(必填)"]),
                    Process = SafeToStr(firstRow["制程(必填)"]),
                    LineId = SafeToStr(firstRow["线体编号(必填)"]),
                    StartTime = ConvertToDateTimeString(firstRow["良率统计开始时间(必填)"]),
                    EndTime = ConvertToDateTimeString(firstRow["良率统计结束时间(必填)"]),
                    DowntimeDetail = SafeToStr(firstRow["停机明细"]),
                    QtyInput = SafeToInt32(firstRow["工站投入(必填)"]),
                    QtyOutput = SafeToInt32(firstRow["工站产出(必填)"]),
                    ActualFpy = SafeToDecimalNullable(firstRow["实际一次良率"]),
                    TargetFpy = SafeToDecimalNullable(firstRow["一次良率目标"]),
                    ActualPy = SafeToDecimal(firstRow["实际最终良率(必填)"]),
                    TargetPy = SafeToDecimal(firstRow["最终良率目标(必填)"]),
                    QtyDefect = SafeToInt32(firstRow["不良数量(必填)"]),
                    QtyScrap = SafeToInt32(firstRow["报废数量(必填)"]),
                    YieldType = SafeToIntNullable(firstRow["不良类型"]),
                    InsCls = SafeToStr(firstRow["不良分类"]),
                    InsItm = SafeToStr(firstRow["不良详细描述"]),
                    QtyIns = SafeToIntNullable(firstRow["检验数"]),
                    QtyDef = SafeToIntNullable(firstRow["不良数"])
                };
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
                    // ✅ 业务字段（全部使用安全转换，避免 DBNull 或格式错误导致崩溃）
                    MaterialType = SafeToStr(row["物料类型(必填)"]),
                    SupplierModel = SafeToStr(row["供应商型号(必填)"]),
                    ProductCode = SafeToStr(row["物料代码(必填)"]),
                    ProductBatch = SafeToStr(row["产品批次(必填)"]),
                    Process = SafeToStr(row["制程(必填)"]),
                    LineId = SafeToStr(row["线体编号(必填)"]),
                    Sn = SafeToStr(row["SN码(必填)"]),
                    MachineName = SafeToStr(row["设备名称"]),
                    Parameter = SafeToStr(row["参数名(必填)"]),
                    KeyValue = SafeToStr(row["参数值(必填)"]),
                    Unit = SafeToStr(row["单位"]),
                    UpperLevel = SafeToStr(row["上限"]),
                    FloorLevel = SafeToStr(row["下限"]),
                    Result = SafeToStr(row["结果判定"]),
                    OpTime = ConvertToDateTimeString(row["工厂数据采集的时间(必填)"])
                });
            }
            return list;
        }

        private static List<OqcData> ConvertToOqcDataList(DataTable dt)
        {
            // ✅ 关键修复：分组键必须包含所有非不良明细字段！
            // 原来只包含 供应商代码_业务线_领域_型号_料号_工站_出货批次_检验日期
            // 这些字段在两个合并区域中完全相同，导致5行被错误合并为1组
            // 
            // 新增: 线体编号_出货数量_抽检批数_合格批数_抽检数量_允收标准
            // 这些字段在两个合并区域中不同(133600 vs 500)，从而正确分为2组
            var groups = dt.AsEnumerable().GroupBy(row =>
                $"{SafeToStr(row["供应商代码(必填)"])}_{SafeToStr(row["业务线(必填)"])}_" +
                $"{SafeToStr(row["领域(必填)"])}_{SafeToStr(row["供应商型号(必填)"])}_" +
                $"{SafeToStr(row["物料代码(必填)"])}_{SafeToStr(row["线体编号"])}_" +
                $"{SafeToStr(row["出货批次(必填)"])}_{SafeToStr(row["工站(必填)"])}_" +
                $"{SafeToDateKey(row["检验日期(必填)"])}_" +
                // ✅ 以下为新增的分组字段 —— 区分不同合并区域的关键
                $"{SafeToStr(row["出货数量(必填)"])}_{SafeToStr(row["抽检批数(必填)"])}_" +
                $"{SafeToStr(row["合格批数(必填)"])}_{SafeToStr(row["抽检数量(必填)"])}_" +
                $"{SafeToStr(row["允收标准(必填)"])}");

            var result = new List<OqcData>();
            foreach (var group in groups)
            {
                var firstRow = group.First();

                var OQC = new OqcData
                {
                    ProductCode = SafeToStr(firstRow["物料代码(必填)"]),
                    FieldType = SafeToStr(firstRow["领域(必填)"]),
                    SupplierModel = SafeToStr(firstRow["供应商型号(必填)"]),
                    LineId = SafeToStr(firstRow["线体编号"]),
                    ShipmentBatch = SafeToStr(firstRow["出货批次(必填)"]),
                    Site = SafeToStr(firstRow["工站(必填)"]),
                    CheckDate = ConvertToDateString(firstRow["检验日期(必填)"]),
                    ShipmentQty = SafeToInt32(firstRow["出货数量(必填)"]),
                    CheckBatchQty = SafeToInt32(firstRow["抽检批数(必填)"]),
                    PassBatchQty = SafeToInt32(firstRow["合格批数(必填)"]),
                    CheckQty = SafeToInt32(firstRow["抽检数量(必填)"]),
                    AcceptStandard = SafeToStr(firstRow["允收标准(必填)"]),
                    FailDetail = new List<OqcDefect>()
                };

                foreach (DataRow row in group)
                {
                    string failName = SafeToStr(row["不良名称"]);
                    if (!string.IsNullOrWhiteSpace(failName))
                    {
                        OQC.FailDetail.Add(new OqcDefect
                        {
                            FailName = failName,
                            FailQty = SafeToInt32(row["不良数量"])
                        });
                    }
                }
                result.Add(OQC);
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
                    // ✅ 业务字段（全部使用安全转换，避免DBNull或格式错误导致崩溃）
                    FieldType = SafeToStr(row["领域(必填)"]),
                    SupplierModel = SafeToStr(row["供应商型号(必填)"]),
                    ProductCode = SafeToStr(row["物料代码(必填)"]),
                    ProductBatch = SafeToStr(row["产品批次(必填)"]),
                    MonitoringMonth = SafeToStr(row["ORT监控截止月份(必填)"]),
                    ProductionDate = ConvertToDateString(row["样品生产时间(必填)"]),
                    TestItem = SafeToStr(row["测试项目(必填)"]),
                    CheckDate = ConvertToDateString(row["检验日期(必填)"]),
                    // SafeToInt32 自带提取数字和默认值机制，即使填了 "3个" 也能提取出 3
                    InputQty = SafeToInt32(row["投入数量(必填)"]),
                    OkQty = SafeToInt32(row["测试总通过数(必填)"]),
                    TestingQty = SafeToInt32(row["测试中数量(必填)"]),
                    NgQty = SafeToInt32(row["测试总NG数(必填)"]),
                    //ProgressDesc = SafeToStr(row["测试进度描述"]), TODO:未找到应用
                    Result = SafeToInt32(row["结果判定:1PASS/2FAIL(必填)"])
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
                    // ✅ 业务字段（全部使用安全转换，避免 DBNull 或格式错误导致崩溃）
                    InspectNo = SafeToStr(row["检验单号(必填)"]),
                    FieldType = SafeToStr(row["物料类型(必填)"]),
                    SupplierModel = SafeToStr(row["供应商型号(必填)"]),
                    // 注意：根据Excel模板，这里的列名没有"(必填)"后缀
                    ProductCode = SafeToStr(row["物料代码"]),
                    ProductBatch = SafeToStr(row["产品批次"]),
                    MaterialNo = SafeToStr(row["原材料料号(必填)"]),
                    MaterialName = SafeToStr(row["原材料名称(必填)"]),
                    VendorName = SafeToStr(row["原材料供应商名称(必填)"]),
                    IncomingBatch = SafeToStr(row["来料批次(必填)"]),
                    DateCode = ConvertToDateString(row["原材料生产日期(必填)"]),
                    ValidityPeriod = SafeToDoubleNullable(row["有效期"]),
                    InspectItem = SafeToStr(row["检验项(必填)"]),
                    InspectValue = SafeToStr(row["检测值(必填)"]),
                    Unit = SafeToStr(row["单位"]),
                    UpperLevel = SafeToStr(row["上限"]),
                    FloorLevel = SafeToStr(row["下限"]),
                    Result = SafeToStr(row["结果判定"]),
                    DealMethod = SafeToStr(row["不良处理方式"]),
                    InspectTime = ConvertToDateString(row["检验日期(必填)"])
                });
            }
            return list;
        }

        private static List<IpqcData> ConvertToIpqcDataList(DataTable dt)
        {
            var list = new List<IpqcData>();

            // 遍历每一行，逐行读取公共字段和业务字段
            foreach (DataRow row in dt.Rows)
            {
                var item = new IpqcData
                {
                    // ✅ 逐行设定业务字段
                    InspectNo = SafeToStr(row["检验单号(必填)"]),
                    MaterialType = SafeToStr(row["物料类型(必填)"]),
                    Model = SafeToStr(row["供应商型号(必填)"]),
                    ProductCode = SafeToStr(row["物料代码(必填)"]),
                    ProductBatch = SafeToStr(row["产品批次(必填)"]),
                    Process = SafeToStr(row["制程(必填)"]),
                    LineId = SafeToStr(row["线体编号(必填)"]),
                    ProdStartTime = ConvertToDateTimeString(row["生产开始时间(必填)"]),
                    ProdEndTime = ConvertToDateTimeString(row["生产结束时间(必填)"]),
                    SampFreq = SafeToStr(row["抽检频次(必填)"]),
                    InspectItem = SafeToStr(row["检验项(必填)"]),
                    InspectTool = SafeToStr(row["检测工具"]),
                    // SafeToInt32 会自动把 "250g" 提取出 250
                    InspectQty = SafeToInt32(row["检测数量(必填)"]),
                    SampleNo = SafeToStr(row["样品编号(必填)"]),
                    InspectValue = SafeToStr(row["检测值(必填)"]),
                    Unit = SafeToStr(row["单位"]),
                    UpperLevel = SafeToStr(row["上限"]),
                    FloorLevel = SafeToStr(row["下限"]),
                    InspectResult = SafeToStr(row["结果判定"]),
                    InspectTime = ConvertToDateString(row["检验日期(必填)"])
                };

                list.Add(item);
            }
            return list;
        }

        #endregion

        #region 辅助方法

        private static string ConvertToDateTimeString(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            if (value is DateTime dt) return dt.ToString("yyyy-MM-dd HH:mm:ss");

            string s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return null;

            if (DateTime.TryParse(s, out DateTime parsed)) return parsed.ToString("yyyy-MM-dd HH:mm:ss");

            // ✅ 处理 Excel 日期序列号
            if (double.TryParse(s, out double oaDate) && oaDate >= 1 && oaDate <= 73415)
            {
                try
                {
                    DateTime d = DateTime.FromOADate(oaDate);
                    if (d.Year >= 2000 && d.Year <= 2099) return d.ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch { }
            }

            return s;
        }

        private static string ConvertToDateString(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            if (value is DateTime dt) return dt.ToString("yyyy-MM-dd");

            string s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return null;

            if (DateTime.TryParse(s, out DateTime parsed)) return parsed.ToString("yyyy-MM-dd");

            // ✅ 处理 Excel 日期序列号 (如 45690 → 2024-12-15)
            if (double.TryParse(s, out double oaDate) && oaDate >= 1 && oaDate <= 73415)
            {
                try
                {
                    DateTime d = DateTime.FromOADate(oaDate);
                    if (d.Year >= 2000 && d.Year <= 2099) return d.ToString("yyyy-MM-dd");
                }
                catch { }
            }

            return s;
        }

        private static string SafeToStr(object value, string defaultValue = "")
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            return value.ToString().Trim();
        }

        private static int SafeToInt32(object value, int defaultValue = 0)
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            string s = value.ToString().Trim();
            if (int.TryParse(s, out int result)) return result;
            if (double.TryParse(s, out double d)) return (int)d;

            // ✅ 新增：从带单位的字符串中提取数字 (如 "250g"→250, "1pcs"→1, "11pcs"→11)
            var match = System.Text.RegularExpressions.Regex.Match(s, @"\d+");
            if (match.Success) return int.Parse(match.Value);

            return defaultValue;
        }

        private static string SafeToDateKey(object value)
        {
            if (value == null || value == DBNull.Value) return "";
            if (value is DateTime dt) return dt.ToString("yyyy-MM-dd");
            string s = value.ToString().Trim();
            if (DateTime.TryParse(s, out DateTime parsed)) return parsed.ToString("yyyy-MM-dd");
            if (double.TryParse(s, out double oaDate) && oaDate >= 1 && oaDate <= 73415)
            {
                try
                {
                    DateTime d = DateTime.FromOADate(oaDate);
                    if (d.Year >= 2000 && d.Year <= 2099) return d.ToString("yyyy-MM-dd");
                }
                catch { }
            }
            return s;
        }

        private static decimal SafeToDecimal(object value, decimal defaultValue = 0m)
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            string s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return defaultValue;
            if (decimal.TryParse(s, out decimal result)) return result;
            return defaultValue;
        }

        private static decimal? SafeToDecimalNullable(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            string s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return null;
            if (decimal.TryParse(s, out decimal result)) return result;
            return null;
        }

        private static int? SafeToIntNullable(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            string s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return null;
            if (int.TryParse(s, out int result)) return result;
            if (double.TryParse(s, out double d)) return (int)d;
            return null;
        }

        private static double? SafeToDoubleNullable(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            string s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return null;
            if (double.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double result))
                return result;
            return null;
        }


        #endregion
    }
}