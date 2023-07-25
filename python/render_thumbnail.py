import bpy
import time
import sys

def setup_material(filepath: str):
    image = bpy.data.images['SKY']
    image.source = 'FILE'
    image.filepath = filepath
    ...

def render(filepath: str | None = None):
    print("rendering")
    if (filepath is not None):
        bpy.context.scene.render.filepath = filepath

    t0 = time.time()
    bpy.ops.render.render(write_still=True)
    t1 = time.time()

    print(f'Finished rendering in {t1 - t0} seconds.')
    ...

if __name__ == '__main__':
    args = sys.argv
    
    exr_in = args[args.index('--exr') + 1]
    jpg_out = args[args.index('--output') + 1]

    setup_material(exr_in)
    render(jpg_out)
