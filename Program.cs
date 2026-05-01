using AIComplaintAgent.Data;
using AIComplaintAgent.Plugins;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using AIComplaintAgent.Services;
using AIComplaintAgent.Agents;
using AIComplaintAgent.Functions;



var builder = WebApplication.CreateBuilder(args);
/*builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));*/
builder.Services.AddScoped<EmailPlugin>();
builder.Services.AddScoped<ContentSafetyService>();
builder.Services.AddScoped<ComplaintService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));
// ربط Semantic Kernel
builder.Services.AddKernel()
    .AddAzureOpenAIChatCompletion(
        deploymentName: builder.Configuration["AzureOpenAI:DeploymentName"]!,
        endpoint: builder.Configuration["AzureOpenAI:Endpoint"]!,
        apiKey: builder.Configuration["AzureOpenAI:ApiKey"]!
    );
builder.Services.AddScoped<ComplaintAgentService>();
builder.Services.AddScoped<FollowUpAgent>();
//builder.Services.AddScoped<FollowUpFunction>();
builder.Services.AddHostedService<FollowUpBackgroundService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Complaint}/{action=Index}/{id?}");

app.Run();