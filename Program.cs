using CricHeroesAnalytics.Services;
using CricHeroesAnalytics.Services.Interfaces;
using TheMarauderMap.ApiClient;
using TheMarauderMap.Components;
using TheMarauderMap.Repositories;
using TheMarauderMap.Services;
using TheMarauderMap.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();
builder.Services.AddSingleton<ISecretService, SecretService>();
builder.Services.AddSingleton<IUserLoginService, UserLoginService>();
builder.Services.AddSingleton<IAccessTokenService, AccessTokenService>();
builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
builder.Services.AddSingleton<IUpstoxApiClient, UpStoxApiClient>();
builder.Services.AddSingleton<IAccessTokenRepository, AccessTokenRepository>();
builder.Services.AddMemoryCache();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
