using AspNetCore_OpenTelemetry_Studies;
using AspNetCore_OpenTelemetry_Studies.Api;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Telemetry.ServiceName));
    options.AddOtlpExporter();
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource((resource) =>
    {
        resource.AddService(Telemetry.ServiceName);
    })
    .WithTracing((builder) =>
    {
        builder.AddAspNetCoreInstrumentation().AddOtlpExporter();
    })
    .WithMetrics((builder) =>
    {
        builder.AddAspNetCoreInstrumentation().AddOtlpExporter();
    });


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPokemonApiEndpoints(app);

app.UseHttpsRedirection();

app.Run();
