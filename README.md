# AuthenticationExample

> meant to be a reference for me for creating apis.
> check the commit history for incremental steps.


## Architecture and techniques used

- options pattern for reading the settings for both `JWT` and `RefreshToken` Settings from `appsettings.json`
- Infrastructure layer references `Design` so it can create it's own migrations
    - so, even in DB 1st approach, you'd only need this layer to scaffold the db.
- API versioning is thru the url `api/v[number]/controller`
- API overrides `ProblemDetailsFactory` to add in custom properties
- docker / docker compose for later, i wanna test nginx, https inside the container and other things.

### Notes

- refresh tokens re issue is the responsibility of the client, having a middleware do that will add considerable overhead to the app.
- using identity to handle user related operations save a lot of time.
- always check the order of the middleware xD
- in the ErrorMiddleware middleware, using newtonsoft will not serialize the way you'd think.



#### commands

> migrations 
```powershell
 dotnet ef migrations add initial -o Migrations -p .\Infrastructure\
 dotnet ef database update -p .\Infrastructure\
```


#### Libraries used

##### Infrastructure
> database interactions layer
- `Microsoft.EntityFrameworkCore.Sqlite` can be interchanged for another provider.
- `Microsoft.Extensions.DependencyInjection.Abstractions` to enable registering `DependencyInjection.cs` into the IoC.
- `Microsoft.Extensions.Identity.Core` for identity
- `Microsoft.EntityFrameworkCore.Design` to create migrations

##### Core
> middle layer, which holds the classes and services used thru out the app
- `Microsoft.AspNetCore.Authentication.JwtBearer` for JWT creation
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` so identity classes can be created
- `Microsoft.AspNetCore.Mvc.ViewFeatures` so a request can be made from the `RegisterRequest` to `AccountController`'s `IsEmailUsed` endpoint.
- `Microsoft.Extensions.DependencyInjection.Abstractions` to enable dependency injection.

#### API
> Presentation Layer
- `Swashbuckle.AspNetCore` swagger.
- `Asp.Versioning.Mvc.ApiExplorer` && `Asp.Versioning.Mvc` for API Versioning