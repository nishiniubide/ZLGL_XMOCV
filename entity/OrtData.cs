using Newtonsoft.Json;

namespace ZLGL_XMOCV.entity
{
    public class OrtData
    {
        [JsonProperty("request_time")]
        public string RequestTime { get; set; }

        [JsonProperty("factory_code")]
        public string FactoryCode { get; set; }

        [JsonProperty("businessline_id")]
        public int BusinessLineId { get; set; }

        [JsonProperty("field_type")]
        public string FieldType { get; set; }

        [JsonProperty("supplier_model")]
        public string SupplierModel { get; set; }

        [JsonProperty("product_code")]
        public string ProductCode { get; set; }

        [JsonProperty("product_batch")]
        public string ProductBatch { get; set; }

        [JsonProperty("monitoring_month")]
        public string MonitoringMonth { get; set; }

        [JsonProperty("production_date")]
        public string ProductionDate { get; set; }

        [JsonProperty("test_item")]
        public string TestItem { get; set; }

        [JsonProperty("test_date")]
        public string TestDate { get; set; }

        [JsonProperty("input_qty")]
        public int InputQty { get; set; }

        [JsonProperty("ok_qty")]
        public int OkQty { get; set; }

        [JsonProperty("testing_qty")]
        public int TestingQty { get; set; }

        [JsonProperty("ng_qty")]
        public int NgQty { get; set; }

        [JsonProperty("progress_desc")]
        public string ProgressDesc { get; set; }

        [JsonProperty("result")]
        public int Result { get; set; }
    }
}