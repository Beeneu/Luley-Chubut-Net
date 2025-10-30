using DotNetEnv;
using Luley_Integracion_Net.Data;
using Luley_Integracion_Net.Repositories;
using Luley_Integracion_Net.Services;

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

// Add repositories
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

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
