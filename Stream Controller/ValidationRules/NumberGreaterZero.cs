﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace Stream_Controller.ValidationRules
{
    public class NumberGreaterZero : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (
                ((string)value).Length == 0 ||
                !int.TryParse((string)value, out int number) ||
                number <= 0 ||
                number > int.MaxValue
                )
            {
                return new ValidationResult(false, "Please enter a number greater than zero.");
            }
            return ValidationResult.ValidResult;
        }
    }
}