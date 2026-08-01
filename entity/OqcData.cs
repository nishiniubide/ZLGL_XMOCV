using Newtonsoft.Json;
using System.Collections.Generic;

namespace ZLGL_XMOCV.entity
{
    public class OqcDefect
    {
        [JsonProperty("fail_name")]
        public string FailName { get; set; }

        [JsonProperty("fail_qty")]
        public int FailQty { get; set; }
    }

    public class OqcData
    {
        [JsonProperty("request_time")]
        public string RequestTime { get; set; }

        [JsonProperty("factory_code")]
        public string FactoryCode { get; set; }

        [JsonProperty("businessline_id")]
        public int BusinessLineId { get; set; }

        [JsonProperty("product_code")]
        public string ProductCode { get; set; }

        [JsonProperty("field_type")]
        public string FieldType { get; set; }

        [JsonProperty("supplier_model")]
        public string SupplierModel { get; set; }

        [JsonProperty("line_id")]
        public string LineId { get; set; }

        [JsonProperty("shipment_batch")]
        public string ShipmentBatch { get; set; }

        [JsonProperty("site")]
        public string Site { get; set; }

        [JsonProperty("check_date")]
        public string CheckDate { get; set; }

        [JsonProperty("shipment_qty")]
        public int ShipmentQty { get; set; }

        [JsonProperty("check_batch_qty")]
        public int CheckBatchQty { get; set; }

        [JsonProperty("pass_batch_qty")]
        public int PassBatchQty { get; set; }

        [JsonProperty("check_qty")]
        public int CheckQty { get; set; }

        [JsonProperty("accept_standard")]
        public string AcceptStandard { get; set; }

        [JsonProperty("fail_detail")]
        public List<OqcDefect> FailDetail { get; set; }
    }
}