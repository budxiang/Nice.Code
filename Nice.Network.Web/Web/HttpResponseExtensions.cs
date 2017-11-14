using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Net;
using System.Text;

namespace Nice.Network.Web
{
    public class HttpResponseExtensions
    {
        private static IsoDateTimeConverter timeConverter = null;
        private static IsoDateTimeConverter GetTimeConverter()
        {
            if (timeConverter == null)
            {
                timeConverter = new IsoDateTimeConverter();
                timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            }
            return timeConverter;
        }
        public static void WriteFile(HttpListenerResponse response, string file, string contentType)
        {
            byte[] page = FileHelper.GetFile(file);
            if (page == null)
            {
                response.StatusCode = StatusCode.NotFound;
                response.Close();
                return;
            }
            response.ContentType = contentType;
            WriteBinary(response, page);
        }
        public static void Write(HttpListenerResponse response, object result, MimeType contentType)
        {
            byte[] data = null;
            if (contentType == MimeType.Json)
            {
                data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result, GetTimeConverter()));
                response.ContentType = MimeMapping.Json;
            }
            else if (contentType == MimeType.Text)
            {
                data = Encoding.UTF8.GetBytes(result.ToString());
                response.ContentType = MimeMapping.Text;
            }
            if (data == null)
            {
                response.StatusCode = StatusCode.ServiceUnavailable;
                response.Close();
                return;
            }
            WriteBinary(response, data);
        }
        public static void Write(HttpListenerResponse response, int statusCode, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            response.StatusCode = statusCode;
            response.ContentType = MimeMapping.Text;
            WriteBinary(response, data);
        }
        public static void WriteBinary(HttpListenerResponse response, byte[] data)
        {
            response.ContentLength64 = data.Length;
            Stream output = response.OutputStream;
            output.Write(data, 0, data.Length);
            output.Close();
            response.Close();
        }

    }
}
