using System.Text;

namespace System.Collections.Generic
{
    public static class CollectionExtensions
    {
        public static string ToSingleString<T>(this IEnumerable<T> enumerable, string separator = ", ")
        {
            StringBuilder sb = new StringBuilder();

            foreach(var item in enumerable)
            {                
                if (item != null)
                {
                    if (sb.Length > 0) sb.Append(separator);
                    sb.Append(item.ToString());
                }
            }

            return sb.ToString();
        }
    }
}
