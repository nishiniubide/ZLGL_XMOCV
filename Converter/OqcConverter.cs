using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using ZLGL_XMOCV.entity;

namespace ZLGL_XMOCV.Converter
{
    /// <summary>
    /// OQC（OQC批通率）维度转换器
    /// </summary>
    public class OqcConverter : DataConverterBase, IDataConverter
    {
        public ConvertResult Convert(DataTable dt)
        {
            DataRow firstRow = dt.Rows[0];

            string factoryCode = SafeToStr(firstRow["供应商代码(必填)"]);
            string pushTime = SafeToStr(firstRow["请求推送的时间(必填)"]);
            int bizLine = SafeToInt32(firstRow["业务线(必填)"], 4);

            var dataList = ConvertToOqcDataList(dt);

            var fullBody = new
            {
                factory_code = factoryCode,
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

        private static List<OqcData> ConvertToOqcDataList(DataTable dt)
        {
            var groups = dt.AsEnumerable().GroupBy(row =>
                $"{SafeToStr(row["供应商代码(必填)"])}_{SafeToStr(row["业务线(必填)"])}_" +
                $"{SafeToStr(row["领域(必填)"])}_{SafeToStr(row["供应商型号(必填)"])}_" +
                $"{SafeToStr(row["物料代码(必填)"])}_{SafeToStr(row["线体编号"])}_" +
                $"{SafeToStr(row["出货批次(必填)"])}_{SafeToStr(row["工站(必填)"])}_" +
                $"{SafeToDateKey(row["检验日期(必填)"])}_" +
                $"{SafeToStr(row["出货数量(必填)"])}_{SafeToStr(row["抽检批数(必填)"])}_" +
                $"{SafeToStr(row["合格批数(必填)"])}_{SafeToStr(row["抽检数量(必填)"])}_" +
                $"{SafeToStr(row["允收标准(必填)"])}");

            var result = new List<OqcData>();
            foreach (var group in groups)
            {
                var firstRow = group.First();
                var oqc = new OqcData
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
                        oqc.FailDetail.Add(new OqcDefect
                        {
                            FailName = failName,
                            FailQty = SafeToInt32(row["不良数量"])
                        });
                    }
                }
                result.Add(oqc);
            }
            return result;
        }
    }
}
