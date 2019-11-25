using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AR.Telegraph.Helpers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "<Pending>")]
    public class AllowedExtensions : ValidationAttribute, IClientModelValidator
    {
        private readonly string[] _allowedExtensions;
        public AllowedExtensions(string[] allowedExtensions)
        {
            _allowedExtensions = allowedExtensions;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extinsion = Path.GetExtension(file.FileName);
                if (_allowedExtensions.Contains(extinsion.ToLower(CultureInfo.CurrentCulture)) == false)
                {
                    return new ValidationResult(ErrorMessage());
                }
            }
            return ValidationResult.Success;
        }
        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-allowedExtensions", ErrorMessage());
            var allowedExtinsionArray = _allowedExtensions.Select(x => x.Replace(".", string.Empty, StringComparison.CurrentCultureIgnoreCase)).ToArray();
            var allowedExtinsionsString = string.Join(",", allowedExtinsionArray);
            MergeAttribute(context.Attributes, "data-val-allowedExtensions-extensions", allowedExtinsionsString);
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
        protected new static string ErrorMessage()
        {
            return $"هذا الإمتداد غير مسموح !";
        }
    }
}
