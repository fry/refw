using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace refw.Communication {
    public class ClientStatus {
        public bool Active;
        public DateTime LastUpdate;
        public Identifier Identifier;
        public byte[] StatusData;

        public IRouterCallback CallbackChannel;

        public override int GetHashCode() {
            return Identifier.GetHashCode();
        }

        public override bool Equals(object obj) {
            return Identifier.Equals(obj);
        }
    }
}