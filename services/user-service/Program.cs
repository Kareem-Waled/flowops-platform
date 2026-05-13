var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var version = Environment.GetEnvironmentVariable("APP_VERSION") ?? "v1";
var users = new List<UserDto>
{
    new(1, "Mohanad", "mohanad@example.com"),
    new(2, "Demo User", "demo@example.com")
};

app.MapGet("/", () => Results.Ok(new { service = "user-service", message = "FlowOps User Service is running" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "user-service" }));
app.MapGet("/ready", () => Results.Ok(new { status = "ready", service = "user-service" }));
app.MapGet("/version", () => Results.Ok(new { service = "user-service", version }));

app.MapGet("/api/users", () => Results.Ok(users));
app.MapGet("/api/users/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user is null ? Results.NotFound(new { message = "User not found" }) : Results.Ok(user);
});
app.MapPost("/api/users", (CreateUserRequest request) =>
{
    var nextId = users.Count == 0 ? 1 : users.Max(u => u.Id) + 1;
    var user = new UserDto(nextId, request.Name, request.Email);
    users.Add(user);
    return Results.Created($"/api/users/{user.Id}", user);
});

app.Run();

record UserDto(int Id, string Name, string Email);
record CreateUserRequest(string Name, string Email);
