using HubDocs;
using HubDocs.Sample.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseStaticFiles();

// Map SignalR hubs
app.MapHubAndRegister<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");

// Configure HubDocs middleware
app.AddHubDocs();

app.MapControllers();

app.Run();
