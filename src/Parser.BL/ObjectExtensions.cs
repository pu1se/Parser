using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.BL
{
    public static class ObjectExtensions
    {
        private static readonly JsonSerializerSettings _jsonSerializationOptions = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        public static string ToJson(this object objectForSerialization)
        {
            if (objectForSerialization == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(objectForSerialization, _jsonSerializationOptions);
        }

        public static T ToObject<T>(this string json) where T: class
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<T>(json, _jsonSerializationOptions);
        }

        public static string ToFormattedExceptionDescription(this Exception exception)
        {
            var stackTrace = exception.StackTrace;
            var errorMessage = exception.Message;

            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
                errorMessage += "; " + exception.Message;
            }
            return $"Exception message: {errorMessage}. {Environment.NewLine}" +
                   $"Stack-trace: {stackTrace}. {Environment.NewLine}";
        }

        public static string Join(this IEnumerable<string> list, string separator)
        {
            return string.Join(separator, list);
        }

        public static bool IsNullOrEmpty(this string st)
        {
            return String.IsNullOrEmpty(st);
        }
    }
}
