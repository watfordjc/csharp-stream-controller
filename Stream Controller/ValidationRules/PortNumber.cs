﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Windows.Controls;

namespace StreamController.ValidationRules
{
    public class PortNumber : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) { throw new ArgumentNullException(nameof(value)); }

            if (
                ((string)value).Length == 0 ||
                !int.TryParse((string)value, out int number) ||
                number <= IPEndPoint.MinPort ||
                number > IPEndPoint.MaxPort
                )
            {
                return new ValidationResult(false, "Please enter a valid port number.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
