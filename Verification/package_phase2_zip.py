"""
package_phase2_zip.py
Packages all Echo Protocol Phase 2 design assets and the contents catalog PDF
into a single, verified submission ZIP file.
"""

import shutil
import zipfile
from pathlib import Path

ROOT = Path(r"C:\Users\batin\OneDrive\Masaüstü\unityproje\EchoProtocol")
SRC_DIR = ROOT / "output" / "Phase_2_Design"
ZIP_NAME = "Echo_Protocol_Phase_2_Design.zip"
OUT_ZIP = ROOT / "output" / ZIP_NAME
DESKTOP_ZIP = Path(r"C:\Users\batin\OneDrive\Masaüstü") / ZIP_NAME
PARENT_ZIP = ROOT.parent / ZIP_NAME

def build_zip():
    print(f"=== Creating {ZIP_NAME} ===")
    
    # 1. Validation check before zipping
    all_files = [f for f in SRC_DIR.rglob('*') if f.is_file()]
    print(f"Total files to package: {len(all_files)}")
    
    forbidden = [f for f in all_files if f.suffix.lower() in ['.cs', '.prefab', '.unity']]
    if forbidden:
        raise ValueError(f"CRITICAL: Found forbidden files in Phase 2 package: {forbidden}")
        
    pdf_found = any(f.name == "Echo_Protocol_Phase_2_Design_Contents.pdf" for f in all_files)
    if not pdf_found:
        raise ValueError("CRITICAL: Contents catalog PDF is missing from the package root!")
        
    # 2. Build ZIP
    OUT_ZIP.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(str(OUT_ZIP), 'w', compression=zipfile.ZIP_DEFLATED, compresslevel=9) as zf:
        for f in all_files:
            rel_path = f.relative_to(SRC_DIR)
            zf.write(f, arcname=str(rel_path))
            
    zip_size_mb = OUT_ZIP.stat().st_size / (1024.0 * 1024.0)
    print(f"ZIP created successfully: {OUT_ZIP} ({zip_size_mb:.2f} MB)")
    
    # 3. Test ZIP integrity
    print("Testing ZIP integrity...")
    with zipfile.ZipFile(str(OUT_ZIP), 'r') as zf:
        corrupted = zf.testzip()
        if corrupted is not None:
            raise RuntimeError(f"ZIP file corrupted at: {corrupted}")
        namelist = zf.namelist()
        print(f"Verified {len(namelist)} items in archive. 0 errors detected.")
        
    # 4. Copy to Desktop & Parent folder for easy submission
    try:
        shutil.copy2(OUT_ZIP, DESKTOP_ZIP)
        print(f"Copied to Desktop: {DESKTOP_ZIP}")
    except Exception as e:
        print(f"Could not copy to Desktop: {e}")
        
    try:
        shutil.copy2(OUT_ZIP, PARENT_ZIP)
        print(f"Copied to workspace root: {PARENT_ZIP}")
    except Exception as e:
        print(f"Could not copy to workspace root: {e}")

if __name__ == '__main__':
    build_zip()
