using OnlineChatApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<WebSocketConnectionManager>();

builder.Services.AddSingleton<WebSocketHandler>();
//builder.Services.AddSingleton<WebSocketHandler>(serviceProvider =>
//{
//    // Create a new instance of WebSocketHandler whenever it's requested
//    return new WebSocketHandler(new WebSocketConnectionManager());
//});
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.UseSession();
app.UseWebSockets();
app.UseMiddleware<WebSocketMiddleware>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();