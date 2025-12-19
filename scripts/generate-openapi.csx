#r "nuget: Microsoft.CodeAnalysis.CSharp, 4.10.0"
#r "nuget: YamlDotNet, 12.0.2"

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// Generates an OpenAPI-like JSON and YAML file (docs/openapi-generated.json|yaml)
// Scans controllers under EllisHope/Areas/Admin/Controllers and EllisHope/Controllers

var repoRoot = Directory.GetCurrentDirectory();
var controllerDirs = new[] { Path.Combine(repoRoot, "EllisHope/Areas/Admin/Controllers"), Path.Combine(repoRoot, "EllisHope/Controllers") };
var controllerFiles = new List<string>();
foreach (var d in controllerDirs)
{
    if (Directory.Exists(d)) controllerFiles.AddRange(Directory.GetFiles(d, "*.cs", SearchOption.AllDirectories));
}

if (!controllerFiles.Any())
{
    Console.WriteLine("No controller files found. Nothing generated.");
    return 0;
}

var doc = new Dictionary<string, object>();
var info = new Dictionary<string, object>
{
    ["title"] = "Ellis Hope Generated API",
    ["version"] = "1.0.0",
    ["description"] = "Generated from source by scripts/generate-openapi.csx"
};

doc["openapi"] = "3.0.3";
doc["info"] = info;
doc["paths"] = new Dictionary<string, Dictionary<string, object>>();

dynamic paths = doc["paths"];

foreach (var file in controllerFiles)
{
    var text = File.ReadAllText(file);
    var tree = CSharpSyntaxTree.ParseText(text);
    var root = tree.GetCompilationUnitRoot();

    var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
    if (classDecl == null) continue;

    // Area attribute
    var areaAttr = classDecl.AttributeLists.SelectMany(al => al.Attributes)
        .FirstOrDefault(a => a.Name.ToString().Contains("Area"));
    string area = null;
    if (areaAttr != null && areaAttr.ArgumentList?.Arguments.Count > 0)
    {
        area = areaAttr.ArgumentList.Arguments[0].ToString().Trim('"');
    }

    var controllerName = classDecl.Identifier.Text.Replace("Controller", "");
    var basePath = area != null ? $"/Admin/{controllerName}" : $"/{controllerName}";

    foreach (var method in classDecl.Members.OfType<MethodDeclarationSyntax>())
    {
        if (!method.Modifiers.Any(m => m.Text == "public")) continue;

        var methodName = method.Identifier.Text;

        // determine http verb from attributes
        var httpAttr = method.AttributeLists.SelectMany(al => al.Attributes)
            .FirstOrDefault(a => a.Name.ToString().StartsWith("Http", StringComparison.OrdinalIgnoreCase));
        string verb = "get";
        if (httpAttr != null)
        {
            var n = httpAttr.Name.ToString();
            if (n.StartsWith("HttpPost", StringComparison.OrdinalIgnoreCase)) verb = "post";
            else if (n.StartsWith("HttpPut", StringComparison.OrdinalIgnoreCase)) verb = "put";
            else if (n.StartsWith("HttpDelete", StringComparison.OrdinalIgnoreCase)) verb = "delete";
            else if (n.StartsWith("HttpGet", StringComparison.OrdinalIgnoreCase)) verb = "get";
        }
        else
        {
            // default: GET unless method name starts with Post/Put/Delete
            if (methodName.StartsWith("Post", StringComparison.OrdinalIgnoreCase) || methodName.StartsWith("Create", StringComparison.OrdinalIgnoreCase)) verb = "post";
            if (methodName.StartsWith("Put", StringComparison.OrdinalIgnoreCase) || methodName.StartsWith("Update", StringComparison.OrdinalIgnoreCase)) verb = "put";
            if (methodName.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) || methodName.StartsWith("Remove", StringComparison.OrdinalIgnoreCase)) verb = "delete";
        }

        // compute path
        var path = basePath + "/" + methodName;

        // if method is Index, also add basePath
        if (string.Equals(methodName, "Index", StringComparison.OrdinalIgnoreCase))
        {
            AddPath(paths, basePath, "get", new Dictionary<string, object> { ["summary"] = "Index" });
        }

        // detect id parameter
        var parameters = method.ParameterList.Parameters;
        if (parameters.Count > 0 && parameters[0].Type != null)
        {
            var firstParamType = parameters[0].Type.ToString();
            var firstParamName = parameters[0].Identifier.Text;
            if (Regex.IsMatch(firstParamName, "^id$", RegexOptions.IgnoreCase) && (firstParamType.Contains("int") || firstParamType.Contains("long")))
            {
                path += "/{id}";
            }
        }

        var operation = new Dictionary<string, object>
        {
            ["summary"] = methodName,
            ["responses"] = new Dictionary<string, object>
            {
                ["200"] = new Dictionary<string, object> { ["description"] = "OK" }
            }
        };

        // if POST and has complex parameter, add requestBody
        if (verb == "post")
        {
            // look for a parameter that is not primitive
            var complex = parameters.FirstOrDefault(p => !IsPrimitiveType(p.Type?.ToString()));
            if (complex != null)
            {
                operation["requestBody"] = new Dictionary<string, object>
                {
                    ["content"] = new Dictionary<string, object>
                    {
                        ["application/x-www-form-urlencoded"] = new Dictionary<string, object>
                        {
                            ["schema"] = new Dictionary<string, object> { ["type"] = "object" }
                        }
                    }
                };
            }
            else
            {
                // generic form
                operation["requestBody"] = new Dictionary<string, object>
                {
                    ["content"] = new Dictionary<string, object>
                    {
                        ["application/x-www-form-urlencoded"] = new Dictionary<string, object>
                        {
                            ["schema"] = new Dictionary<string, object> { ["type"] = "object" }
                        }
                    }
                };
            }

            // typical redirect response
            var responses = (Dictionary<string, object>)operation["responses"];
            responses["302"] = new Dictionary<string, object> { ["description"] = "Redirect" };
        }

        AddPath(paths, path, verb, operation);
    }
}

// write JSON and YAML
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
var json = JsonSerializer.Serialize(doc, jsonOptions);
File.WriteAllText(Path.Combine(repoRoot, "docs", "openapi-generated.json"), json);

var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
var yaml = serializer.Serialize(doc);
File.WriteAllText(Path.Combine(repoRoot, "docs", "openapi-generated.yaml"), yaml);

Console.WriteLine("Generated docs/openapi-generated.json and docs/openapi-generated.yaml");
return 0;


bool IsPrimitiveType(string t)
{
    if (string.IsNullOrWhiteSpace(t)) return false;
    t = t.ToLowerInvariant();
    return t.Contains("int") || t.Contains("long") || t.Contains("string") || t.Contains("bool") || t.Contains("datetime") || t.Contains("decimal") || t.Contains("double") || t.Contains("float");
}

void AddPath(dynamic pathsObj, string path, string verb, Dictionary<string, object> operation)
{
    var dict = (IDictionary<string, Dictionary<string, object>>)pathsObj;
    if (!dict.ContainsKey(path)) dict[path] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    var inner = dict[path];
    inner[verb] = operation;
}
