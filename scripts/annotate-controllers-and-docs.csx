// scripts/annotate-controllers-and-docs.csx
using System.Text.RegularExpressions;

var repoRoot = Directory.GetCurrentDirectory();
var srcDirs = new[] { Path.Combine(repoRoot, "EllisHope/Areas/Admin/Controllers"), Path.Combine(repoRoot, "EllisHope/Controllers") };
var apiRefPath = Path.Combine(repoRoot, "docs", "API_REFERENCE.md");

var controllerFiles = new List<string>();
foreach (var d in srcDirs)
{
    if (Directory.Exists(d)) controllerFiles.AddRange(Directory.GetFiles(d, "*.cs", SearchOption.AllDirectories));
}

// 1. Parse API_REFERENCE.md for endpoint descriptions
var apiRefText = File.ReadAllText(apiRefPath);
var mdEndpointRegex = new Regex(@"- (GET|POST|PUT|DELETE) `(?<path>[^`]+)` ?— ?(?<desc>.+)", RegexOptions.IgnoreCase);
var endpointDescriptions = new Dictionary<string, string>();
foreach (Match m in mdEndpointRegex.Matches(apiRefText))
{
    var path = m.Groups["path"].Value.Trim();
    var desc = m.Groups["desc"].Value.Trim();
    endpointDescriptions[path] = desc;
}

// 2. Parse and update controllers
var methodRegex = new Regex(@"(/// <summary>[\s\S]+?</summary>\s+)?(\[SwaggerOperation\([\s\S]+?\)\]\s+)?public\s+(?:async\s+)?(?:Task<[^>]+>|Task|IActionResult|ActionResult<[^>]+>|ActionResult)\s+(?<name>\w+)\s*\((?<params>[^\)]*)\)", RegexOptions.Compiled);
var httpAttrRegex = new Regex(@"\[Http(Get|Post|Put|Delete)\]", RegexOptions.IgnoreCase);
var areaRegex = new Regex(@"\[Area\(""(?<area>[^""]+)""\)\]", RegexOptions.Compiled);
var classRegex = new Regex(@"class\s+(?<name>\w+Controller)\b", RegexOptions.Compiled);

foreach (var file in controllerFiles)
{
    var text = File.ReadAllText(file);
    var areaMatch = areaRegex.Match(text);
    var area = areaMatch.Success ? areaMatch.Groups["area"].Value : null;
    var classMatch = classRegex.Match(text);
    if (!classMatch.Success) continue;
    var controllerName = classMatch.Groups["name"].Value.Replace("Controller", "");
    var updated = false;
    var newText = text;
    var matches = methodRegex.Matches(text);
    int offset = 0;
    foreach (Match m in matches)
    {
        var xmlComment = m.Groups[1].Value;
        var swaggerOp = m.Groups[2].Value;
        var methodName = m.Groups["name"].Value;
        var paramsText = m.Groups["params"].Value;
        var methodStart = m.Index + offset;
        var methodLen = m.Length;

        // Find HTTP verb
        var before = newText.Substring(0, methodStart);
        var httpAttrMatch = httpAttrRegex.Matches(before).Cast<Match>().LastOrDefault();
        var verb = httpAttrMatch?.Groups[1].Value.ToUpper() ?? "GET";

        // Build endpoint path
        var basePath = area != null ? $"/Admin/{controllerName}" : $"/{controllerName}";
        var path = basePath + (methodName == "Index" ? "" : $"/{methodName}");
        if (Regex.IsMatch(paramsText, @"\b(?:int|long)\s+id\b")) path += "/{id}";

        // Get description from API_REFERENCE.md
        var desc = endpointDescriptions.TryGetValue(path, out var d) ? d : $"TODO: Describe {verb} {path}";

        // Prepare XML comment and SwaggerOperation
        var xmlSummary = $"/// <summary>\n    /// {desc}\n    /// </summary>\n    ";
        var swaggerAnnotation = $"[SwaggerOperation(Summary = \"{desc.Replace("\"", "\\\"") }\")]\n    ";

        // Replace or insert XML comment and SwaggerOperation
        var methodBlock = newText.Substring(methodStart, methodLen);
        var newBlock = methodBlock;
        if (!methodBlock.TrimStart().StartsWith("/// <summary>"))
        {
            newBlock = xmlSummary + newBlock;
            updated = true;
        }
        else
        {
            // Replace existing summary
            newBlock = Regex.Replace(newBlock, @"/// <summary>[\s\S]+?</summary>", xmlSummary.Trim(), RegexOptions.Multiline);
            updated = true;
        }
        if (!methodBlock.Contains("[SwaggerOperation"))
        {
            newBlock = xmlSummary + swaggerAnnotation + methodBlock.TrimStart();
            updated = true;
        }
        else
        {
            // Replace existing SwaggerOperation
            newBlock = Regex.Replace(newBlock, @"\[SwaggerOperation\([\s\S]+?\)\]", swaggerAnnotation.Trim(), RegexOptions.Multiline);
            updated = true;
        }
        // Replace in file text
        newText = newText.Remove(methodStart, methodLen).Insert(methodStart, newBlock);
        offset += newBlock.Length - methodBlock.Length;
    }
    if (updated && newText != text)
    {
        File.WriteAllText(file, newText);
        Console.WriteLine($"[UPDATED] {file} with XML comments and Swagger annotations.");
    }
}

Console.WriteLine("Controller annotation update complete.");
return 0;