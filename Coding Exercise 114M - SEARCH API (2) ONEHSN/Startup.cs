using Microsoft.EntityFrameworkCore;


// Model representing a Rectangle
public class Rectangle
{
    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

// DbContext for interacting with the database
public class RectangleDbContext : DbContext
{
    public DbSet<Rectangle> Rectangles { get; set; }

    public RectangleDbContext(DbContextOptions<RectangleDbContext> options) : base(options)
    {
    }
}

// Seed some data into the database
public static class DataSeeder
{
    public static void SeedData(RectangleDbContext dbContext)
    {
        if (!dbContext.Rectangles.Any())
        {
            List<Rectangle> rectangles = new List<Rectangle>
        {
            new Rectangle { X = 10, Y = 10, Width = 50, Height = 50 },
            new Rectangle { X = 100, Y = 100, Width = 30, Height = 40 },
            // Add more rectangles as needed
        };

            dbContext.Rectangles.AddRange(rectangles);
            dbContext.SaveChanges();
        }
    }
}

// API startup configuration
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<RectangleDbContext>(options =>
            options.UseInMemoryDatabase("InMemoryDatabase"));

        // Connection string configuration
        // services.AddDbContext<RectangleDbContext>(options =>
        //     options.UseSqlServer("your_connection_string_here"));

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Rectangle API");
            });

            endpoints.MapPost("/search", async context =>
            {
                var dbContext = context.RequestServices.GetRequiredService<RectangleDbContext>();
                var coordinates = await context.Request.ReadFromJsonAsync<int[]>();

                if (coordinates == null || coordinates.Length == 0)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid input data.");
                    return;
                }

                var matchingRectangles = dbContext.Rectangles
                    .Where(rectangle =>
                        coordinates.Any(coordinate =>
                            coordinate >= rectangle.X &&
                            coordinate <= rectangle.X + rectangle.Width &&
                            coordinate >= rectangle.Y &&
                            coordinate <= rectangle.Y + rectangle.Height))
                    .ToList();

                var response = new
                {
                    Coordinates = coordinates,
                    Rectangles = matchingRectangles
                };

                await context.Response.WriteAsJsonAsync(response);
            });

        });
    }
}
