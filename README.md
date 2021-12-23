# NetCore ReactJS

+ ASP.NET Core 5.0
+ ASP.NET Core 5.0 Web API
+ ReactJS + Redux
+ Redux Saga
+ Debug.WriteLine() => File using System.Diagnostics.TextWriterTraceListener
+ Entity Framework Logging: Debug.WriteLine()
`
  +    using (var context = new SchoolDBEntities())
  +    {
  +        context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
  +    }
`
+ https://stackoverflow.com/questions/52306788/logging-all-entity-framework-queries-in-the-debug-window-in-db-first-approach
+ IIS Web Server
+ Auto Deploy to IIS (git pull + dotnet publish)
