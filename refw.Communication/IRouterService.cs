using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace refw.Communication {
    [ServiceContract(SessionMode=SessionMode.Required, CallbackContract=typeof(IRouterCallback))]
    public interface IRouterService {
        [OperationContract(IsOneWay = true, IsInitiating = true)]
        void RegisterClient(Identifier identifier);

        [OperationContract(IsOneWay = true)]
        void UnregisterClient(Identifier identifier);

        [OperationContract(IsOneWay = true)]
        void SetStatus(byte[] statusData);

        [OperationContract(IsOneWay = true)]
        void SetStatus(Identifier identifier, byte[] statusData);
        
        [OperationContract(IsOneWay = true)]
        void SetStatus(Dictionary<Identifier, byte[]> statuses);

        [OperationContract(IsOneWay = true)]
        void SendMessage(Identifier identifier, byte[] messageData);
    }
}
