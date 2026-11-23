"""
generate_phase2_sprites.py
Generates crisp, high-resolution 2D Sprites and UI assets with alpha transparency
for Echo Protocol Phase 2 (Design Phase) submission.
"""

import math
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

# Color Palette
CYAN = (39, 196, 216, 255)
CYAN_GLOW = (39, 196, 216, 120)
VIOLET = (169, 147, 242, 255)
AMBER = (230, 169, 75, 255)
RED = (235, 75, 75, 255)
WHITE = (255, 255, 255, 255)
DARK_NAVY = (11, 20, 32, 240)
PANEL_BG = (18, 34, 53, 220)
BORDER_LINE = (55, 85, 115, 255)

def get_font(size: int, bold: bool = False):
    font_path = r"C:\Windows\Fonts\arialbd.ttf" if bold else r"C:\Windows\Fonts\arial.ttf"
    try:
        return ImageFont.truetype(font_path, size)
    except:
        return ImageFont.load_default()

def draw_rounded_rect(draw: ImageDraw.ImageDraw, xy, radius, fill=None, outline=None, width=1):
    draw.rounded_rectangle(xy, radius=radius, fill=fill, outline=outline, width=width)

def generate_all_sprites(target_dir: Path):
    target_dir.mkdir(parents=True, exist_ok=True)
    
    # -------------------------------------------------------------
    # 1. CROSSHAIRS & RETICLES
    # -------------------------------------------------------------
    # 1.1 Standard Crosshair (128x128)
    im = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    cx, cy = 64, 64
    # Center dot
    d.ellipse((cx - 2, cy - 2, cx + 2, cy + 2), fill=CYAN)
    # 4 ticks
    tick_len = 10
    gap = 8
    # Top, Bottom, Left, Right
    d.line((cx, cy - gap - tick_len, cx, cy - gap), fill=CYAN, width=2)
    d.line((cx, cy + gap, cx, cy + gap + tick_len), fill=CYAN, width=2)
    d.line((cx - gap - tick_len, cy, cx - gap, cy), fill=CYAN, width=2)
    d.line((cx + gap, cy, cx + gap + tick_len, cy), fill=CYAN, width=2)
    (target_dir / "Crosshairs").mkdir(parents=True, exist_ok=True)
    im.save(target_dir / "Crosshairs" / "reticle_crosshair_standard.png")

    # 1.2 ADS Precision Sight (256x256)
    im = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    cx, cy = 128, 128
    # Outer ring
    d.ellipse((cx - 60, cy - 60, cx + 60, cy + 60), outline=CYAN_GLOW, width=3)
    # Inner ring
    d.ellipse((cx - 28, cy - 28, cx + 28, cy + 28), outline=CYAN, width=2)
    # Central dot
    d.ellipse((cx - 2, cy - 2, cx + 2, cy + 2), fill=CYAN)
    # Range notches
    for offset in [-45, 45]:
        d.line((cx + offset, cy - 4, cx + offset, cy + 4), fill=CYAN, width=2)
        d.line((cx - 4, cy + offset, cx + 4, cy + offset), fill=CYAN, width=2)
    # Triangular chevrons
    d.polygon([(cx, cy - 18), (cx - 5, cy - 12), (cx + 5, cy - 12)], fill=AMBER)
    im.save(target_dir / "Crosshairs" / "reticle_ads_precision.png")

    # 1.3 Hitmarker (128x128)
    im = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    cx, cy = 64, 64
    d_gap = 6
    d_len = 14
    # 4 diagonal lines (X shape)
    d.line((cx - d_gap - d_len, cy - d_gap - d_len, cx - d_gap, cy - d_gap), fill=RED, width=3)
    d.line((cx + d_gap, cy - d_gap, cx + d_gap + d_len, cy - d_gap - d_len), fill=RED, width=3)
    d.line((cx - d_gap - d_len, cy + d_gap + d_len, cx - d_gap, cy + d_gap), fill=RED, width=3)
    d.line((cx + d_gap, cy + d_gap, cx + d_gap + d_len, cy + d_gap + d_len), fill=RED, width=3)
    im.save(target_dir / "Crosshairs" / "reticle_hitmarker.png")

    # -------------------------------------------------------------
    # 2. HUD ELEMENTS
    # -------------------------------------------------------------
    (target_dir / "HUD").mkdir(parents=True, exist_ok=True)
    # 2.1 Health Frame (320x48)
    im = Image.new("RGBA", (320, 48), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_rounded_rect(d, (4, 4, 315, 43), 6, fill=DARK_NAVY, outline=BORDER_LINE, width=2)
    d.polygon([(4, 4), (16, 4), (4, 16)], fill=CYAN)
    d.text((18, 14), "OPERATOR INTEGRITY", fill=WHITE, font=get_font(12, bold=True))
    im.save(target_dir / "HUD" / "hud_health_frame.png")

    # 2.2 Health Bar Fill (280x20)
    im = Image.new("RGBA", (280, 20), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Segmented health pips
    pips = 10
    pip_w = 25
    gap = 3
    for p in range(pips):
        x0 = p * (pip_w + gap) + 2
        draw_rounded_rect(d, (x0, 2, x0 + pip_w, 18), 3, fill=CYAN, outline=None)
    im.save(target_dir / "HUD" / "hud_health_bar_fill.png")

    # 2.3 Loop Timer Circle BG (128x128)
    im = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    cx, cy = 64, 64
    d.ellipse((8, 8, 120, 120), fill=DARK_NAVY, outline=BORDER_LINE, width=3)
    d.ellipse((22, 22, 106, 106), outline=(40, 60, 85, 255), width=2)
    d.text((cx - 16, cy - 10), "20s", fill=WHITE, font=get_font(16, bold=True))
    im.save(target_dir / "HUD" / "hud_timer_circle_bg.png")

    # 2.4 Loop Timer Circle Fill (128x128)
    im = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.arc((12, 12, 116, 116), start=-90, end=180, fill=AMBER, width=6)
    im.save(target_dir / "HUD" / "hud_timer_circle_fill.png")

    # 2.5 Echo Slot Empty (80x80)
    im = Image.new("RGBA", (80, 80), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_rounded_rect(d, (4, 4, 75, 75), 8, fill=DARK_NAVY, outline=BORDER_LINE, width=2)
    d.text((26, 28), "[+]", fill=(120, 140, 160, 255), font=get_font(20, bold=True))
    im.save(target_dir / "HUD" / "hud_echo_slot_empty.png")

    # 2.6 Echo Slot Cyan - Echo 1 (80x80)
    im = Image.new("RGBA", (80, 80), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_rounded_rect(d, (4, 4, 75, 75), 8, fill=DARK_NAVY, outline=CYAN, width=3)
    d.text((14, 12), "ECHO 1", fill=CYAN, font=get_font(12, bold=True))
    # Soundwave graphic
    for idx, h in enumerate([10, 18, 28, 22, 14]):
        x = 22 + idx * 8
        d.line((x, 52 - h//2, x, 52 + h//2), fill=CYAN, width=3)
    im.save(target_dir / "HUD" / "hud_echo_slot_cyan.png")

    # 2.7 Echo Slot Violet - Echo 2 (80x80)
    im = Image.new("RGBA", (80, 80), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_rounded_rect(d, (4, 4, 75, 75), 8, fill=DARK_NAVY, outline=VIOLET, width=3)
    d.text((14, 12), "ECHO 2", fill=VIOLET, font=get_font(12, bold=True))
    for idx, h in enumerate([12, 24, 18, 30, 16]):
        x = 22 + idx * 8
        d.line((x, 52 - h//2, x, 52 + h//2), fill=VIOLET, width=3)
    im.save(target_dir / "HUD" / "hud_echo_slot_violet.png")

    # -------------------------------------------------------------
    # 3. KEY BADGES
    # -------------------------------------------------------------
    (target_dir / "KeyBadges").mkdir(parents=True, exist_ok=True)
    keys = [
        ("E", "E"), ("R", "R"), ("T", "T"), ("Q", "Q"), ("X", "X"),
        ("Tab", "TAB"), ("Shift", "SHIFT"), ("Space", "SPACE"),
        ("LMB", "LMB"), ("RMB", "RMB")
    ]
    for filename, label in keys:
        w = 90 if len(label) > 3 else 64
        h = 64
        im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        d = ImageDraw.Draw(im)
        draw_rounded_rect(d, (3, 3, w - 4, h - 4), 8, fill=(24, 36, 52, 255), outline=CYAN, width=2)
        # Bevel highlight
        d.line((8, 6, w - 9, 6), fill=(100, 180, 220, 200), width=1)
        font = get_font(18 if len(label) > 3 else 24, bold=True)
        # Center text
        bbox = font.getbbox(label)
        tw = bbox[2] - bbox[0]
        th = bbox[3] - bbox[1]
        d.text(((w - tw) // 2, (h - th) // 2 - 2), label, fill=WHITE, font=font)
        im.save(target_dir / "KeyBadges" / f"key_badge_{filename}.png")

    # -------------------------------------------------------------
    # 4. DEVICE ICONS (96x96)
    # -------------------------------------------------------------
    (target_dir / "DeviceIcons").mkdir(parents=True, exist_ok=True)
    # 4.1 Console
    im = Image.new("RGBA", (96, 96), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_rounded_rect(d, (8, 8, 87, 87), 12, fill=DARK_NAVY, outline=CYAN, width=2)
    draw_rounded_rect(d, (20, 20, 75, 60), 4, fill=(15, 28, 42, 255), outline=CYAN, width=1)
    d.line((26, 32, 60, 32), fill=CYAN, width=2)
    d.line((26, 42, 50, 42), fill=CYAN, width=2)
    d.line((36, 68, 60, 68), fill=CYAN, width=3)
    im.save(target_dir / "DeviceIcons" / "icon_device_console.png")

    # 4.2 Lens Sensor
    im = Image.new("RGBA", (96, 96), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_rounded_rect(d, (8, 8, 87, 87), 12, fill=DARK_NAVY, outline=AMBER, width=2)
    cx, cy = 48, 48
    d.ellipse((cx - 24, cy - 24, cx + 24, cy + 24), outline=AMBER, width=2)
    d.ellipse((cx - 12, cy - 12, cx + 12, cy + 12), fill=AMBER)
    # Aperture blades
    for a in [0, 90, 180, 270]:
        rad = math.radians(a)
        x1 = cx + math.cos(rad) * 16
        y1 = cy + math.sin(rad) * 16
        x2 = cx + math.cos(rad) * 28
        y2 = cy + math.sin(rad) * 28
        d.line((x1, y1, x2, y2), fill=AMBER, width=2)
    im.save(target_dir / "DeviceIcons" / "icon_device_lens_sensor.png")

    # 4.3 Upper Relay (Amber)
    im = Image.new("RGBA", (96, 96), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_rounded_rect(d, (8, 8, 87, 87), 12, fill=DARK_NAVY, outline=AMBER, width=2)
    # Lightning bolt
    bolt = [(52, 18), (34, 46), (46, 46), (40, 78), (62, 42), (50, 42)]
    d.polygon(bolt, fill=AMBER)
    im.save(target_dir / "DeviceIcons" / "icon_device_relay_upper.png")

    # 4.4 Lower Relay (Blue)
    im = Image.new("RGBA", (96, 96), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    BLUE_RELAY = (60, 140, 240, 255)
    draw_rounded_rect(d, (8, 8, 87, 87), 12, fill=DARK_NAVY, outline=BLUE_RELAY, width=2)
    bolt = [(52, 18), (34, 46), (46, 46), (40, 78), (62, 42), (50, 42)]
    d.polygon(bolt, fill=BLUE_RELAY)
    im.save(target_dir / "DeviceIcons" / "icon_device_relay_lower.png")

    # 4.5 Resonator
    im = Image.new("RGBA", (96, 96), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_rounded_rect(d, (8, 8, 87, 87), 12, fill=DARK_NAVY, outline=CYAN, width=2)
    # Tuning fork / wave coil
    d.arc((24, 24, 72, 72), start=140, end=400, fill=CYAN, width=3)
    d.arc((32, 32, 64, 64), start=140, end=400, fill=WHITE, width=2)
    d.ellipse((44, 44, 52, 52), fill=CYAN)
    im.save(target_dir / "DeviceIcons" / "icon_device_resonator.png")

    # 4.6 Evacuation Door
    im = Image.new("RGBA", (96, 96), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_rounded_rect(d, (8, 8, 87, 87), 12, fill=DARK_NAVY, outline=CYAN, width=2)
    # Door frame + running stick figure
    d.rectangle((26, 20, 68, 76), outline=CYAN, width=2)
    # Running figure
    d.ellipse((44, 26, 52, 34), fill=WHITE)
    d.line((48, 34, 48, 52), fill=WHITE, width=3)
    d.line((48, 40, 36, 48), fill=WHITE, width=2)
    d.line((48, 40, 58, 46), fill=WHITE, width=2)
    d.line((48, 52, 40, 68), fill=WHITE, width=2)
    d.line((48, 52, 58, 64), fill=WHITE, width=2)
    im.save(target_dir / "DeviceIcons" / "icon_device_evac_door.png")

    # 4.7 Memory Archive (Violet)
    im = Image.new("RGBA", (96, 96), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_rounded_rect(d, (8, 8, 87, 87), 12, fill=DARK_NAVY, outline=VIOLET, width=2)
    # Diamond data crystal
    crystal = [(48, 20), (70, 48), (48, 76), (26, 48)]
    d.polygon(crystal, fill=(169, 147, 242, 100), outline=VIOLET)
    d.line((48, 20, 48, 76), fill=VIOLET, width=1)
    d.line((26, 48, 70, 48), fill=VIOLET, width=1)
    im.save(target_dir / "DeviceIcons" / "icon_device_memory_archive.png")

    # -------------------------------------------------------------
    # 5. PLANNING MAP (TAB) PINS (64x64)
    # -------------------------------------------------------------
    (target_dir / "PlanningMap").mkdir(parents=True, exist_ok=True)
    # 5.1 Player Pin (Cyan chevron)
    im = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.polygon([(32, 10), (52, 50), (32, 40), (12, 50)], fill=WHITE, outline=CYAN)
    im.save(target_dir / "PlanningMap" / "map_player_pin.png")

    # 5.2 Echo 1 Pin (Cyan dot with ring)
    im = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.ellipse((14, 14, 50, 50), outline=CYAN, width=3)
    d.ellipse((22, 22, 42, 42), fill=CYAN)
    d.text((28, 22), "1", fill=DARK_NAVY, font=get_font(16, bold=True))
    im.save(target_dir / "PlanningMap" / "map_echo_cyan_pin.png")

    # 5.3 Echo 2 Pin (Violet dot with ring)
    im = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.ellipse((14, 14, 50, 50), outline=VIOLET, width=3)
    d.ellipse((22, 22, 42, 42), fill=VIOLET)
    d.text((28, 22), "2", fill=DARK_NAVY, font=get_font(16, bold=True))
    im.save(target_dir / "PlanningMap" / "map_echo_violet_pin.png")

    # 5.4 Event: Shot (Crosshair pin)
    im = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.ellipse((12, 12, 52, 52), fill=DARK_NAVY, outline=RED, width=2)
    d.line((32, 16, 32, 48), fill=RED, width=2)
    d.line((16, 32, 48, 32), fill=RED, width=2)
    im.save(target_dir / "PlanningMap" / "map_event_shot.png")

    # 5.5 Event: Console
    im = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.ellipse((12, 12, 52, 52), fill=DARK_NAVY, outline=AMBER, width=2)
    d.text((24, 20), "E", fill=AMBER, font=get_font(20, bold=True))
    im.save(target_dir / "PlanningMap" / "map_event_console.png")

    # 5.6 Event: Archive
    im = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.ellipse((12, 12, 52, 52), fill=DARK_NAVY, outline=VIOLET, width=2)
    d.polygon([(32, 18), (44, 32), (32, 46), (20, 32)], fill=VIOLET)
    im.save(target_dir / "PlanningMap" / "map_event_archive.png")

    # -------------------------------------------------------------
    # 6. BRANDING & MENUS
    # -------------------------------------------------------------
    (target_dir / "Branding_and_Menu").mkdir(parents=True, exist_ok=True)
    # 6.1 Logo (800x200)
    im = Image.new("RGBA", (800, 200), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Subtle background plate
    draw_rounded_rect(d, (10, 10, 790, 190), 16, fill=DARK_NAVY, outline=BORDER_LINE, width=2)
    # Chromatic aberration layers
    f_logo = get_font(52, bold=True)
    f_sub = get_font(18, bold=True)
    # Red ghost
    d.text((68, 48), "ECHO PROTOCOL", fill=(235, 75, 75, 130), font=f_logo)
    # Cyan ghost
    d.text((74, 52), "ECHO PROTOCOL", fill=(39, 196, 216, 160), font=f_logo)
    # Main White text
    d.text((70, 50), "ECHO PROTOCOL", fill=WHITE, font=f_logo)
    # Subtitle
    d.text((72, 122), "TEMPORAL TACTICAL DISCIPLINE // FACILITY RECON", fill=CYAN, font=f_sub)
    # Right-side chevron graphic
    for i in range(4):
        x = 680 + i * 20
        d.polygon([(x, 55), (x + 12, 95), (x, 135), (x - 6, 135), (x + 6, 95), (x - 6, 55)], fill=CYAN if i==3 else BORDER_LINE)
    im.save(target_dir / "Branding_and_Menu" / "logo_echo_protocol_vector.png")

    # 6.2 Menu Button Normal (360x70)
    im = Image.new("RGBA", (360, 70), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_rounded_rect(d, (4, 4, 355, 65), 8, fill=PANEL_BG, outline=BORDER_LINE, width=2)
    d.polygon([(4, 4), (16, 4), (4, 16)], fill=BORDER_LINE)
    im.save(target_dir / "Branding_and_Menu" / "button_primary_normal.png")

    # 6.3 Menu Button Hover (360x70)
    im = Image.new("RGBA", (360, 70), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_rounded_rect(d, (4, 4, 355, 65), 8, fill=(28, 52, 78, 240), outline=CYAN, width=3)
    d.polygon([(4, 4), (18, 4), (4, 18)], fill=CYAN)
    im.save(target_dir / "Branding_and_Menu" / "button_primary_hover.png")

    # 6.4 Menu Button Active (360x70)
    im = Image.new("RGBA", (360, 70), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_rounded_rect(d, (4, 4, 355, 65), 8, fill=(35, 75, 110, 255), outline=WHITE, width=3)
    d.polygon([(4, 4), (18, 4), (4, 18)], fill=WHITE)
    im.save(target_dir / "Branding_and_Menu" / "button_primary_active.png")

    # 6.5 Dialog Panel Frame (600x400)
    im = Image.new("RGBA", (600, 400), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_rounded_rect(d, (6, 6, 593, 393), 12, fill=DARK_NAVY, outline=BORDER_LINE, width=2)
    # Header bar
    d.rectangle((8, 8, 591, 52), fill=(18, 34, 53, 255))
    d.line((8, 52, 591, 52), fill=CYAN, width=2)
    # Corner brackets
    for x, y, dx, dy in [(12, 12, 16, 0), (12, 12, 0, 16), (587, 12, -16, 0), (587, 12, 0, 16),
                         (12, 387, 16, 0), (12, 387, 0, -16), (587, 387, -16, 0), (587, 387, 0, -16)]:
        d.line((x, y, x + dx, y + dy), fill=CYAN, width=2)
    im.save(target_dir / "Branding_and_Menu" / "panel_dialog_frame.png")

if __name__ == '__main__':
    out_dir = Path(r"C:\Users\batin\OneDrive\Masaüstü\unityproje\EchoProtocol\output\Phase_2_Design\Assets\Sprites_UI")
    generate_all_sprites(out_dir)
    print("All Phase 2 sprite & UI assets generated successfully!")
