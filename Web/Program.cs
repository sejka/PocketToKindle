using Core;
using Core.EmailSenders;
using Parsers;
using PocketSharp;
using Web.BackgroundService;
using Web.Database;

var builder = WebApplication.CreateBuilder(args);

var config = new Config();
builder.Configuration.Bind(config);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<Config>(config);
builder.Services.AddDbContext<P2kDbContext>();
builder.Services.AddTransient<IUserService, UserService>();
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddHostedService<TimedHostedService>();
builder.Services.AddTransient<ImageInliner>();
builder.Services.AddTransient<IParser, ReadabilityParser>(sp =>
{
    return new ReadabilityParser(config.ParsersApiEndpoint, sp.GetRequiredService<IHttpClientFactory>(), sp.GetRequiredService<ImageInliner>());
});

builder.Services.AddTransient(sp =>
{
    return new MercuryApiParser(config.ParsersApiEndpoint, sp.GetRequiredService<IHttpClientFactory>(), sp.GetRequiredService<ImageInliner>());
});

builder.Services.AddTransient(sp =>
{
    return new ReadabilityParser(config.ParsersApiEndpoint, sp.GetRequiredService<IHttpClientFactory>(), sp.GetRequiredService<ImageInliner>());
});

builder.Services.AddTransient(sp =>
{
    return new PocketClient(config.PocketConsumerKey, callbackUri: config.PocketRedirectUri);
});
builder.Services.AddTransient<IEmailSender, BrevoSender>(sp =>
{
    return new BrevoSender(sp.GetRequiredService<IHttpClientFactory>(), config.EmailSenderApiKey, config.HostEmail, sp.GetRequiredService<ILogger<BrevoSender>>());
});

builder.Services.AddTransient(sp =>
{
    return new ArticleSender(sp.GetRequiredService<PocketClient>(),
                             sp.GetRequiredService<IParser>(),
                             sp.GetRequiredService<IEmailSender>(),
                             sp.GetRequiredService<IUserService>(),
                             sp.GetRequiredService<ILogger<ArticleSender>>(),
                             config.ServiceDomain);
});

builder.Services.AddTransient<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetService<P2kDbContext>().Database;
    db.EnsureCreated();
    //await db.MigrateAsync();
}

app.Run();