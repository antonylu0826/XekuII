using XekuII.Generator;

// XekuII Generator CLI
if (args.Length < 1)
{
    Console.WriteLine("XekuII Generator - Generate BO, API Controllers, and Frontend from YAML");
    Console.WriteLine("=================");
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run -- <entities-dir> [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --output <bo-dir>          BO output directory (default: ../XekuII.ApiHost/BusinessObjects)");
    Console.WriteLine("  --controllers <api-dir>    API Controllers output directory");
    Console.WriteLine("  --frontend <web-dir>       Frontend generated output directory");
    Console.WriteLine("  --namespace <ns>           BO namespace (default: XekuII.ApiHost.BusinessObjects)");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run -- ../entities --output ../XekuII.ApiHost/BusinessObjects --controllers ../XekuII.ApiHost/API");
    Console.WriteLine("  dotnet run -- ../entities --output ../XekuII.ApiHost/BusinessObjects --controllers ../XekuII.ApiHost/API --frontend ../xekuii-web/src/generated");
    return;
}

// Parse arguments
var entitiesDir = args[0];
var boOutputDir = "../XekuII.ApiHost/BusinessObjects";
var controllersOutputDir = (string?)null;
var frontendOutputDir = (string?)null;
var targetNamespace = "XekuII.ApiHost.BusinessObjects";

for (int i = 1; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--output":
            if (i + 1 < args.Length) boOutputDir = args[++i];
            break;
        case "--controllers":
            if (i + 1 < args.Length) controllersOutputDir = args[++i];
            break;
        case "--frontend":
            if (i + 1 < args.Length) frontendOutputDir = args[++i];
            break;
        case "--namespace":
            if (i + 1 < args.Length) targetNamespace = args[++i];
            break;
    }
}

Console.WriteLine("🚀 Running XekuII Generator");
Console.WriteLine("=================");
Console.WriteLine($"📂 Entities:     {entitiesDir}");
Console.WriteLine($"📁 BO Output:   {boOutputDir}");
if (controllersOutputDir != null) Console.WriteLine($"📁 Controllers: {controllersOutputDir}");
if (frontendOutputDir != null) Console.WriteLine($"🌐 Frontend:    {frontendOutputDir}");
Console.WriteLine($"📦 Namespace:   {targetNamespace}");
Console.WriteLine();

// Run generator
var generator = new XekuGenerator(boOutputDir);
generator.GenerateAll(
    entitiesDir,
    targetNamespace,
    generateControllers: controllersOutputDir != null,
    controllersOutputDir,
    generateFrontend: frontendOutputDir != null,
    frontendOutputDir
);

Console.WriteLine();
Console.WriteLine("✅ Generation complete!");
