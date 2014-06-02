using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace refw.Communication {
    [DataContract]
    public class Identifier {
        public const string Delimiter = "-";

        List<string> prefixes = new List<string>();
        [DataMember]
        public List<string> Prefixes {
            get { return prefixes; }
            set { prefixes = value; }
        }

        public override string ToString() {
            return String.Join(Delimiter, Prefixes);
        }

        public override int GetHashCode() {
            int hc = 0;
            foreach (var prefix in prefixes)
                hc ^= prefix.GetHashCode();
            return hc;
        }

        public override bool Equals(object obj) {
            if (obj is Identifier)
                return Prefixes.SequenceEqual(((Identifier)obj).Prefixes);
            return false;
        }
    }
}