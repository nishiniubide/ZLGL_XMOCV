using Newtonsoft.Json;

namespace ZLGL_XMOCV.entity
{
    public class IpqcData
    {
        [JsonProperty("inspect_no")]
        public string InspectNo { get; set; }

        [JsonProperty("material_type")]
        public string MaterialType { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("product_code")]
        public string ProductCode { get; set; }

        [JsonProperty("product_batch")]
        public string ProductBatch { get; set; }

        [JsonProperty("process")]
        public string Process { get; set; }

        [JsonProperty("line_id")]
        public string LineId { get; set; }

        [JsonProperty("prod_start_time")]
        public string ProdStartTime { get; set; }

        [JsonProperty("prod_end_time")]
        public string ProdEndTime { get; set; }

        [JsonProperty("samp_freq")]
        public string SampFreq { get; set; }

        [JsonProperty("inspect_item")]
        public string InspectItem { get; set; }

        [JsonProperty("inspect_tool")]
        public string InspectTool { get; set; }

        [JsonProperty("inspect_qty")]
        public int InspectQty { get; set; }

        [JsonProperty("sample_no")]
        public string SampleNo { get; set; }

        [JsonProperty("inspect_value")]
        public string InspectValue { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("upper_level")]
        public string UpperLevel { get; set; }

        [JsonProperty("floor_level")]
        public string FloorLevel { get; set; }

        [JsonProperty("inspect_result")]
        public string InspectResult { get; set; }

        [JsonProperty("inspect_time")]
        public string InspectTime { get; set; }
    }
}