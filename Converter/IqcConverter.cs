using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using ZLGL_XMOCV.entity;

namespace ZLGL_XMOCV.Converter
{
    /// <summary>
    /// IQC（IQC检验信息）维度转换器
    /// </summary>
    public class IqcConverter : DataConverterBase, IDataConverter
    {
        public ConvertResult Convert(DataTable dt)
        {
            var firstRow = dt.Rows[0];

            string factoryCode = SafeToStr(firstRow["供应商代码(必填)"]);
            string pushTime = SafeToStr(firstRow["请求推送的时间(必填)"]);
            int bizLine = SafeToInt32(firstRow["业务线(必填)"], 4);

            var dataList = ConvertToIqcDataList(dt);

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

        private static List<IqcData> ConvertToIqcDataList(DataTable dt)
        {
            var list = new List<IqcData>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new IqcData
                {
                    InspectNo = SafeToStr(row["检验单号(必填)"]),
                    FieldType = SafeToStr(row["物料类型(必填)"]),
                    SupplierModel = SafeToStr(row["供应商型号(必填)"]),
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
    }
}
