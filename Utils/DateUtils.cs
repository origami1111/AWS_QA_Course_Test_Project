using System.Globalization;

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

        public static string ParseNumericDateFormatT(double numericDate)
        {
            long unixTimestamp = Convert.ToInt64(numericDate);
            DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            return dateTime.ToString("yyyy-MM-dd HH:mm:sszzz");
        }
    }
}
