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

// Register SignalR hubs
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");

// Configure HubDocs - discovers hubs with [HubDocs] attribute from registered endpoints
app.AddHubDocs(options =>
{
    options.Title = "HubDocs Sample SignalR API";
    options.Version = "1.0.0";
    options.Description = "Sample project showing HubDocs rich JSON export and interactive SignalR hub explorer.";
    options.ProjectUrl = "https://github.com/mberrishdev/HubDocs";
    options.TermsOfService = "https://github.com/mberrishdev/HubDocs/blob/main/LICENSE";

    options.Contact.Name = "HubDocs Team";
    options.Contact.Email = "support@hubdocs.dev";
    options.Contact.Url = "https://github.com/mberrishdev/HubDocs/issues";

    options.License.Name = "MIT";
    options.License.Url = "https://github.com/mberrishdev/HubDocs/blob/main/LICENSE";
});

app.MapControllers();

app.Run();
