using SocketServer.Library.Attributes;
using SocketServer.Library.Requests;
using SocketServer.Library.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SocketServer.Library
{
    [SocketRequestService]
    public class ServerService
    {
        [SocketRequestMethod]
        public SocketResponse<bool> Connect(object data)
        {
            var request = SocketRequest.ConvertRequest<string>(data);
            var response = SocketResponse.GetResponse<bool, string>(request);
            response.Data = true;
            return response;
        }

        [SocketRequestMethod]
        public SocketResponse<object> Message(object data)
        {
            var request = SocketRequest.ConvertRequest<string>(data);
            var response = SocketResponse.GetResponse<object, string>(request);
            var messages = MessageManager.GetMessages(request.ClientId);
            response.Data = messages != null && messages.Count > 0 ? messages : null;
            return response;
        }
    }

    
}
