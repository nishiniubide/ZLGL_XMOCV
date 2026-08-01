using System;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using ZLGL_XMOCV.entity;

namespace ZLGL_XMOCV.Converter
{
    /// <summary>
    /// LL（工站良率）维度转换器
    /// </summary>
    public class YieldConverter : DataConverterBase, IDataConverter
    {
        public ConvertResult Convert(DataTable dt)
        {
            DataRow firstRow = dt.Rows[0];

            string factoryCode = SafeToStr(firstRow["供应商代码(必填)"]);
            string pushTime = SafeToStr(firstRow["请求推送的时间(必填)"]);
            int bizLine = SafeToInt32(firstRow["业务线(必填)"], 4);
            int supplierType = SafeToInt32(firstRow["供应商类型(必填)"], 1);

            var dataList = ConvertToYieldDataList(dt);

            var fullBody = new
            {
                factory_code = factoryCode,
                supplier_type = supplierType,
                businessline_id = bizLine,
                data_lines = dataList.Count,
                request_time = ConvertToDateTimeString(pushTime),
                data = dataList
            };

            string bodyJson = JsonConvert.SerializeObject(fullBody, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy-MM-dd HH:mm:ss"
            });

            return new ConvertResult { BodyJson = bodyJson };
        }

        private static System.Collections.Generic.List<YieldData> ConvertToYieldDataList(DataTable dt)
        {
            var groups = dt.AsEnumerable().GroupBy(row => row["接口主键(必填)"].ToString());
            var result = new System.Collections.Generic.List<YieldData>();

            foreach (var group in groups)
            {
                var firstRow = group.First();
                var yield = new YieldData
                {
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
    }
}
