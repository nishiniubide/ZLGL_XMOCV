using System;
using System.Data;
using System.Text.RegularExpressions;

namespace ZLGL_XMOCV.Converter
{
    /// <summary>
    /// 转换器公共基类 —— 提供 SafeToStr / SafeToInt32 / 日期转换等通用方法
    /// 各维度转换器继承此类，避免重复代码
    /// </summary>
    public abstract class DataConverterBase
    {
        protected static string ConvertToDateTimeString(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            if (value is DateTime dt) return dt.ToString("yyyy-MM-dd HH:mm:ss");

            string s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return null;

            if (DateTime.TryParse(s, out DateTime parsed)) return parsed.ToString("yyyy-MM-dd HH:mm:ss");

            if (double.TryParse(s, out double oaDate) && oaDate >= 1 && oaDate <= 73415)
            {
                try
                {
                    DateTime d = DateTime.FromOADate(oaDate);
                    if (d.Year >= 2000 && d.Year <= 2099) return d.ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch { }
            }

            return s;
        }

        protected static string ConvertToDateString(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            if (value is DateTime dt) return dt.ToString("yyyy-MM-dd");

            string s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return null;

            if (DateTime.TryParse(s, out DateTime parsed)) return parsed.ToString("yyyy-MM-dd");

            if (double.TryParse(s, out double oaDate) && oaDate >= 1 && oaDate <= 73415)
            {
                try
                {
                    DateTime d = DateTime.FromOADate(oaDate);
                    if (d.Year >= 2000 && d.Year <= 2099) return d.ToString("yyyy-MM-dd");
                }
                catch { }
            }

            return s;
        }

        protected static string SafeToStr(object value, string defaultValue = "")
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            return value.ToString().Trim();
        }

        protected static int SafeToInt32(object value, int defaultValue = 0)
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            string s = value.ToString().Trim();
            if (int.TryParse(s, out int result)) return result;
            if (double.TryParse(s, out double d)) return (int)d;

            var match = Regex.Match(s, @"\d+");
            if (match.Success) return int.Parse(match.Value);

            return defaultValue;
        }

        protected static string SafeToDateKey(object value)
        {
            if (value == null || value == DBNull.Value) return "";
            if (value is DateTime dt) return dt.ToString("yyyy-MM-dd");
            string s = value.ToString().Trim();
            if (DateTime.TryParse(s, out DateTime parsed)) return parsed.ToString("yyyy-MM-dd");
            if (double.TryParse(s, out double oaDate) && oaDate >= 1 && oaDate <= 73415)
            {
                try
                {
                    DateTime d = DateTime.FromOADate(oaDate);
                    if (d.Year >= 2000 && d.Year <= 2099) return d.ToString("yyyy-MM-dd");
                }
                catch { }
            }
            return s;
        }

        protected static decimal SafeToDecimal(object value, decimal defaultValue = 0m)
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            string s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return defaultValue;
            if (decimal.TryParse(s, out decimal result)) return result;
            return defaultValue;
        }

        protected static decimal? SafeToDecimalNullable(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            string s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return null;
            if (decimal.TryParse(s, out decimal result)) return result;
            return null;
        }

        protected static int? SafeToIntNullable(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            string s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return null;
            if (int.TryParse(s, out int result)) return result;
            if (double.TryParse(s, out double d)) return (int)d;
            return null;
        }

        protected static double? SafeToDoubleNullable(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            string s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return null;
            if (double.TryParse(s, out double result)) return result;
            return null;
        }
    }
}
