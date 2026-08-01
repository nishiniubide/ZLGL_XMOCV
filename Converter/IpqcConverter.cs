using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using ZLGL_XMOCV.entity;

namespace ZLGL_XMOCV.Converter
{
    /// <summary>
    /// IPQC（IPQC检验信息）维度转换器
    /// </summary>
    public class IpqcConverter : DataConverterBase, IDataConverter
    {
        public ConvertResult Convert(DataTable dt)
        {
            var firstRow = dt.Rows[0];

            string factoryCode = SafeToStr(firstRow["供应商代码(必填)"]);
            string pushTime = SafeToStr(firstRow["请求推送的时间(必填)"]);
            int bizLine = SafeToInt32(firstRow["业务线(必填)"], 4);

            var dataList = ConvertToIpqcDataList(dt);

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

        private static List<IpqcData> ConvertToIpqcDataList(DataTable dt)
        {
            var list = new List<IpqcData>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new IpqcData
                {
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
                    InspectQty = SafeToInt32(row["检测数量(必填)"]),
                    SampleNo = SafeToStr(row["样品编号(必填)"]),
                    InspectValue = SafeToStr(row["检测值(必填)"]),
                    Unit = SafeToStr(row["单位"]),
                    UpperLevel = SafeToStr(row["上限"]),
                    FloorLevel = SafeToStr(row["下限"]),
                    InspectResult = SafeToStr(row["结果判定"]),
                    InspectTime = ConvertToDateString(row["检验日期(必填)"])
                });
            }
            return list;
        }
    }
}
