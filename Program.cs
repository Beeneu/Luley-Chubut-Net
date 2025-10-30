using DotNetEnv;
using Luley_Integracion_Net.Data;
using Luley_Integracion_Net.Repositories;
using Luley_Integracion_Net.Services;
using Microsoft.EntityFrameworkCore;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add connection singleton to hana
builder.Services.AddSingleton(sp =>
{
    var host = Env.GetString("HANA_HOST");
    var port = Env.GetString("HANA_PORT");
    var database = Env.GetString("HANA_DATABASE");
    var user = Env.GetString("HANA_USER");
    var password = Env.GetString("HANA_PASSWORD");

    return HanaConnectionFactory.Create(host, port, database, user, password);
});

builder.Services.AddHttpClient();
builder.Services.AddScoped(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    var baseUrl = Env.GetString("CHUBUT_API_BASE_URL");
    return new HttpService(httpClient, baseUrl);
});

builder.Services.AddDbContext<LuleyDbContext>(
    static (sp, options) =>
    {
        var host = Env.GetString("DB_HOST");
        var user = Env.GetString("DB_USER");
        var password = Env.GetString("DB_PASSWORD");
        var dbName = Env.GetString("DB_NAME");

        options.UseNpgsql(
            $"Host={host};Port=5432;Username={user};Password={password};Database={dbName}"
        );
    }
);

// Add repositories
builder.Services.AddScoped<HanaDbRepository>();
builder.Services.AddScoped<DeliveryNoteDbRepository>();
builder.Services.AddScoped<OrderService>();

// Add cron job
builder.Services.AddHostedService<OrderCronJob>();

var app = builder.Build();

// Checking if the connection with Postgres succeeded
await using var scope = app.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<LuleyDbContext>();
var canConnect = await db.Database.CanConnectAsync();
app.Logger.LogInformation("Connection with Postgres stablished?: {canConnect}", canConnect);

app.MapGet(
    "/",
    async (OrderService orderService) =>
    {
        await orderService.ProcessUpdatedOrdersAsync();
        return Results.Ok();
    }
);

app.MapGet(
    "/healthcheck",
    () =>
    {
        return Results.Ok();
    }
);

app.Run();
