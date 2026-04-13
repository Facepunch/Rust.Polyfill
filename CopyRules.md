### How/what to copy over

## Facepunch.System
- Copy everything
- Delete:
  - BurstUtil.cs
  - Tests/
  - Math/NativeGrid.cs
  - Math/NativeParallelGrid.cs
  - Collections/NativeMemoryStream.cs
- Patch:
  - Memoized.cs: Remove prop: KeyCodeToString 
  - Collections/ListHashSet.cs;GetRandon(): Replace `return vals[UnityEngine.Random.Range(0, count)];` with `return vals[Random.Shared.Next(0, count)];`

## Rust.Data
- copy everything (preferably without .meta but whatever)
- Delete
  - Half3.cs

### Handy Bits
Delete all .meta and .asmdef files recursively from CWD (after copying files)

`Get-ChildItem -Path . -Recurse -File -Include *.meta,*.asmdef -Force | Remove-Item -Force`