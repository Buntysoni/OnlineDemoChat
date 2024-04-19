using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace WebApplication2
{
    public class WebSocketHandler
    {
        private readonly WebSocketConnectionManager _connectionManager;
        public string CurrentConnection = "";

        public WebSocketHandler(WebSocketConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task HandleWebSocketConnection(HttpContext context, WebSocket socket, bool IsNewCon = false, bool IsRejoin = false, string existingId = "")
        {
            if (IsNewCon)
            {
                CurrentConnection = _connectionManager.AddSocket(socket, IsRejoin, existingId);
                if (!IsRejoin)
                    await SendMessageToAllAsync($"user {CurrentConnection} is joined!", true, "");
            }

            try
            {
                await ReceiveMessages(socket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var rData = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageDataModel>(message);
                        if (rData != null)
                            CurrentUser.CurrentUserId = rData.CurrenctUserId = _connectionManager.GetId(socket);
                        string reJoin = rData.IsRejoin ? "Rejoined" : "";
                        await SendMessageToAllAsync($"User {reJoin} {rData.CurrenctUserId}: {DateTime.Now.ToString("MM/dd/yyyy HH.mm.ss")} : {rData.Message}", rData.IsAll, rData?.ParticularUser);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await OnDisconnected(socket);
                    }
                });
            }
            catch (Exception ex)
            {
                _connectionManager.RemoveSocket(_connectionManager.GetId(socket));
                await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
            }
        }

        private async Task ReceiveMessages(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                handleMessage(result, buffer);
            }
        }

        public virtual async Task<string> OnDisconnected(WebSocket socket)
        {
            string socketId = _connectionManager.GetId(socket);
            await SendMessageToAllAsync($"user {socketId} is left!", true, "");
            _connectionManager.RemoveSocket(socketId);

            return socketId;
        }

        private async Task SendMessageToAllAsync(string message, bool IsAll, string ParticularUser)
        {
            //foreach (var socket in _connectionManager.SendMessageAsync())
            //{
            //    if (socket.State == WebSocketState.Open)
            //    {
            //        await socket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, CancellationToken.None);
            //    }
            //}
            await _connectionManager.SendMessageAsync(message.Split(':')[0].ToString().Replace("User ", ""), message, IsAll, ParticularUser);
            //await SendMessageToAllAsync(message);
        }
    }

    public class MessageDataModel
    {
        public string? Message { get; set; }
        public bool IsAll { get; set; }
        public string? ParticularUser { get; set; }
        public string? CurrenctUserId { get; set; }
        public bool IsRejoin { get; set; }
    }

    public static class CurrentUser
    {
        public static string? CurrentUserId { get; set; }
    }

    public class CurrentUserDemo
    {
        public string? CurrentUserId { get; set; }
    }

    public class UsersList
    {
        public string? UserId { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
