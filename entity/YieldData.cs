using Newtonsoft.Json;
using System.Collections.Generic;

namespace ZLGL_XMOCV.entity
{
    public class YieldData
    {
        [JsonProperty("key_code")]
        public string KeyCode { get; set; }

        [JsonProperty("check_type")]
        public int? CheckType { get; set; }

        [JsonProperty("material_type")]
        public string MaterialType { get; set; }

        [JsonProperty("field_type")]
        public string FieldType { get; set; }

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

        [JsonProperty("start_time")]
        public string StartTime { get; set; }

        [JsonProperty("end_time")]
        public string EndTime { get; set; }

        [JsonProperty("downtime_detail")]
        public string DowntimeDetail { get; set; }

        [JsonProperty("qty_input")]
        public int QtyInput { get; set; }

        [JsonProperty("qty_output")]
        public int QtyOutput { get; set; }

        [JsonProperty("actual_fpy")]
        public decimal? ActualFpy { get; set; }

        [JsonProperty("target_fpy")]
        public decimal? TargetFpy { get; set; }

        [JsonProperty("actual_py")]
        public decimal ActualPy { get; set; }

        [JsonProperty("target_py")]
        public decimal TargetPy { get; set; }

        [JsonProperty("qty_defect")]
        public int QtyDefect { get; set; }

        [JsonProperty("qty_scrap")]
        public int QtyScrap { get; set; }

        [JsonProperty("yield_type")]
        public int? YieldType { get; set; }

        [JsonProperty("inscls")]
        public string InsCls { get; set; }

        [JsonProperty("insitm")]
        public string InsItm { get; set; }

        [JsonProperty("qtyins")]
        public int? QtyIns { get; set; }

        [JsonProperty("qtydef")]
        public int? QtyDef { get; set; }
    }
}