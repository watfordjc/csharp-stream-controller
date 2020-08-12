using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace uk.JohnCook.dotnet.StreamController.ValidationRules
{
    public class NumberGreaterZero : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) { throw new ArgumentNullException(nameof(value)); }

            if (
                ((string)value).Length == 0 ||
                !int.TryParse((string)value, out int number) ||
                number <= 0 ||
                number > int.MaxValue
                )
            {
                return new ValidationResult(false, Properties.Resources.validation_error_NumberGreaterZero);
            }
            return ValidationResult.ValidResult;
        }
    }
}
