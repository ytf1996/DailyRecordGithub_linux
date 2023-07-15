using System;
using System.Collections.Generic;
using System.Text;

namespace Roim.Common
{
    public class ConvertHelper
    {
        public static decimal? ConvertToDecimal(string value, bool NullreturnZero = false)
        {
            decimal Result = 0m;
            if (decimal.TryParse(value, out Result))
                return Result;
            if (NullreturnZero)
                return 0;

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <param name="NullreturnValue">如果转换失败返回值</param>
        /// <returns></returns>
        public static int? ConvertToInt(string value, int? NullreturnValue = null)
        {
            int Result = 0;
            if (int.TryParse(value, out Result))
                return Result;

            if (NullreturnValue != null)
                return NullreturnValue.Value;

            return null;
        }

        public static bool IsInt(string value)
        {
            if (ConvertToInt(value) == null)
                return false;
            return true;
        }

        public static string ConvertToString(object value)
        {
            if (value == null)
                return string.Empty;
            return value.ToString();
        }

        public static object IsIntReturnIntOrString(string value)
        {
            int valueInt = 0;
            if(int.TryParse(value,out valueInt))
            {
                return valueInt;
            }
            return value;
        }
    }
}
