using System.Net.WebSockets;

namespace WebApplication2
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketHandler _webSocketHandler;
        //private readonly IHttpContextAccessor _httpContextAccessor;

        public WebSocketMiddleware(RequestDelegate next, WebSocketHandler webSocketHandler/*, IHttpContextAccessor httpContextAccessor*/)
        {
            _next = next;
            _webSocketHandler = webSocketHandler;
            //_httpContextAccessor = httpContextAccessor;
        }

        public async Task Invoke(HttpContext context, WebSocketConnectionManager connectionManager)
        {
            try
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        bool IsRejoin = Convert.ToBoolean(context.Request.Query["isRejoin"]);
                        string existingId = Convert.ToString(context.Request.Query["existingId"]);
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await _webSocketHandler.HandleWebSocketConnection(context, webSocket, true, IsRejoin, existingId);
                    }
                    else
                    {
                        //context.Response.StatusCode = 400;
                        await _next.Invoke(context);
                        return;
                    }
                }
                else
                {
                    await _next(context);
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"An error occurred: {ex.Message}");
            }
        }
    }
}
