using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

var initialScopes = builder.Configuration["DownstreamApiBook:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
            .AddDownstreamApi("DownstreamApiUser", builder.Configuration.GetSection("DownstreamApiUser"))
            .AddDownstreamApi("DownstreamApiBook", builder.Configuration.GetSection("DownstreamApiBook"))
            .AddInMemoryTokenCaches();

builder.Services.AddHttpClient("GoogleBookManagerApiClient",
                        x =>
                        {
                            x.BaseAddress = new Uri(builder?.Configuration?.GetValue<string>("GoogleBookAPI:BaseUrl"));
                        });

builder.Services.AddHttpClient("GoogleBookImageAPIClient",
                        x =>
                        {
                            x.BaseAddress = new Uri(builder?.Configuration?.GetValue<string>("GoogleBookAPI:ImageAPIBaseURL"));
                        });

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();


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

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
