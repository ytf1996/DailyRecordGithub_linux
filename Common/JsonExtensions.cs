using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MesMessagePlat
{
    public static class JsonExtensions
    {
        /// <summary>Converts given object to JSON string.</summary>
        /// <returns></returns>
        public static string ToJsonString(this object obj, bool camelCase = false, bool indented = false)
        {
            if(obj == null)
            {
                return string.Empty;
            }
            JsonSerializerSettings settings = new JsonSerializerSettings();
            if (camelCase)
                settings.ContractResolver = (IContractResolver)new CamelCasePropertyNamesContractResolver();
            if (indented)
                settings.Formatting = Formatting.Indented;
            return JsonConvert.SerializeObject(obj, settings);
        }


        public static T ToModel<T>(this string json)
           where T : class
        {
            T list = null;

            if(string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                list = JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                
            }

            return list;
        }
    }
}
