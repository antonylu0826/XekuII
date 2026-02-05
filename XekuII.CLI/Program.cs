using XekuII.Generator;

// Parse command line arguments
var commandArgs = Environment.GetCommandLineArgs();

if (commandArgs.Length < 2)
{
    ShowHelp();
    return;
}

var command = commandArgs.Length > 1 ? commandArgs[1] : "help";

switch (command.ToLower())
{
    case "generate":
        RunGenerate(commandArgs);
        break;
    case "help":
    default:
        ShowHelp();
        break;
}

void RunGenerate(string[] arguments)
{
    var entitiesDir = "./entities";
    var outputDir = "./XekuII.Generated";
    var ns = "XekuII.Generated";
    var generateApi = false;
    var apiOutputDir = "./XekuII.WebApi/Controllers";

    // Parse arguments
    for (int i = 2; i < arguments.Length; i++)
    {
        if (arguments[i] == "--api")
        {
            generateApi = true;
        }
        else if (arguments[i].StartsWith("--entities="))
        {
            entitiesDir = arguments[i].Replace("--entities=", "");
        }
        else if (arguments[i].StartsWith("--output="))
        {
            outputDir = arguments[i].Replace("--output=", "");
        }
        else if (arguments[i].StartsWith("--namespace="))
        {
            ns = arguments[i].Replace("--namespace=", "");
        }
        else if (arguments[i].StartsWith("--api-output="))
        {
            apiOutputDir = arguments[i].Replace("--api-output=", "");
        }
        else if (!arguments[i].StartsWith("--"))
        {
            // Positional arguments for backward compatibility
            if (i == 2) entitiesDir = arguments[i];
            else if (i == 3) outputDir = arguments[i];
            else if (i == 4) ns = arguments[i];
        }
    }

    if (!Directory.Exists(entitiesDir))
    {
        Console.WriteLine($"? Error: Entities directory not found: {entitiesDir}");
        return;
    }

    Console.WriteLine("?? XekuII Code Generator");
    Console.WriteLine("======================");
    Console.WriteLine();

    var generator = new XekuGenerator(outputDir);
    generator.GenerateAll(entitiesDir, ns, generateApi, generateApi ? apiOutputDir : null);
}

void ShowHelp()
{
    Console.WriteLine("XekuII CLI - Code Generator for XAF");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  XekuII generate [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --entities=<dir>    Entities YAML directory (default: ./entities)");
    Console.WriteLine("  --output=<dir>      BO output directory (default: ./XekuII.Generated)");
    Console.WriteLine("  --namespace=<ns>    Target namespace (default: XekuII.Generated)");
    Console.WriteLine("  --api               Also generate API Controllers");
    Console.WriteLine("  --api-output=<dir>  API Controllers output (default: ./XekuII.WebApi/Controllers)");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  XekuII generate ./entities ./XekuII.Generated XekuII.Generated");
    Console.WriteLine("  XekuII generate --api");
    Console.WriteLine();
}
