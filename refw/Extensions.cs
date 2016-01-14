using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace refw {
    public static class Extensions {
        public static string ToHexString(this IEnumerable<byte> bytes) {
            var lst = bytes.ToArray();
            var sb = new StringBuilder(lst.Length * 3);
            for (int i = 0; i < lst.Length; i++) {
                sb.AppendFormat("{0:X2} ", lst[i]);
            }
            return sb.ToString();
        }
    }
}
