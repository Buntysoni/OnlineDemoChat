using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.WebSockets;
using OnlineChatApp.Models;

namespace OnlineChatApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly WebSocketHandler _webSocketHandler;

        public HomeController(ILogger<HomeController> logger, WebSocketConnectionManager connectionManager, WebSocketHandler webSocketHandler)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _webSocketHandler = webSocketHandler;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetActiveUsers()
        {
            var activeUsers = _connectionManager.GetAllActiveUserIds()
                .Select(socket => new UsersList
                {
                    UserId = socket.ToString()
                })
                        .ToList();
            return Ok(activeUsers);
        }

        [HttpGet]
        public IActionResult GetCurrentUser()
        {
            var activeUsers = _connectionManager.GetAllActiveUserIds()
                .Select(socket => socket.ToString())
                .ToList();
            return Ok(activeUsers);
        }

        //[HttpGet("/wsf")]
        //public async Task GetWebSocket()
        //{
        //    var context = ControllerContext.HttpContext;
        //    if (context.WebSockets.IsWebSocketRequest)
        //    {
        //        System.Net.WebSockets.WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        //        await _webSocketHandler.HandleWebSocketConnection(webSocket);
        //        // Handle WebSocket communication here
        //        await Receive(webSocket);
        //    }
        //    else
        //    {
        //        context.Response.StatusCode = 400;
        //    }
        //}
        //async Task Receive(WebSocket socket)
        //{
        //    var buffer = new byte[1024 * 4];
        //    while (socket.State == WebSocketState.Open)
        //    {
        //        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //        if (result.MessageType == WebSocketMessageType.Text)
        //        {
        //            var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
        //            // Handle received message here
        //            // For example, you can log it or send a response back to the client
        //            Console.WriteLine("Received message: " + message);
        //        }
        //    }
        //}
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
