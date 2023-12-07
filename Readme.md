# Nintendo Module System Checksum Generator

A simple script used to generated a file checksums for ModuleSystem games.

## Usage

```sh
ChecksumGenerator.exe <path> [-o|--output OUTPUT]
```

- **Path:** Bar (`|`) separated list of game paths (for multiple game versions) [Required]
- **Output:** The output folder for the generated checksum binaries (default `./output`) [Optional]

### In-app help message

```sh
ChecksumGenerator.exe -h|--help
```