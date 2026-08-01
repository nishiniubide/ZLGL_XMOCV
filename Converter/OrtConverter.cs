using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using ZLGL_XMOCV.entity;

namespace ZLGL_XMOCV.Converter
{
    /// <summary>
    /// ORT（ORT检验数据）维度转换器
    /// </summary>
    public class OrtConverter : DataConverterBase, IDataConverter
    {
        public ConvertResult Convert(DataTable dt)
        {
            DataRow firstRow = dt.Rows[0];

            string factoryCode = SafeToStr(firstRow["供应商代码(必填)"]);
            string pushTime = SafeToStr(firstRow["请求推送的时间(必填)"]);
            int bizLine = SafeToInt32(firstRow["业务线(必填)"], 4);

            var dataList = ConvertToOrtDataList(dt);

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

        private static List<OrtData> ConvertToOrtDataList(DataTable dt)
        {
            var list = new List<OrtData>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new OrtData
                {
                    FieldType = SafeToStr(row["领域(必填)"]),
                    SupplierModel = SafeToStr(row["供应商型号(必填)"]),
                    ProductCode = SafeToStr(row["物料代码(必填)"]),
                    ProductBatch = SafeToStr(row["产品批次(必填)"]),
                    MonitoringMonth = SafeToStr(row["ORT监控截止月份(必填)"]),
                    ProductionDate = ConvertToDateString(row["样品生产时间(必填)"]),
                    TestItem = SafeToStr(row["测试项目(必填)"]),
                    CheckDate = ConvertToDateString(row["检验日期(必填)"]),
                    InputQty = SafeToInt32(row["投入数量(必填)"]),
                    OkQty = SafeToInt32(row["测试总通过数(必填)"]),
                    TestingQty = SafeToInt32(row["测试中数量(必填)"]),
                    NgQty = SafeToInt32(row["测试总NG数(必填)"]),
                    Result = SafeToInt32(row["结果判定:1PASS/2FAIL(必填)"])
                });
            }
            return list;
        }
    }
}
