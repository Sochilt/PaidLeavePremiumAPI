# PaidLeavePremiumApi
Minimal ASP.NET Core API to calculate a paid leave premium with configurable rate and employer/employee split.

## Run
```bash
dotnet run
# health
curl http://localhost:5000/healthz
# calculate
curl -X POST http://localhost:5000/calculate -H "Content-Type: application/json" -d "{"Wages": 72000}"
```
Configure `appsettings.json` for rate, split, and wage base cap. Add tests and auth for production.
