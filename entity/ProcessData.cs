using Newtonsoft.Json;

namespace ZLGL_XMOCV.entity
{
    public class ProcessData
    {
        [JsonProperty("material_type")]
        public string MaterialType { get; set; }

        [JsonProperty("supplier_model")]
        public string SupplierModel { get; set; }

        [JsonProperty("product_code")]
        public string ProductCode { get; set; }

        [JsonProperty("product_batch")]
        public string ProductBatch { get; set; }

        [JsonProperty("process")]
        public string Process { get; set; }

        [JsonProperty("line_id")]
        public string LineId { get; set; }

        [JsonProperty("sn")]
        public string Sn { get; set; }

        [JsonProperty("parameter")]
        public string Parameter { get; set; }

        [JsonProperty("machine_id")]
        public string MachineId { get; set; }

        [JsonProperty("machine_name")]
        public string MachineName { get; set; }

        [JsonProperty("key_value")]
        public string KeyValue { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("upper_level")]
        public string UpperLevel { get; set; }

        [JsonProperty("floor_level")]
        public string FloorLevel { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("op_time")]
        public string OpTime { get; set; }
    }
}