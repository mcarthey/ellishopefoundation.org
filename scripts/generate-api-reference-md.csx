// scripts/generate-api-reference-md.csx
using System.Text.RegularExpressions;

var repoRoot = Directory.GetCurrentDirectory();
var srcDirs = new[] { Path.Combine(repoRoot, "EllisHope/Areas/Admin/Controllers"), Path.Combine(repoRoot, "EllisHope/Controllers") };
var controllerFiles = new List<string>();
foreach (var d in srcDirs)
{
    if (Directory.Exists(d)) controllerFiles.AddRange(Directory.GetFiles(d, "*.cs", SearchOption.AllDirectories));
}

if (!controllerFiles.Any())
{
    Console.WriteLine("No controller files found; skipping API reference generation.");
    return 0;
}

var endpoints = new SortedSet<string>();
var areaRegex = new Regex("\\[Area\\(\\\"(?<area>[^\\\"]+)\\\"\\)\\]", RegexOptions.Compiled);
var classRegex = new Regex("class\\s+(?<name>\\w+Controller)\\b", RegexOptions.Compiled);
var methodRegex = new Regex("public\\s+(?:async\\s+)?(?:Task<[^>]+>|Task|IActionResult|ActionResult<[^>]+>|ActionResult)\\s+(?<name>\\w+)\\s*\\((?<params>[^)]*)\\)", RegexOptions.Compiled);

foreach (var file in controllerFiles)
{
    var text = File.ReadAllText(file);

    var areaMatch = areaRegex.Match(text);
    var area = areaMatch.Success ? areaMatch.Groups["area"].Value : null;

    var classMatch = classRegex.Match(text);
    if (!classMatch.Success) continue;
    var controllerName = classMatch.Groups["name"].Value.Replace("Controller", "");

    foreach (Match m in methodRegex.Matches(text))
    {
        var methodName = m.Groups["name"].Value;
        var paramsText = m.Groups["params"].Value;

        var basePath = area != null ? $"/Admin/{controllerName}" : $"/{controllerName}";
        var path = basePath + "/" + methodName;

        if (string.Equals(methodName, "Index", StringComparison.OrdinalIgnoreCase))
        {
            endpoints.Add(basePath);
        }

        var idParam = Regex.Match(paramsText, "\\b(?:int|long)\\s+id\\b");
        if (idParam.Success)
        {
            path += "/{id}";
        }

        endpoints.Add(path.TrimEnd('/'));
    }
}

// Write to docs/API_REFERENCE.md (replace or append to a section)
var docsPath = Path.Combine(repoRoot, "docs", "API_REFERENCE_GENERATED.md");
var header = "# API Reference — Ellis Hope Foundation (Auto-Generated)\n\n";
var content = header + "## Discovered Endpoints\n\n" + string.Join("\n", endpoints.Select(e => $"- `{e}`")) + "\n";
File.WriteAllText(docsPath, content);

Console.WriteLine("API_REFERENCE_GENERATED.md updated with discovered endpoints.");
return 0;