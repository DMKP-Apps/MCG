using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Script.Serialization;

namespace SocketServer.Library
{
    public static class Extensions
    {
        #region JSON Extensions

        public static T FromJsonString<T>(this string obj, int recursionDepth = 100)
        {
            if (obj == null || string.IsNullOrEmpty(obj.Trim())) return default(T);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            serializer.RecursionLimit = recursionDepth;
            return serializer.Deserialize<T>(obj);
        }

        public static object FromJsonString(this string obj, int recursionDepth = 100)
        {
            if (obj == null || string.IsNullOrEmpty(obj.Trim())) return null;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            serializer.RecursionLimit = recursionDepth;
            return serializer.DeserializeObject(obj);
        }

        public static string ToJsonString(this object obj, int recursionDepth = 100)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            serializer.RecursionLimit = int.MaxValue;
            return serializer.Serialize(obj);
        }


        #endregion
    }

}
