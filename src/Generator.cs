using ModuleSystem.ChecksumGenerator.Extensions;
using SarcLibrary;
using Standart.Hash.xxHash;
using System.Buffers.Binary;

namespace ModuleSystem.ChecksumGenerator;

public class Generator
{
    private int _count = 0;
    private readonly List<ulong> _keys = [];
    private readonly List<ulong> _checksums = [];

    public void Collect(string inputFolder, int version)
    {
        ZstdExtension zstd = new(Path.Combine(inputFolder, "Pack", "ZsDic.pack.zs"));
        foreach (var file in Directory.GetFiles(inputFolder, "*.*", SearchOption.AllDirectories)) {
            CollectFile(file, File.ReadAllBytes(file), inputFolder, version, zstd);
        }
    }

    public void Write(in Stream output)
    {
        Span<byte> qword = stackalloc byte[sizeof(ulong)];

        foreach (var key in _keys) {
            BinaryPrimitives.WriteUInt64LittleEndian(qword, key);
            output.Write(qword);
        }

        foreach (var checksum in _checksums) {
            BinaryPrimitives.WriteUInt64LittleEndian(qword, checksum);
            output.Write(qword);
        }
    }

    public void CollectFile(string file, byte[] src, string root, int version, ZstdExtension zstd)
    {
        if (src.Length <= 0) {
            return;
        }

        byte[] buffer = zstd.Decompress(src, file);
        AddChecksum(Path.Combine(file), buffer, root, version);

        if (buffer.Length > 4 && buffer.AsSpan()[0..4].SequenceEqual("SARC"u8)) {
            SarcFile sarc = SarcFile.FromBinary(buffer);
            foreach ((var fileName, var data) in sarc) {
                CollectFile(Path.Combine(file, fileName), data, root, version, zstd);
            }
        }
    }

    public void AddChecksum(string file, byte[] buffer, string root, int version)
    {
        Console.Write($"\r{++_count}");
        
        string path = Path.GetRelativePath(root, file).Replace('\\', '/');
        ulong key = xxHash64.ComputeHash(path);
        ulong checksum = xxHash64.ComputeHash(buffer);

        int index;

        // If the key exists with a different
        // value add a version specific entry
        if ((index = _keys.IndexOf(key)) >= 0) {
            if (_checksums[index] == checksum) {
                return;
            }

            key = xxHash64.ComputeHash(path += $"#{version}");
        }

        _keys.Add(key);
        _checksums.Add(checksum);
    }
}
