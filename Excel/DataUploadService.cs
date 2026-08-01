using System;
using System.Collections.Generic;
using System.Data;
using ZLGL_XMOCV.Api;
using ZLGL_XMOCV.Config;
using ZLGL_XMOCV.Converter;

using ZLGL_XMOCV.Validation;

namespace ZLGL_XMOCV.Excel
{
    /// <summary>
    /// 业务编排服务 —— 串联 Excel读取 → 校验 → 转换 → 上传 的完整流程
    /// 窗体只调用此服务，不再直接操作底层组件
    /// </summary>
    public class DataUploadService
    {
        private readonly IExcelReader _excelReader;
        private readonly IX5Client _x5Client;

        public DataUploadService(IExcelReader excelReader, IX5Client x5Client)
        {
            _excelReader = excelReader ?? throw new ArgumentNullException(nameof(excelReader));
            _x5Client = x5Client ?? throw new ArgumentNullException(nameof(x5Client));
        }

        /// <summary>
        /// 读取并校验 Excel
        /// </summary>
        public ImportResult ImportAndValidate(string filePath, DataDimension dimension)
        {
            var result = new ImportResult();

            try
            {
                result.Data = _excelReader.Read(filePath, null, dimension);

                var validator = ValidatorFactory.GetValidator(dimension);
                result.Validation = validator.Validate(result.Data);

                result.Success = result.Validation.IsValid;
                result.RowCount = result.Data?.Rows.Count ?? 0;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 转换并上传数据
        /// </summary>
        public UploadResult ConvertAndUpload(DataTable data, DataDimension dimension)
        {
            var result = new UploadResult();

            try
            {
                // 转换
                var converter = ConverterFactory.GetConverter(dimension);
                var convertResult = converter.Convert(data);

                if (!convertResult.Success)
                {
                    result.Success = false;
                    result.Message = "数据转换失败";
                    return result;
                }

                // 获取配置
                var config = CredentialProvider.GetConfigTest(dimension);

                // 校验配置完整性
                if (string.IsNullOrEmpty(config.UserName) || string.IsNullOrEmpty(config.Password) ||
                    string.IsNullOrEmpty(config.Url) || string.IsNullOrEmpty(config.AppId) ||
                    string.IsNullOrEmpty(config.AppKey))
                {
                    result.Success = false;
                    result.Message = "X5 配置中存在空字段，请检查！";
                    return result;
                }

                // 上传
                var x5Result = _x5Client.PostData(config, convertResult.BodyJson);
                result.Success = x5Result.Success;
                result.Message = x5Result.Success
                    ? $"上传成功！{x5Result.Message}"
                    : $"上传失败：{x5Result.Code} - {x5Result.Message}";
                result.RawContent = x5Result.RawContent;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }
    }

    #region 服务层结果模型

    public class ImportResult
    {
        public bool Success { get; set; }
        public DataTable Data { get; set; }
        public ValidationResult Validation { get; set; }
        public int RowCount { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class UploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string RawContent { get; set; }
    }

    #endregion
}
