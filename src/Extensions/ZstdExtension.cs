using SarcLibrary;
using ZstdSharp;

namespace ModuleSystem.ChecksumGenerator.Extensions;

public class ZstdExtension
{
    private readonly Decompressor _defaultDecompressor = new();
    private readonly Dictionary<string, Decompressor> _decompressors = [];

    public ZstdExtension(string zsDicFile)
    {
        if (File.Exists(zsDicFile)) {
            Span<byte> data = _defaultDecompressor.Unwrap(File.ReadAllBytes(zsDicFile));
            SarcFile sarc = SarcFile.FromBinary(data.ToArray());

            foreach ((var file, var fileData) in sarc) {
                Decompressor decompressor = new();
                decompressor.LoadDictionary(fileData);
                _decompressors[file[..file.LastIndexOf('.')]] = decompressor;
            }
        }
    }

    public byte[] Decompress(byte[] buffer, string file)
    {
        if (!file.EndsWith(".zs")) {
            return buffer;
        }

        try {
            foreach ((var key, var decompressor) in _decompressors) {
                if (file.EndsWith($"{key}.zs")) {
                    return decompressor.Unwrap(buffer).ToArray();
                }
            }

            if (_decompressors.TryGetValue("zs", out Decompressor? common)) {
                return common.Unwrap(buffer).ToArray();
            }

            return _defaultDecompressor.Unwrap(buffer).ToArray();
        }
        catch (Exception ex) {
            throw new Exception($"Could not decompress '{file}'", ex);
        }
    }
}
