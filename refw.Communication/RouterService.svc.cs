using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace refw.Communication {
    public class RouterService : IRouterService {
        // Clients local to the router
        static Dictionary<Identifier, ClientStatus> knownClients = new Dictionary<Identifier, ClientStatus>();

        // Clients local to the session
        Dictionary<Identifier, ClientStatus> sessionClients = new Dictionary<Identifier, ClientStatus>();

        public void RegisterClient(Identifier identifier) {
            ClientStatus status = null;
            if (!sessionClients.TryGetValue(identifier, out status)) {
                status = new ClientStatus {
                    Identifier = identifier
                };
                sessionClients[identifier] = status;
            }

            status.Active = true;
            status.LastUpdate = DateTime.UtcNow;
            status.StatusData = null;
        }

        public void SetStatus(Identifier identifier, byte[] statusData) {
            
        }

        public void SendMessage(Identifier identifier, byte[] messageData) {
            if (knownClients.ContainsKey(identifier)) {
                // Send message
            } else {
                // Forward message upwards
            }
        }

        public void UnregisterClient(Identifier identifier) {
            throw new NotImplementedException();
        }

        public void SetStatus(byte[] statusData) {
            throw new NotImplementedException();
        }

        public void SetStatus(Dictionary<Identifier, byte[]> statuses) {
            throw new NotImplementedException();
        }
    }
}
