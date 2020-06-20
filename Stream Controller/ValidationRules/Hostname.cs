using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace Stream_Controller.ValidationRules
{
    public class Hostname : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (((string)value).Length == 0)
            {
                return new ValidationResult(false, "Please enter a hostname or IP address.");
            }
            if (Uri.CheckHostName((string)value) == UriHostNameType.Unknown)
            {
                return new ValidationResult(false, "Please enter a valid hostname or IP address.");
                
            }
            return ValidationResult.ValidResult;
        }
    }
}
