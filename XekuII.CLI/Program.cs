using XekuII.CLI.Models;
using XekuII.CLI.Services;
using XekuII.Generator;

var commandArgs = Environment.GetCommandLineArgs();
var workingDirectory = Directory.GetCurrentDirectory();

if (commandArgs.Length < 2)
{
    ShowHelp();
    return 0;
}

var command = commandArgs[1].ToLower();

return command switch
{
    "generate" => RunGenerate(commandArgs),
    "start" => await RunStartAsync(commandArgs, workingDirectory),
    "stop" => RunStop(commandArgs, workingDirectory),
    "status" => RunStatus(workingDirectory),
    "help" or "--help" or "-h" => ShowHelp(),
    _ => ShowHelp()
};

int RunGenerate(string[] arguments)
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
        Console.WriteLine($"Error: Entities directory not found: {entitiesDir}");
        return 1;
    }

    Console.WriteLine("XekuII Code Generator");
    Console.WriteLine("=====================");
    Console.WriteLine();

    var generator = new XekuGenerator(outputDir);
    generator.GenerateAll(entitiesDir, ns, generateApi, generateApi ? apiOutputDir : null);
    return 0;
}

async Task<int> RunStartAsync(string[] arguments, string workDir)
{
    var (backend, frontend, all) = ParseServiceFlags(arguments);

    Console.WriteLine();
    Console.WriteLine("  XekuII 服務管理");
    Console.WriteLine("  ===============");
    Console.WriteLine();

    using var manager = new ServiceManager(workDir);
    var results = await manager.StartServicesAsync(backend, frontend, all);

    foreach (var result in results)
    {
        if (result.Success)
        {
            Console.WriteLine($"  [OK] {result.ServiceName}");
            Console.WriteLine($"       PID:  {result.ProcessId}");
            Console.WriteLine($"       Port: {result.Port}");
            Console.WriteLine($"       URL:  {result.Url}");
            if (result.Message != null)
            {
                Console.WriteLine($"       Note: {result.Message}");
            }
        }
        else
        {
            Console.WriteLine($"  [FAIL] {result.ServiceName}");
            Console.WriteLine($"         Error: {result.Message}");
        }
        Console.WriteLine();
    }

    return results.All(r => r.Success) ? 0 : 1;
}

int RunStop(string[] arguments, string workDir)
{
    var (backend, frontend, all) = ParseServiceFlags(arguments);

    Console.WriteLine();
    Console.WriteLine("  停止 XekuII 服務");
    Console.WriteLine("  ================");
    Console.WriteLine();

    using var manager = new ServiceManager(workDir);
    var results = manager.StopServices(backend, frontend, all);

    foreach (var result in results)
    {
        var status = result.Success ? "[OK]" : "[FAIL]";
        Console.WriteLine($"  {status} {result.ServiceName}: {result.Message ?? "已停止"}");
    }

    Console.WriteLine();
    return results.All(r => r.Success) ? 0 : 1;
}

int RunStatus(string workDir)
{
    Console.WriteLine();
    Console.WriteLine("  XekuII 服務狀態");
    Console.WriteLine("  ===============");
    Console.WriteLine();

    using var manager = new ServiceManager(workDir);
    var statuses = manager.GetStatus();

    // Table header
    Console.WriteLine($"  {"服務",-12} {"狀態",-12} {"PID",-10} {"端口",-8} {"啟動時間",-20}");
    Console.WriteLine($"  {new string('-', 12)} {new string('-', 12)} {new string('-', 10)} {new string('-', 8)} {new string('-', 20)}");

    foreach (var status in statuses)
    {
        var statusText = status.Status switch
        {
            ServiceStatus.Running => "Running",
            ServiceStatus.Starting => "Starting",
            ServiceStatus.Unhealthy => "Unhealthy",
            ServiceStatus.Error => "Error",
            _ => "Stopped"
        };

        var pid = status.ProcessId?.ToString() ?? "-";
        var startTime = status.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";

        Console.WriteLine($"  {status.Name,-12} {statusText,-12} {pid,-10} {status.Port,-8} {startTime,-20}");

        if (status.Note != null)
        {
            Console.WriteLine($"  {"",12} Note: {status.Note}");
        }
    }

    Console.WriteLine();
    return 0;
}

(bool backend, bool frontend, bool all) ParseServiceFlags(string[] arguments)
{
    bool backend = false, frontend = false, all = false;

    for (int i = 2; i < arguments.Length; i++)
    {
        switch (arguments[i].ToLower())
        {
            case "--backend" or "-b":
                backend = true;
                break;
            case "--frontend" or "-f":
                frontend = true;
                break;
            case "--all" or "-a":
                all = true;
                break;
        }
    }

    // Default to all if no flags specified
    if (!backend && !frontend && !all)
    {
        all = true;
    }

    return (backend, frontend, all);
}

int ShowHelp()
{
    Console.WriteLine("XekuII CLI - YAML 驅動開發工具");
    Console.WriteLine();
    Console.WriteLine("命令:");
    Console.WriteLine("  generate     從 YAML 生成代碼");
    Console.WriteLine("  start        啟動服務");
    Console.WriteLine("  stop         停止服務");
    Console.WriteLine("  status       顯示服務狀態");
    Console.WriteLine();
    Console.WriteLine("服務管理選項:");
    Console.WriteLine("  --backend, -b    僅後端 API (localhost:5000)");
    Console.WriteLine("  --frontend, -f   僅前端 Web (localhost:5173)");
    Console.WriteLine("  --all, -a        全部服務 (預設)");
    Console.WriteLine();
    Console.WriteLine("範例:");
    Console.WriteLine("  xekuii start                    # 啟動所有服務");
    Console.WriteLine("  xekuii start --backend          # 僅啟動後端");
    Console.WriteLine("  xekuii stop --frontend          # 僅停止前端");
    Console.WriteLine("  xekuii status                   # 顯示狀態");
    Console.WriteLine();
    Console.WriteLine("代碼生成選項:");
    Console.WriteLine("  --entities=<dir>    YAML 目錄 (預設: ./entities)");
    Console.WriteLine("  --output=<dir>      BO 輸出目錄");
    Console.WriteLine("  --api               生成 API Controllers");
    Console.WriteLine("  --api-output=<dir>  Controllers 輸出目錄");
    Console.WriteLine();
    return 0;
}
