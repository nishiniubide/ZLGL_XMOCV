using Newtonsoft.Json;
namespace ZLGL_XMOCV.entity
{
    public class IqcData
    {
        [JsonProperty("inspect_no")]
        public string InspectNo { get; set; }

        [JsonProperty("field_type")]
        public string FieldType { get; set; }

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
        public double? ValidityPeriod { get; set; }

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

        [JsonProperty("deal_method")]
        public string DealMethod { get; set; }

        [JsonProperty("inspect_time")]
        public string InspectTime { get; set; }
    }
}
