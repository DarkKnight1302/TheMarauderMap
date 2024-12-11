using Blazored.SessionStorage;
using TheMarauderMap.Services;
using TheMarauderMap.Services.Interfaces;
using Quartz;
using TheMarauderMap.ApiClient;
using TheMarauderMap.Components;
using TheMarauderMap.CronJob;
using TheMarauderMap.Repositories;

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
builder.Services.AddSingleton<ISessionRepository, SessionRepository>();
builder.Services.AddSingleton<IStockRepository, StockRepository>();
builder.Services.AddSingleton<IJobExecutionRepository, JobExecutionRepository>();
builder.Services.AddSingleton<IStockRecommendationService, StockRecommendationService>();
builder.Services.AddSingleton<IActiveStockRepository, ActiveStockRepository>();
builder.Services.AddSingleton<IStockTradeService, StockTradeService>();
builder.Services.AddSingleton<IUserInvestmentsRepository, UserInvestmentRepository>();
builder.Services.AddSingleton<IUserBlackListRepository, UserBlackListedRepository>();
builder.Services.AddSingleton<IScreenerClient, ScreenerClient>();
builder.Services.AddSingleton<IStockFundamentalsRepository, StockFundamentalsRepository>();
builder.Services.AddSingleton<IStockScoringService, StockScoringService>();
builder.Services.AddSingleton<IRetryStrategy>(sp => new RetryStrategy(maxRetries: 3, delay: TimeSpan.FromSeconds(1)));
builder.Services.AddMemoryCache();
builder.Services.AddBlazoredSessionStorage();
builder.Logging.AddConsole();
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("StockPriceUpdate");

    q.AddJob<StockPriceUpdateJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
                        .ForJob(jobKey)
                        .WithIdentity("StockPriceUpdate-trigger")
                        .WithCronSchedule("0 30 8 * * ?"));

    var sellJobKey = new JobKey("StockSellJob");

    q.AddJob<StockSellJob>(opts => opts.WithIdentity(sellJobKey));

    q.AddTrigger(opts => opts
                        .ForJob(sellJobKey)
                        .WithIdentity("StockSellJob-trigger")
                        .WithCronSchedule("0 0 * * * ?"));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
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
