var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var version = Environment.GetEnvironmentVariable("APP_VERSION") ?? "v1";
var products = new List<ProductDto>
{
    new(1, "Laptop", "Electronics", 25000, 8),
    new(2, "Keyboard", "Accessories", 900, 30),
    new(3, "Mouse", "Accessories", 450, 50)
};

app.MapGet("/", () => Results.Ok(new { service = "product-service", message = "FlowOps Product Service is running" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "product-service" }));
app.MapGet("/ready", () => Results.Ok(new { status = "ready", service = "product-service" }));
app.MapGet("/version", () => Results.Ok(new { service = "product-service", version }));

app.MapGet("/api/products", (string? category) =>
{
    var result = string.IsNullOrWhiteSpace(category)
        ? products
        : products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    return Results.Ok(result);
});
app.MapGet("/api/products/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    return product is null ? Results.NotFound(new { message = "Product not found" }) : Results.Ok(product);
});
app.MapPost("/api/products", (CreateProductRequest request) =>
{
    var nextId = products.Count == 0 ? 1 : products.Max(p => p.Id) + 1;
    var product = new ProductDto(nextId, request.Name, request.Category, request.Price, request.Stock);
    products.Add(product);
    return Results.Created($"/api/products/{product.Id}", product);
});

app.Run();

record ProductDto(int Id, string Name, string Category, decimal Price, int Stock);
record CreateProductRequest(string Name, string Category, decimal Price, int Stock);
