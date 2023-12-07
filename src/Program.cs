using ModuleSystem.ChecksumGenerator;
using ModuleSystem.ChecksumGenerator.Extensions;
using System.Reflection;

Console.WriteLine($"""
    Module System Checksum Generator [Version {Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "Undefined"}]
    (c) Arch Leaders. MIT License

    """);

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


Flags flags = Flags.Parse(args[2..]);

string outputFile = flags.Get("ChecksumMap.bin", "o", "output");
Console.WriteLine($"Registered output path: '{outputFile}'");

Generator generator = new();
foreach (var inputFolder in inputFolders) {
    Console.WriteLine($"Processing: {inputFolder}");
    generator.Collect(inputFolder, inputFolder.TryGetVersion());
}

Directory.CreateDirectory(Path.GetDirectoryName(outputFile) ?? string.Empty);
using FileStream fs = File.Create(outputFile);
generator.Write(fs);