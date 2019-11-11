using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows.Controls;

namespace Snapfish.AlwaysOnTop.ValidationRules
{
    public class IpValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string IpAddress = (string) value;
            //Check for all 4 fields
            string[] ipAdressByteFields = IpAddress.Split('.');
            if (ipAdressByteFields.Length != 4)
            {
                return new ValidationResult(false, "A valid IP address contains 4 fields");
            }

            if (ipAdressByteFields.Any(r => !byte.TryParse(r, out _)))
            {
                return new ValidationResult(false, "Invalid value in Ip address field. A valid value range for a byte field is from 0 to 255");
            }

            return !(IPAddress.TryParse(IpAddress, out IPAddress _)) ? new ValidationResult(false, "Invalid IP address") : new ValidationResult(true, null); 
        }
    }
}