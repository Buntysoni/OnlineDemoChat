using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

public class WebSocketConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();
    private static ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();

    public string AddSocket(WebSocket socket, bool IsRejoin, string existingId)
    {
        var connectionId = !IsRejoin ? Guid.NewGuid().ToString() : existingId;
        _sockets.TryAdd(connectionId, socket);
        _users.TryAdd(connectionId, connectionId);
        return connectionId;
    }

    public void RemoveSocket(string connectionId)
    {
        _users.TryRemove(connectionId, out _);
        _sockets.TryRemove(connectionId, out _);
    }

    public string GetId(WebSocket socket)
    {
        return _sockets.FirstOrDefault(p => p.Value == socket).Key;
    }

    public WebSocket GetSocketById(string connectionId)
    {
        _sockets.TryGetValue(connectionId, out WebSocket socket);
        return socket;
    }

    public async Task SendMessageAsync(string connectionId, string message, bool IsAll = false, string ParticularUser = "")
    {
        if (IsAll)
        {
            await SendMessageToAllAsync(message);
        }
        else
        {
            if (ParticularUser != "0")
            {
                if (_sockets.TryGetValue(connectionId, out WebSocket socket))
                {
                    await SendMessageAsync(socket, message);
                }
                if (_sockets.TryGetValue(ParticularUser, out WebSocket socketp) && connectionId != ParticularUser)
                {
                    await SendMessageAsync(socketp, message);
                }
            }
            else
            {
                if (_sockets.TryGetValue(connectionId, out WebSocket socket))
                {
                    await SendMessageAsync(socket, message);
                }
                else
                {
                    // Handle error: WebSocket not found
                }
            }
        }
    }

    public async Task SendMessageToAllAsync(string message)
    {
        foreach (var socket in _sockets.Values)
        {
            await SendMessageAsync(socket, message);
        }
    }

    private async Task SendMessageAsync(WebSocket socket, string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public List<WebSocket> GetAllConnections()
    {
        return _sockets.Values.ToList();
    }

    public List<string> GetAllActiveUserIds()
    {
        return _users.Keys.ToList();
    }
}
