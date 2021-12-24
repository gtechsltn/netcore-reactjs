# NetCore ReactJS
+ .NET 6.0 Long Term Support (LTS)
+ Visual Studio 2022
+ ASP.NET Core 6.0 (C# 10)
+ ASP.NET Core 6.0 Web API
+ EF Core 6.0
+ Swagger: OpenAPI Specifications
+ ReactJS + Redux + Saga
+ IIS Web Server
+ Auto Deploy to IIS (git pull + dotnet publish)

# Pros and Cons
+ **HTTP/3**
+ **OpenSSL 3**
+ Performance
  + **System.Text.Json** can use the C# source generation feature to improve performance, reduce private memory usage, and facilitate assembly trimming, which reduces app size.
+ Debugging
  + Debug.WriteLine() => File using System.Diagnostics.TextWriterTraceListener
  + Entity Framework Logging: `context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);`
  + Entity Framework Core Logging: `context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);`
