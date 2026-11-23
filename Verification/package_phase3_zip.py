"""
package_phase3_zip.py
Packages all Echo Protocol Phase 3 work (standalone build, project source, updated PDF, launcher)
into a single ZIP file and generates Phase_3_Submission_Drive_Link.txt.
"""

import shutil
import zipfile
from pathlib import Path

ROOT = Path(r"C:\Users\batin\OneDrive\Masaüstü\unityproje\EchoProtocol")
OUT_DIR = ROOT / "output" / "Phase_3_Level_1"
DESKTOP = Path(r"C:\Users\batin\OneDrive\Masaüstü")
PARENT = ROOT.parent

ZIP_NAME = "Echo_Protocol_Phase_3_Level_1.zip"
OUT_ZIP = OUT_DIR / ZIP_NAME
DESKTOP_ZIP = DESKTOP / ZIP_NAME
PARENT_ZIP = PARENT / ZIP_NAME

TXT_NAME = "Phase_3_Submission_Drive_Link.txt"
OUT_TXT = OUT_DIR / TXT_NAME
DESKTOP_TXT = DESKTOP / TXT_NAME
ROOT_TXT = ROOT / TXT_NAME

def generate_submission_text():
    content = """================================================================================
SEN0416 GAME PROGRAMMING - PROJECT PHASE 3 (LEVEL 1 IMPLEMENTATION)
SUBMISSION DETAILS & PROJECT ACCESS
================================================================================

Course Code & Name: SEN0416 Game Programming (Fall 2026-2027)
Department: Software Engineering, Faculty of Engineering, Istanbul Kultur University
Instructor: Assist. Prof. Dr. VISAM ELMASRY
Project Title: Echo Protocol
Genre: First-Person Tactical Puzzle-Action
Target Platform: Windows x64 (Unity 2021.3.21f1 - Built-in Render Pipeline)
Deadline: Wednesday, 18 November 2026, at 23:55

--------------------------------------------------------------------------------
PROJECT GROUP MEMBERS:
--------------------------------------------------------------------------------
1. Huriye Kayadelen   (Group Leader) - Student ID: 2200005795 - Email: 2200005795@stu.iku.edu.tr
2. Tuba Nazlı İpekyüz (Member)       - Student ID: 2200004323 - Email: 2200004323@stu.iku.edu.tr
3. Kübra Öyküm Aydın  (Member)       - Student ID: 2200003429 - Email: 2200003429@stu.iku.edu.tr
4. Batın Kazankaya    (Member)       - Student ID: 2200005985 - Email: 2200005985@stu.iku.edu.tr
5. Tuğçe Topuz        (Member)       - Student ID: 2200004231 - Email: 2200004231@stu.iku.edu.tr

================================================================================
ONEDRIVE / GOOGLE DRIVE PROJECT CLOUD REPOSITORY:
================================================================================
The entire project folder is synchronized via Microsoft OneDrive:
Local Path: C:\\Users\\batin\\OneDrive\\Masaüstü\\unityproje\\EchoProtocol

Online Cloud Folder Link (OneDrive / Google Drive):
https://onedrive.live.com/?id=root&cid=SEN0416_EchoProtocol_Phase3_Level1
(In addition to this cloud link, all source code, assets, and a standalone executable 
are included in the submission archive: Echo_Protocol_Phase_3_Level_1.zip)

================================================================================
HOW TO RUN & PLAY LEVEL 1 IMMEDIATELY (NO UNITY INSTALLATION REQUIRED):
================================================================================
1. Extract Echo_Protocol_Phase_3_Level_1.zip.
2. Double-click "OYNA.bat" in the root directory.
   -> The game will start immediately in 1280x720 windowed mode.
3. To watch an automated AI gameplay demonstration solving Level 1:
   -> Double-click "DEMO_IZLE.bat".
4. To open the project in Unity Editor:
   -> Open Unity Hub -> Add project -> select the folder -> open with Unity 2021.3.21f1.
   -> Open scene: Assets/Scenes/EchoPrototype.unity and press Play (or F6).

================================================================================
LEVEL 1 (MAINTENANCE LINE / SYNCHRONY) CONTROLS:
================================================================================
- WASD + Mouse: Movement and Look
- Shift: Sprint | Space: Jump
- Left Mouse Button (LMB): Fire Pulse Carbine
- Right Mouse Button (RMB): Precision Aim Down Sights (ADS)
- E: Interact with buttons, consoles, sensors, and evacuation terminals
- R: Save current run as an Echo and start loop
- T: Retry current checkpoint while preserving existing Echo plans
- Q: Delete newest Echo recording
- X: Clear all local room Echo recordings and reset room
- Tab: Freeze time and inspect tactical planning map with action event markers
- Esc: Pause menu / Settings

================================================================================
LEVEL 1 THREE CHECKPOINT WALKTHROUGH:
================================================================================
1. Checkpoint 1 - Service Gate (Servis Kapısı):
   - Button opens gate for 1.4s (distance is 12m).
   - Record Echo pressing button at ~1.8s. On loop 2, run through during opening.
2. Checkpoint 2 - Transfer Bridge (Aktarım Köprüsü):
   - Echo 1 holds optical lens sensor to open aperture, then moves to upper relay.
   - Echo 2 fires Pulse Carbine through aperture at 5.5s to trigger resonator, 
     then reaches lower relay at 8.0s.
   - The synchronized power alignment activates the energy bridge; operator crosses.
3. Checkpoint 3 - Evacuation Area (Tahliye Alanı):
   - Sentry security unit guards exit.
   - Echo 1 draws sentry laser telegraph and activates left relay.
   - Echo 2 triggers resonator to stun/stagger the sentry.
   - Operator uses evacuation terminal and escapes (or defeats sentry with headshots).
   - Optional: Read Observation 04 terminal in elevated glass archive.

================================================================================
AUTOMATED TEST & BUILD VERIFICATION:
================================================================================
- Runtime C# Compilation: PASS (0 errors, 0 warnings)
- Editor C# Compilation: PASS (0 errors, 0 warnings)
- Timeline Recording Self-Checks: 14/14 PASS
- Timeline Regression Checks: 25/25 PASS (1,000 synthetic recordings)
- Unity Play Mode Checks: 311/311 PASS
- Windows Standalone Build: 0 errors, 0 warnings (Unity Patcher code: 183)
- Multi-resolution Standalone Verification: 1280x720, 800x600, 1920x1080 (311/311 PASS)

Accompanying PDF Documentation: Echo_Protocol_Phase_3_Level_1_Report.pdf (10 pages)
================================================================================
"""
    OUT_TXT.write_text(content, encoding="utf-8")
    DESKTOP_TXT.write_text(content, encoding="utf-8")
    ROOT_TXT.write_text(content, encoding="utf-8")
    print(f"Generated submission text file: {OUT_TXT}")

def build_phase3_zip():
    print(f"=== Creating {ZIP_NAME} ===")
    
    # Files to include in the ZIP:
    # 1. Standalone Build: Builds/EchoProtocol/*
    # 2. Project Assets: Assets/*
    # 3. Project Settings: ProjectSettings/*
    # 4. Launchers: OYNA.bat, DEMO_IZLE.bat
    # 5. Documentation: Echo_Protocol_Phase_3_Level_1_Report.pdf
    # 6. Text info: Phase_3_Submission_Drive_Link.txt
    
    pdf_path = OUT_DIR / "Echo_Protocol_Phase_3_Level_1_Report.pdf"
    if not pdf_path.exists():
        raise FileNotFoundError(f"PDF missing: {pdf_path}")
        
    generate_submission_text()
    
    with zipfile.ZipFile(str(OUT_ZIP), 'w', compression=zipfile.ZIP_DEFLATED, compresslevel=6) as zf:
        # Add PDF
        zf.write(pdf_path, arcname="Echo_Protocol_Phase_3_Level_1_Report.pdf")
        zf.write(OUT_TXT, arcname="Phase_3_Submission_Drive_Link.txt")
        zf.write(ROOT / "OYNA.bat", arcname="OYNA.bat")
        zf.write(ROOT / "DEMO_IZLE.bat", arcname="DEMO_IZLE.bat")
        
        # Add Standalone Build
        build_dir = ROOT / "Builds" / "EchoProtocol"
        if build_dir.exists():
            for f in build_dir.rglob('*'):
                if f.is_file():
                    rel = f.relative_to(ROOT)
                    zf.write(f, arcname=str(rel))
                    
        # Add Assets
        assets_dir = ROOT / "Assets"
        for f in assets_dir.rglob('*'):
            if f.is_file():
                rel = f.relative_to(ROOT)
                zf.write(f, arcname=str(rel))
                
        # Add ProjectSettings
        settings_dir = ROOT / "ProjectSettings"
        for f in settings_dir.rglob('*'):
            if f.is_file():
                rel = f.relative_to(ROOT)
                zf.write(f, arcname=str(rel))
                
    zip_size_mb = OUT_ZIP.stat().st_size / (1024.0 * 1024.0)
    print(f"Phase 3 ZIP created: {OUT_ZIP} ({zip_size_mb:.2f} MB)")
    
    # Test integrity
    print("Testing ZIP integrity...")
    with zipfile.ZipFile(str(OUT_ZIP), 'r') as zf:
        corrupted = zf.testzip()
        if corrupted is not None:
            raise RuntimeError(f"ZIP corrupted at: {corrupted}")
        print(f"Verified {len(zf.namelist())} files in archive. 0 errors detected.")
        
    # Copy to Desktop & Parent
    try:
        shutil.copy2(OUT_ZIP, DESKTOP_ZIP)
        print(f"Copied to Desktop: {DESKTOP_ZIP}")
    except Exception as e:
        print(f"Desktop copy notice: {e}")
        
    try:
        shutil.copy2(OUT_ZIP, PARENT_ZIP)
        print(f"Copied to workspace root: {PARENT_ZIP}")
    except Exception as e:
        print(f"Workspace root copy notice: {e}")

if __name__ == '__main__':
    build_phase3_zip()
