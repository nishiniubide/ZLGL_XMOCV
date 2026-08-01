using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using ZLGL_XMOCV.entity;

namespace ZLGL_XMOCV.Converter
{
    /// <summary>
    /// ZC（制程参数）维度转换器
    /// </summary>
    public class ProcessConverter : DataConverterBase, IDataConverter
    {
        public ConvertResult Convert(DataTable dt)
        {
            var firstRow = dt.Rows[0];

            string factoryCode = SafeToStr(firstRow["供应商代码(必填)"]);
            string pushTime = SafeToStr(firstRow["请求推送的时间(必填)"]);
            int bizLine = SafeToInt32(firstRow["业务线(必填)"], 4);

            var dataList = ConvertToProcessDataList(dt);

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

        private static List<ProcessData> ConvertToProcessDataList(DataTable dt)
        {
            var list = new List<ProcessData>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ProcessData
                {
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
    }
}
