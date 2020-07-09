using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace uk.JohnCook.dotnet.StreamController.ValidationRules
{
    public class Hostname : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) { throw new ArgumentNullException(nameof(value)); }

            if (
                ((string)value).Length == 0 ||
                Uri.CheckHostName((string)value) == UriHostNameType.Unknown
                )
            {
                return new ValidationResult(false, "Please enter a valid hostname or IP address.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
