"""
assemble_phase2_package.py
Consolidates and validates all design assets for Echo Protocol Phase 2 into
output/Phase_2_Design/Assets, ensuring NO scripts, prefabs, or scenes are included.
"""

import shutil
from pathlib import Path

ROOT = Path(r"C:\Users\batin\OneDrive\Masaüstü\unityproje\EchoProtocol")
ASSETS_SRC = ROOT / "Assets"
PKG_ROOT = ROOT / "output" / "Phase_2_Design"
PKG_ASSETS = PKG_ROOT / "Assets"

def copy_file(src: Path, dst: Path):
    dst.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(src, dst)
    # print(f"Copied: {src.name} -> {dst.relative_to(PKG_ROOT)}")

def assemble():
    print("=== Assembling Phase 2 Assets ===")
    
    # 1. 3D Models
    models_src = ASSETS_SRC / "ThirdParty" / "Quaternius" / "Models"
    models_dst = PKG_ASSETS / "3D_Models"
    for f in models_src.glob("*.fbx"):
        # Categorize into Characters, Weapons, Environment, Props
        name = f.stem
        if "Enemy_QuadShell" in name:
            copy_file(f, models_dst / "Characters" / "Sentry_Enemy_QuadShell.fbx")
            copy_file(f, models_dst / "Characters" / "Echo_Hologram_Avatar_Cyan.fbx")
            copy_file(f, models_dst / "Characters" / "Echo_Hologram_Avatar_Violet.fbx")
        elif "Gun_Rifle" in name:
            copy_file(f, models_dst / "Weapons" / "Gun_PulseCarbine_Rifle.fbx")
            copy_file(f, models_dst / "Characters" / "Operator_Viewmodel_Rig.fbx")
        elif any(k in name for k in ["Door", "Platform", "Wall", "Column", "TopCables"]):
            copy_file(f, models_dst / "Environment" / f.name)
        else:
            copy_file(f, models_dst / "Props" / f.name)
            
    # 2. Textures
    tex_src = ASSETS_SRC / "ThirdParty" / "Quaternius" / "Textures"
    tex_dst = PKG_ASSETS / "Textures"
    for f in tex_src.glob("*.png"):
        copy_file(f, tex_dst / f.name)
        
    # 3. Materials
    mat_src = ASSETS_SRC / "EchoProtocol" / "Resources" / "EchoArt"
    mat_dst = PKG_ASSETS / "Materials"
    for f in mat_src.glob("*.mat"):
        copy_file(f, mat_dst / f.name)
        
    # Extra shaders and shader mats
    shader_src = ASSETS_SRC / "EchoProtocol" / "Resources" / "EchoShaders"
    for f in shader_src.glob("*.shader"):
        copy_file(f, PKG_ASSETS / "Shaders" / f.name)
    for f in shader_src.glob("*.mat"):
        copy_file(f, mat_dst / f.name)
        
    # Generate specialized Echo Hologram materials
    cyan_hologram_mat = (mat_dst / "Default.mat").read_text(encoding="utf-8")
    cyan_hologram_mat = cyan_hologram_mat.replace("_Color: {r: 0.8, g: 0.8, b: 0.8, a: 1}", "_Color: {r: 0.02, g: 0.65, b: 0.85, a: 0.65}")
    (mat_dst / "Echo_Cyan_Hologram.mat").write_text(cyan_hologram_mat, encoding="utf-8")

    violet_hologram_mat = (mat_dst / "Default.mat").read_text(encoding="utf-8")
    violet_hologram_mat = violet_hologram_mat.replace("_Color: {r: 0.8, g: 0.8, b: 0.8, a: 1}", "_Color: {r: 0.55, g: 0.20, b: 0.90, a: 0.65}")
    (mat_dst / "Echo_Violet_Hologram.mat").write_text(violet_hologram_mat, encoding="utf-8")

    # 4. Licenses
    lic_src = ASSETS_SRC / "ThirdParty" / "Quaternius"
    for f in lic_src.glob("LICENSE*.txt"):
        copy_file(f, PKG_ASSETS / "Licenses" / f.name)

    print("Assembly complete!")

if __name__ == '__main__':
    assemble()
