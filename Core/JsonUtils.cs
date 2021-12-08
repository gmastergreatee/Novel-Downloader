using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Core
{
    public static class JsonUtils
    {
        public static string SerializeJson(object obj)
        {
            var javaScriptSerializer = new JavaScriptSerializer();
            return javaScriptSerializer.Serialize(obj);
        }

        public static T DeserializeJson<T>(string Json)
        {
            var javaScriptSerializer = new JavaScriptSerializer();
            return javaScriptSerializer.Deserialize<T>(Json);
        }
    }
}
