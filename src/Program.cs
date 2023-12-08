using ModuleSystem.ChecksumGenerator;
using ModuleSystem.ChecksumGenerator.Extensions;
using System.Reflection;

string title = $"Module System Checksum Generator [Version {Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "Undefined"}]";
Console.WriteLine($"""
    {title}
    (c) Arch Leaders. MIT License

    """);

Console.Title = title;

if (args.Length <= 0 || args[0] is "-h" or "--help") {
    Console.WriteLine("""
        Usage:
          ChecksumGenerator <path1|path2> [-o|--output OUTPUT]
        """);

    return;
}

List<string> inputFolders = args[0].Split('|')
    .Where(x => File.Exists(Path.Combine(x, "Pack", "ZsDic.pack.zs")))
    .ToList();

if (inputFolders.Count <= 0) {
    throw new ArgumentException("No valid input folder!");
}


Flags flags = Flags.Parse(args[1..]);

string outputFile = flags.Get("ChecksumMap.bin", "o", "output");
Console.WriteLine($"Registered output path: '{outputFile}'");

Generator generator = new();
foreach (var inputFolder in inputFolders) {
    Console.WriteLine($"\nProcessing: {inputFolder}");
    generator.Collect(inputFolder, inputFolder.TryGetVersion());
}

string? dir = Path.GetDirectoryName(outputFile);
if (!string.IsNullOrEmpty(dir)) {
    Directory.CreateDirectory(dir);
}

using FileStream fs = File.Create(outputFile);
generator.Write(fs);