# PolyHaven Asset Porting Toolbox

This is a utility tool made to assist in the process of porting [PolyHaven](https://polyhaven.com/) assets to S&Box. It isn't really meant to be used except internally, so the code is a mess and I don't have usage instructions for now.

Ping me on Discord (Igrium) if you want a rundown.

## Setup

Skybox thumbnails are rendered in Blender, so point the tool at your install before porting HDRIs:

```
ph_blender_path "/path/to/blender"
```

It's a `Saved` convar, so you only have to do it once. `skies.blend` and `python/render_thumbnail.py`
in this repo are what it renders with.

## Console commands

| Command | What |
|---|---|
| `ph_download_hdri <id>` | Port an HDRI into `materials/skybox/`, with a Blender-rendered thumbnail. |
| `ph_download_texture <id> [res] [aoRes]` | Port a texture set into `materials/<category>/`. Defaults `2k` / `1k`. |
| `ph_dump_assets` | Dump the PolyHaven asset list to the console. |

HDRIs and materials run through separate pipelines - `AssetCompilePipeline` handles skyboxes (Blender
thumbnail, `skybox.template`), `MaterialCompilePipeline` handles PBR materials (category subfolder,
`simple_standard` / `simple_metal` template depending on whether PolyHaven ships a metalness map).

## Publishing

Uploads are **stubbed out** while the rewrite is in progress - `AssetPublishing.DryRun` defaults to
`true`, so `Publish` logs and returns instead of pushing to asset.party. Flip it to `false` once the
generated packages have been eyeballed.

## Linux caveat

`CDirWatcher` isn't implemented on Linux ("directory watches will do nothing"), so the asset system
never notices files written underneath it - neither the textures we download nor the `_c` files the
resource compiler writes. Left alone, a freshly ported asset reads as `IsCompiled = false` with an
empty `CompiledFile`, and loading it fails with `ERROR_FILEOPEN: File not found` even though the `_c`
is correct on disk.

The pipeline works around it with explicit `AssetSystem.RegisterFile` calls at both ends:

- every downloaded texture, before generating the material that references it - otherwise the vmat
  compile fails with `Invalid Dependency Information` and then suppresses its own retries;
- the compiled `.vmat_c` and the `.generated.vtex_c` children afterwards, in
  `AssetCompilePipeline.RegisterCompiledOutput` - the vmat_c alone isn't enough, the engine rejects it
  as a "Parent with missing children".

With those in place a ported asset is usable in the same session, no restart needed.

A second Linux trap, in the same spirit: `Pixmap.FromFile` prefixes any path without a colon in it
with the `toolimages:` Qt search path. On Windows an absolute path always has one (`C:\`), on Linux it
never does, so the thumbnail came back as a pixmap that reported itself fine but silently failed to
save - the engine then kept its own flat equirect preview. `ThumbnailGenerator.AssignThumbnail`
decodes the PNG with `Bitmap.CreateFromBytes` and goes through `Pixmap.FromBitmap` instead.

Also: editing this project's C# while the editor is running triggers a hotload that kills the in-editor
MCP server (and with it the editor). Batch your edits and restart.
