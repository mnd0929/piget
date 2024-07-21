using System.Collections.Generic;
using System.Linq;

namespace QislEngine.Utils
{
    public static class Array
    {
        public static string[] TrimArrayStart(string[] array)
        {
            List<string> res = array.ToList();
            res.Remove(array.FirstOrDefault());

            return ListToArray(res);
        }
        public static string[] ListToArray(List<string> list)
        {
            string[] result = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                result[i] = list[i];
            }

            return result;
        }
    }
}
