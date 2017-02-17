using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//using System.Web.Script.Serialization;

namespace SocketClient.Library
{

   

    public static class Extensions
    {
        //#region JSON Extensions

        public static T FromJsonString<T>(this string obj)
        {
            return JsonUtility.FromJson<T>(obj);
        }

        public static object FromJsonString(this string obj)
        {
            return JsonUtility.FromJson(obj, typeof(object));
        }

        public static string ToJsonString(this object obj)
        {

            return JsonUtility.ToJson(obj);
        }


        //#endregion
    }

}
