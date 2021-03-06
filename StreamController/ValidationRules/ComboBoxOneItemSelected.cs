﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace uk.JohnCook.dotnet.StreamController.ValidationRules
{
    public class ComboBoxOneItemSelected : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) {
                return new ValidationResult(false, Properties.Resources.validation_error_ComboBoxOneItemSelected);
            }
            return ValidationResult.ValidResult;
        }
    }
}
