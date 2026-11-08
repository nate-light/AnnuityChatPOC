using AnnuityChatPOC.Components;
using AnnuityChatPOC.Application.Abstractions;
using AnnuityChatPOC.Application.Prompts;
using AnnuityChatPOC.Application.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddFilter("AnnuityChatPOC.Application.Services", LogLevel.Information);
builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient(nameof(OpenAiResponsesService));
builder.Services.AddScoped<AssistantInstructionsProvider>();
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(OpenAiOptions.SectionName));
builder.Services.Configure<ChatOptions>(builder.Configuration.GetSection(ChatOptions.SectionName));
builder.Services.AddScoped<IOpenAiResponsesService, OpenAiResponsesService>();
builder.Services.AddScoped<IProductCatalogService, ProductCatalogService>();
builder.Services.AddScoped<IRecommendationValidationService, RecommendationValidationService>();
builder.Services.AddScoped<ISuitabilityProfileService, SuitabilityProfileService>();
builder.Services.AddScoped<IConversationService, ConversationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    using var scope = app.Logger.BeginScope(new Dictionary<string, object?>
    {
        ["RequestPath"] = context.Request.Path.Value,
        ["TraceIdentifier"] = context.TraceIdentifier
    });
    await next();
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
