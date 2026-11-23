"""
prepare_phase3_figures.py
Crops automated test overlays from high-resolution 1280x720 gameplay captures
to create clean, professional visual figures for Phase 3 Level 1 documentation.
"""

from pathlib import Path
from PIL import Image

SRC = Path(r"C:\Users\batin\OneDrive\Masaüstü\unityproje\EchoProtocol\Verification\Results\2026-11-09\Hardening")
DST = Path(r"C:\Users\batin\OneDrive\Masaüstü\unityproje\EchoProtocol\tmp\pdfs\phase3_assets")
DST.mkdir(parents=True, exist_ok=True)

captures = [
    ("game-1280x720-sample-service.png", "fig_checkpoint1_service.png"),
    ("game-1280x720-sample-transfer-open.png", "fig_checkpoint2_transfer.png"),
    ("game-1280x720-sample-final-plan.png", "fig_checkpoint3_evacuation.png"),
    ("game-1280x720-sample-combat.png", "fig_sentry_combat.png"),
    ("game-1280x720-sample-event-map.png", "fig_planning_map.png"),
    ("game-1280x720-immersion-sector-1.png", "fig_sector1_facility.png"),
    ("game-1280x720-sample-win.png", "fig_level_complete.png"),
]

for src_name, dst_name in captures:
    src_file = SRC / src_name
    if src_file.exists():
        im = Image.open(src_file).convert("RGB")
        w, h = im.size
        # Crop bottom 6.5% if there's an automated test bar, keeping crisp 16:9 view
        cropped = im.crop((0, 0, w, int(h * 0.935)))
        cropped.save(DST / dst_name, quality=95)
        print(f"Processed: {dst_name} ({cropped.size})")
    else:
        print(f"Warning: {src_name} not found!")

print("All Phase 3 figures prepared!")
