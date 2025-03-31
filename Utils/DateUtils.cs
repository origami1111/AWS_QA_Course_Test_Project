using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS_QA_Course_Test_Project.Utils
{
    public class DateUtils
    {
        public static string FormatDateForUploadedImage(string date)
        {
            DateTime parsedDate = DateTime.Parse(date, null, DateTimeStyles.AdjustToUniversal);
            return parsedDate.ToString("yyyy-MM-dd HH:mm:ss+00:00");
        }

        public static string FormatDateForDeletedImage(string date)
        {
            DateTime parsedDate = DateTime.Parse(date, null, DateTimeStyles.AdjustToUniversal);
            return parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
