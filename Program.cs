using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PaidLeaveOptions>(
    builder.Configuration.GetSection("PaidLeave")
);

builder.Services.AddProblemDetails();

var app = builder.Build();

// Correlation Id middleware
app.Use(async (ctx, next) =>
{
    const string header = "X-Correlation-Id";
    if (!ctx.Request.Headers.TryGetValue(header, out var id) || string.IsNullOrWhiteSpace(id))
    {
        id = Guid.NewGuid().ToString("n");
        ctx.Response.Headers[header] = id!;
    }
    await next.Invoke();
});

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.MapPost("/calculate", (CalculateRequest req, IConfiguration config) =>
{
    var opts = config.GetSection("PaidLeave").Get<PaidLeaveOptions>() ?? new PaidLeaveOptions();
    if (req.Wages < 0) return Results.Problem("Wages must be non‑negative", statusCode: 400);
    var wageBase = Math.Min(req.Wages, opts.WageBaseCap);

    var total = wageBase * opts.TotalRate;
    var employer = total * opts.EmployerShareFraction;
    var employee = total - employer;

    return Results.Ok(new CalculateResponse
    {
        WageBaseUsed = wageBase,
        TotalPremium = Math.Round(total, 2),
        EmployerContribution = Math.Round(employer, 2),
        EmployeeContribution = Math.Round(employee, 2),
        AppliedRate = opts.TotalRate,
        EmployerShareFraction = opts.EmployerShareFraction
    });
});

app.Run();

public sealed class PaidLeaveOptions
{
    public double TotalRate { get; set; } = 0.0088; // 0.88%
    public double EmployerShareFraction { get; set; } = 0.5; // 50/50 split
    public double WageBaseCap { get; set; } = 100000.0; // example cap; set per policy
}

public sealed class CalculateRequest
{
    [Range(0, double.MaxValue)]
    public double Wages { get; set; }
}

public sealed class CalculateResponse
{
    public double WageBaseUsed { get; set; }
    public double TotalPremium { get; set; }
    public double EmployerContribution { get; set; }
    public double EmployeeContribution { get; set; }
    public double AppliedRate { get; set; }
    public double EmployerShareFraction { get; set; }
}
