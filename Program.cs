global using JsonFormGenerator;
global using YamlDotNet;
global using YamlDotNet.Serialization;
global using YamlDotNet.Serialization.NamingConventions;
using System.Text;
using System.Text.Json;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

static Resources GetResources()
{
    var yaml = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

    using var stream = new FileStream("resources.yaml", FileMode.Open, FileAccess.Read);
    using var reader = new StreamReader(stream);

    return yaml.Deserialize<Resources>(reader);
}

app.MapGet("resources.yaml", () =>
{
    var stream = new FileStream("resources.yaml", FileMode.Open, FileAccess.Read);
    return Results.Stream(stream, "text/yaml");
});

app.MapGet("/", (HttpContext context) => new
{
    styles = Enum.GetNames(typeof(UiStyle)),
    forms = GetResources().Forms.Select(x => x.Name),
    callHint = $"{context.Request.Scheme}://{context.Request.Host}/{{style}}/{{form}}",
    callExample = $"{context.Request.Scheme}://{context.Request.Host}/{Enum.GetNames(typeof(UiStyle)).First()}/{GetResources().Forms.Select(x => x.Name).First()}",
    definitions = $"{context.Request.Scheme}://{context.Request.Host}/resources.yaml"
});

app.Map("/{style}/{form}", async (HttpContext context, string style, string form) =>
{
    context.Response.ContentType = "application/json; charset=utf8";

    var selectedStyle = UiStyle.AdaptiveCards;

    if (Enum.TryParse(style, out UiStyle userStyle))
        selectedStyle = userStyle;

    var resources = GetResources();
    var definition = resources.Forms.FirstOrDefault(x => x.Name == form);

    if (context.Request.Method == "POST")
    {
        using var reader = new StreamReader(context.Request.Body);
        var json = await reader.ReadToEndAsync();
        // value presets
        var para = JsonSerializer.Deserialize<JsonElement>(json);

        foreach (var layout in definition.Layouts.Cast<IDictionary<string, object>>().Where(x => x.ContainsKey("name")))
        {
            if (para.TryGetProperty(layout["name"] as string, out var value))
                layout["value"] = value;
        }
    }

    if (definition != null)
        return definition.CreateLayout(resources.Components, selectedStyle, title: "untitled");

    return "{}";
});

app.Run();
