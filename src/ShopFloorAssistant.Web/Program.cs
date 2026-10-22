using Microsoft.Extensions.Options;
using ShopFloorAssistant.Core.AI;
using ShopFloorAssistant.Core.Chat;
using ShopFloorAssistant.Core.Ingestion;
using ShopFloorAssistant.Core.Retrieval;
using ShopFloorAssistant.Core.Storage;
using ShopFloorAssistant.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Chat and embedding clients for the provider chosen in the "AI" configuration section,
// the vector store with the indexed documents, and the retrieval-augmented chat on top.
builder.AddShopFloorVectorStore();
builder.Services.AddShopFloorAi(builder.Configuration);
builder.Services.AddShopFloorChat(builder.Configuration);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Citation links open the source PDF; the browser's viewer jumps to the page from "#page=N".
app.MapGet("/documents/{fileName}", (string fileName, IOptions<IngestionOptions> options) =>
    DocumentFiles.Resolve(options.Value.DocumentsPath, fileName) is { } path
        ? Results.File(path, "application/pdf")
        : Results.NotFound());

app.MapDefaultEndpoints();

app.Run();
