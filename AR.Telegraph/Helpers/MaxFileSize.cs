using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace AR.Telegraph.Helpers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "<Pending>")]
    public class MaxFileSize : ValidationAttribute, IClientModelValidator
    {
        private readonly int _maxFileSize;
        public MaxFileSize(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if(file.Length > _maxFileSize)
                {
                    return new ValidationResult(ErrorMessage());
                }
            }
            return ValidationResult.Success;
        }
        public void AddValidation(ClientModelValidationContext context)
        {
            if(context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-maxFileSize", ErrorMessage());
            var maxFileSize = _maxFileSize.ToString(CultureInfo.InvariantCulture);
            MergeAttribute(context.Attributes, "data-val-maxFileSize-size", maxFileSize);
        }

        private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                return false;
            }
            attributes.Add(key, value);
            return true;
        }
        protected new string ErrorMessage()
        {
            if (_maxFileSize <= 1024)
            {
                return $"أكبر حجم مسموح به {_maxFileSize} بايت";
            }
            else if (_maxFileSize <= 1024 * 1024 && _maxFileSize > 1024)
            {
                return $"أكبر حجم مسموح به {_maxFileSize / 1024 } كيلوبايت";
            }
            else if (_maxFileSize <= 1024 * 1024 * 1024 && _maxFileSize > 1024 * 1024)
            {
                return $"أكبر حجم مسموح به {_maxFileSize / 1024 * 1024 } ميقابايت";
            }
            else
            {
                return $"أكبر حجم مسموح به {_maxFileSize / 1024 * 1024 * 1024} قيقابايت";
            }
        }
    }
}
