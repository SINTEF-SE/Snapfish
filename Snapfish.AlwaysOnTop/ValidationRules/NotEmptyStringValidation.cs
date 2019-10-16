using System;
using System.Globalization;
using System.Windows.Controls;

namespace Snapfish.AlwaysOnTop.ValidationRules
{
    public class NotEmptyStringValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string stringValue = (string) value;
            return String.IsNullOrWhiteSpace(stringValue)
                ? new ValidationResult(false, "The string cannot be empty. Please insert a sender name")
                : new ValidationResult(true, null);
        }
    }
}