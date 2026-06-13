using Newtonsoft.Json;

namespace ZLGL_XMOCV.entity
{
    public class IqcData
    {
        [JsonProperty("request_time")]
        public string RequestTime { get; set; }

        [JsonProperty("factory_code")]
        public string FactoryCode { get; set; }

        [JsonProperty("businessline_id")]
        public int BusinessLineId { get; set; }

        [JsonProperty("inspect_no")]
        public string InspectNo { get; set; }

        [JsonProperty("material_type")]
        public string MaterialType { get; set; }

        [JsonProperty("supplier_model")]
        public string SupplierModel { get; set; }

        [JsonProperty("product_code")]
        public string ProductCode { get; set; }

        [JsonProperty("product_batch")]
        public string ProductBatch { get; set; }

        [JsonProperty("material_no")]
        public string MaterialNo { get; set; }

        [JsonProperty("material_name")]
        public string MaterialName { get; set; }

        [JsonProperty("vendor_name")]
        public string VendorName { get; set; }

        [JsonProperty("incoming_batch")]
        public string IncomingBatch { get; set; }

        [JsonProperty("date_code")]
        public string DateCode { get; set; }

        [JsonProperty("validity_period")]
        public string ValidityPeriod { get; set; }

        [JsonProperty("inspect_item")]
        public string InspectItem { get; set; }

        [JsonProperty("inspect_value")]
        public string InspectValue { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("upper_level")]
        public string UpperLevel { get; set; }

        [JsonProperty("floor_level")]
        public string FloorLevel { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("disposal_method")]
        public string DisposalMethod { get; set; }

        [JsonProperty("inspect_date")]
        public string InspectDate { get; set; }
    }
}