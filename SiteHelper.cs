using MaschinenDataein.Models.Data;

namespace MaschinenDataein.Helper
{
    public class SiteHelper
    {
        public static decimal ConvertTemperatur(int temp)
        {
            decimal temperatur = 0;
            if (temp > 0)
            {
                temperatur = Convert.ToDecimal(temp) / 10;
                
            }
            return temperatur;

        }
        public static string ConvertTemperaturToString(int temp)
        {

           
            return String.Format("{0:0.00}", ConvertTemperatur(temp));

        }
    }
}
