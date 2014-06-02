using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.Communication {
    public interface IRouterCallback {
        void OnReceiveCommand(Identifier identifier, byte[] commandData);
    }
}
