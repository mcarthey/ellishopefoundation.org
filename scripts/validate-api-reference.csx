// Simple controller-to-docs validator
// Scans controller .cs files for public action methods and checks that docs/API_REFERENCE.md mentions the paths.

using System.Text.RegularExpressions;

var repoRoot = Directory.GetCurrentDirectory();
var srcDirs = new [] { Path.Combine(repoRoot, "EllisHope/Areas/Admin/Controllers"), Path.Combine(repoRoot, "EllisHope/Controllers") };
var controllerFiles = new List<string>();
foreach (var d in srcDirs)
{
    if (Directory.Exists(d)) controllerFiles.AddRange(Directory.GetFiles(d, "*.cs", SearchOption.AllDirectories));
}

if (!controllerFiles.Any())
{
    Console.WriteLine("No controller files found; skipping API reference validation.");
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

        // Build base path
        var basePath = area != null ? $"/Admin/{controllerName}" : $"/{controllerName}";
        var path = basePath + "/" + methodName;

        // If method is Index, also add base path without /Index
        if (string.Equals(methodName, "Index", StringComparison.OrdinalIgnoreCase))
        {
            endpoints.Add(basePath);
        }

        // If first param looks like id (int id or int id = ... or int id, or int id) include {id}
        var idParam = Regex.Match(paramsText, "\\b(?:int|long)\\s+id\\b");
        if (idParam.Success)
        {
            path += "/{id}";
        }

        endpoints.Add(path.TrimEnd('/'));
    }
}

Console.WriteLine("Discovered endpoints from controllers:");
foreach (var e in endpoints) Console.WriteLine(e);

var docsPath = Path.Combine(repoRoot, "docs", "API_REFERENCE.md");
if (!File.Exists(docsPath))
{
    Console.WriteLine($"Docs file not found at {docsPath}");
    return 1;
}

var docsText = File.ReadAllText(docsPath);
var missing = new List<string>();
foreach (var e in endpoints)
{
    if (!docsText.Contains(e, StringComparison.OrdinalIgnoreCase)) missing.Add(e);
}

if (missing.Any())
{
    Console.WriteLine();
    Console.WriteLine("The following endpoints were found in code but are missing from docs/API_REFERENCE.md:");
    foreach (var m in missing) Console.WriteLine(m);
    Console.WriteLine();
    Console.WriteLine("Please update docs/API_REFERENCE.md to include these endpoints.");
    return 2;
}

Console.WriteLine("API_REFERENCE.md appears to include all discovered controller endpoints.");
return 0;
