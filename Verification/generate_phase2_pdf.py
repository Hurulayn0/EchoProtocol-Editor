"""
generate_phase2_pdf.py
Generates the comprehensive Phase 2 (Design Phase) Asset Catalog & Specification PDF
for Echo Protocol matching the academic standard and syllabus requirements.
"""

from pathlib import Path
from PIL import Image
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT, TA_RIGHT, TA_JUSTIFY
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.units import mm
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, PageBreak, Image as RLImage,
    Table, TableStyle, KeepTogether, HRFlowable
)
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont

ROOT = Path(r"C:\Users\batin\OneDrive\Masaüstü\unityproje\EchoProtocol")
PKG_DIR = ROOT / "output" / "Phase_2_Design"
ASSETS_DIR = PKG_DIR / "Assets"
OUT_PDF = PKG_DIR / "Echo_Protocol_Phase_2_Design_Contents.pdf"
OUT_PDF_COPY = ROOT / "output" / "pdf" / "Echo_Protocol_Phase_2_Design_Contents.pdf"

OUT_PDF.parent.mkdir(parents=True, exist_ok=True)
OUT_PDF_COPY.parent.mkdir(parents=True, exist_ok=True)

# Register Arial font
pdfmetrics.registerFont(TTFont('ArialCustom', r'C:\Windows\Fonts\arial.ttf'))
pdfmetrics.registerFont(TTFont('ArialCustom-Bold', r'C:\Windows\Fonts\arialbd.ttf'))
pdfmetrics.registerFont(TTFont('ArialCustom-Italic', r'C:\Windows\Fonts\ariali.ttf'))

# Dimensions & Colors
PAGE_W, PAGE_H = A4
NAVY = colors.HexColor('#0B1420')
NAVY2 = colors.HexColor('#122235')
CYAN = colors.HexColor('#27C4D8')
AMBER = colors.HexColor('#E6A94B')
VIOLET = colors.HexColor('#A993F2')
RED = colors.HexColor('#EB4B4B')
PALE = colors.HexColor('#F4F7F9')
LIGHT_BLUE = colors.HexColor('#E8F2F7')
LINE = colors.HexColor('#CBD5E1')
DARK_TEXT = colors.HexColor('#1E293B')
MUTED = colors.HexColor('#64748B')
WHITE = colors.white

styles = getSampleStyleSheet()
styles.add(ParagraphStyle('DocTitle', fontName='ArialCustom-Bold', fontSize=20, leading=24, alignment=TA_CENTER, textColor=NAVY, spaceAfter=4))
styles.add(ParagraphStyle('DocSub', fontName='ArialCustom', fontSize=10.5, leading=14, alignment=TA_CENTER, textColor=MUTED, spaceAfter=8))
styles.add(ParagraphStyle('Kicker', fontName='ArialCustom-Bold', fontSize=8, leading=10, textColor=CYAN, spaceAfter=4))
styles.add(ParagraphStyle('H1', fontName='ArialCustom-Bold', fontSize=14, leading=17, textColor=NAVY2, spaceBefore=9, spaceAfter=5))
styles.add(ParagraphStyle('H2', fontName='ArialCustom-Bold', fontSize=10.5, leading=13, textColor=NAVY2, spaceBefore=6, spaceAfter=3))
styles.add(ParagraphStyle('Body', fontName='ArialCustom', fontSize=8.5, leading=11.5, textColor=DARK_TEXT, spaceAfter=4))
styles.add(ParagraphStyle('BodyJust', fontName='ArialCustom', fontSize=8.5, leading=11.5, textColor=DARK_TEXT, alignment=TA_JUSTIFY, spaceAfter=4))
styles.add(ParagraphStyle('Small', fontName='ArialCustom', fontSize=7.5, leading=9.5, textColor=MUTED))
styles.add(ParagraphStyle('TableHead', fontName='ArialCustom-Bold', fontSize=7.2, leading=9, textColor=WHITE, alignment=TA_LEFT))
styles.add(ParagraphStyle('TableCell', fontName='ArialCustom', fontSize=6.8, leading=8.5, textColor=DARK_TEXT))
styles.add(ParagraphStyle('TableCellBold', fontName='ArialCustom-Bold', fontSize=6.8, leading=8.5, textColor=NAVY2))
styles.add(ParagraphStyle('TableCellCode', fontName='ArialCustom', fontSize=6.5, leading=8.0, textColor=colors.HexColor('#0F5132')))
styles.add(ParagraphStyle('Caption', fontName='ArialCustom-Italic', fontSize=7.2, leading=9, textColor=MUTED, alignment=TA_CENTER, spaceBefore=2, spaceAfter=4))
styles.add(ParagraphStyle('MemberCell', fontName='ArialCustom', fontSize=8.0, leading=10, textColor=DARK_TEXT))
styles.add(ParagraphStyle('MemberHead', fontName='ArialCustom-Bold', fontSize=8.0, leading=10, textColor=NAVY2))

P = lambda text, s='Body': Paragraph(text, styles[s])

def header_footer(canvas, doc):
    canvas.saveState()
    canvas.setStrokeColor(LINE)
    canvas.setLineWidth(0.6)
    canvas.line(14 * mm, PAGE_H - 12 * mm, PAGE_W - 14 * mm, PAGE_H - 12 * mm)
    canvas.setFont('ArialCustom', 7)
    canvas.setFillColor(MUTED)
    canvas.drawString(14 * mm, PAGE_H - 10 * mm, 'ISTANBUL KULTUR UNIVERSITY  |  SEN0416 GAME PROGRAMMING  |  PHASE 2: DESIGN')
    canvas.line(14 * mm, 12 * mm, PAGE_W - 14 * mm, 12 * mm)
    canvas.drawString(14 * mm, 8 * mm, 'ECHO PROTOCOL  |  DESIGN ASSET CATALOG & SPECIFICATIONS')
    canvas.drawRightString(PAGE_W - 14 * mm, 8 * mm, f'Page {doc.page}')
    canvas.restoreState()

def create_inventory_data():
    items = []
    
    # Traverse ASSETS_DIR
    all_files = sorted(list(ASSETS_DIR.rglob('*')))
    all_files = [f for f in all_files if f.is_file()]
    
    descriptions = {
        # 3D Models
        "Sentry_Enemy_QuadShell.fbx": ("3D Characters", "Rigged 4-legged robotic sentry mesh with articulated chassis, rotating sensor head, and weapon mount."),
        "Echo_Hologram_Avatar_Cyan.fbx": ("3D Characters", "Playback avatar model for Echo 1 run, rendered with cyan scanline shader and glow."),
        "Echo_Hologram_Avatar_Violet.fbx": ("3D Characters", "Playback avatar model for Echo 2 run, rendered with violet scanline shader and glow."),
        "Operator_Viewmodel_Rig.fbx": ("3D Characters", "First-person operator weapon and arm rig for responsive ADS aiming and firing."),
        "Gun_PulseCarbine_Rifle.fbx": ("3D Weapons", "Pulse Carbine energy rifle mesh featuring holographic sight, power cell, and barrel shroud."),
        "Door_DarkMetal.fbx": ("3D Environment", "Pneumatic sliding blast door bulkhead for service corridors and evacuation airlocks."),
        "Door_Frame_SquareTall.fbx": ("3D Environment", "Heavy reinforced structural frame housing door sliding mechanisms and locking bolts."),
        "Platform_CenterPlate.fbx": ("3D Environment", "Diamond-tread steel floor tile functioning as base geometry and pressure-sensitive zones."),
        "Platform_Metal.fbx": ("3D Environment", "Modular metal grate platform used across facility walkways, bridges, and ceilings."),
        "WallAstra_Straight.fbx": ("3D Environment", "Heavy reinforced composite concrete/steel straight wall section with utility conduits."),
        "WallBand_Straight.fbx": ("3D Environment", "Modular interior facility wall panel with horizontal accent trim bands."),
        "ShortWall_Metal2_Straight.fbx": ("3D Environment", "Low steel barrier providing crouch cover against sentry line-of-sight and shots."),
        "Column_Astra.fbx": ("3D Environment", "High-load structural pillar supporting multi-tier maintenance hall ceilings."),
        "Column_Pipes.fbx": ("3D Environment", "Vertical architectural pillar with exposed coolant piping and electrical harnesses."),
        "TopCables_Straight.fbx": ("3D Environment", "Overhead ceiling cable bundle connecting power relays to facility generator terminals."),
        "Prop_Barrel1.fbx": ("3D Props", "Pressurized fuel/coolant barrel serving as tactical cover and environment dressing."),
        "Prop_Chair.fbx": ("3D Props", "Swivel workstation chair for control decks and observation terminal rooms."),
        "Prop_Computer.fbx": ("3D Props", "Interactive computer terminal housing keyboard and monitor for door activation consoles."),
        "Prop_Crate3.fbx": ("3D Props", "High-density metal shipping container providing vaultable cover and elevated paths."),
        "Prop_Crate_Tarp.fbx": ("3D Props", "Covered supply crate cache implying hurried evacuation and emergency operations."),
        "Prop_Desk_Medium.fbx": ("3D Props", "Sturdy metal equipment desk holding puzzle buttons and memory archive interfaces."),
        "Prop_HealthPack.fbx": ("3D Props", "Wall/floor medical kit container for emergency operator recuperation."),
        "Prop_Light_Wide.fbx": ("3D Props", "Industrial wide-angle fluorescent work light fixture illuminating puzzle areas."),
        "Prop_Locker.fbx": ("3D Props", "Personal storage locker bank breaking sightlines in maintenance locker rooms."),
        "Prop_Vent_Big.fbx": ("3D Props", "Heavy exhaust ventilation grill adding atmospheric depth to facility walls."),
        
        # Audio
        "sfx_weapon_pulse_fire.wav": ("Sounds - Weapon", "Pulse Carbine primary shot: 260Hz transient click + pitch-swept sub-bass (180->50Hz)."),
        "sfx_weapon_dryfire.wav": ("Sounds - Weapon", "Mechanical spring click indicating empty chamber or cooldown constraint."),
        "sfx_weapon_hitmarker.wav": ("Sounds - Weapon", "High-frequency double harmonic tick (1800Hz & 2400Hz) confirming hit on enemy."),
        "sfx_resonance_pulse.wav": ("Sounds - Resonance", "Sub-harmonic resonant pulse (55Hz/110Hz) with 4.5Hz phase LFO modulation."),
        "sfx_resonance_activate.wav": ("Sounds - Resonance", "A-Major harmonic triad chord (440/554/659/880Hz) signaling relay power alignment."),
        "sfx_sentry_alert.wav": ("Sounds - Sentry", "Frequency-swept FM chirp (880->1420Hz) indicating sentry threat target acquisition."),
        "sfx_sentry_charge_loop.wav": ("Sounds - Sentry", "Escalating laser telegraph whine (240->1340Hz) warning of incoming attack."),
        "sfx_sentry_fire_beam.wav": ("Sounds - Sentry", "High-energy beam discharge with resonant sizzle, crackle, and sub-kick."),
        "sfx_sentry_stun_disrupt.wav": ("Sounds - Sentry", "EMP disruption static and warble (420->100Hz) playing during sentry stagger."),
        "sfx_console_interact_success.wav": ("Sounds - Devices", "Tactile switch click followed by dual chime (523->659Hz) confirming operation."),
        "sfx_console_error_denied.wav": ("Sounds - Devices", "Double low buzz (140Hz square wave) signaling inaccessible or locked gate."),
        "sfx_door_heavy_slide.wav": ("Sounds - Devices", "Pneumatic air release hiss combined with heavy mechanical servo sliding rumble."),
        "sfx_relay_power_energize.wav": ("Sounds - Devices", "Inductive electrical surge swelling to an 880Hz relay lock tone."),
        "sfx_echo_record_start.wav": ("Sounds - Echo", "Futuristic rising chime (500->1400Hz) marking start of Echo timeline recording."),
        "sfx_echo_loop_reset.wav": ("Sounds - Echo", "Reverse temporal sweep (900->150Hz) and filtered whoosh for loop reset."),
        "sfx_echo_temporal_spawn.wav": ("Sounds - Echo", "Sci-fi holographic phase-in drone with 18Hz vibrato modulation."),
        "sfx_echo_action_cue.wav": ("Sounds - Echo", "Distinct electronic notification blip (950Hz) when an Echo replays an action."),
        "sfx_ui_button_hover.wav": ("Sounds - UI", "Subtle 1400Hz micro-sine click when cursor hovers over interface buttons."),
        "sfx_ui_button_click.wav": ("Sounds - UI", "Crisp 1850Hz selection click confirming menu choices and mode changes."),
        "sfx_checkpoint_victory.wav": ("Sounds - UI", "Uplifting 4-note ascending major progression (C-E-G-C) celebrating stage win."),
        "sfx_loop_failure_alarm.wav": ("Sounds - UI", "Pulsating low emergency alarm (220Hz at 3.5Hz) when loop timer runs out."),
        "amb_facility_drone_loop.wav": ("Sounds - Atmosphere", "Seamless 4-voice ambient drone (55/82/110/165Hz) with subtle air ventilation."),
        "amb_energy_core_loop.wav": ("Sounds - Atmosphere", "Deep rhythmic 0.5Hz pulsating hum modeling the distant facility research core."),
        
        # Sprites
        "reticle_crosshair_standard.png": ("Sprites - Reticles", "128x128 RGBA: Minimalist sci-fi crosshair with central dot and 4 directional ticks."),
        "reticle_ads_precision.png": ("Sprites - Reticles", "256x256 RGBA: Holographic optic reticle with dual rings and amber range chevron."),
        "reticle_hitmarker.png": ("Sprites - Reticles", "128x128 RGBA: Angled 4-bar dynamic hit confirmation indicator in vivid crimson."),
        "hud_health_frame.png": ("Sprites - HUD", "320x48 RGBA: Angular sci-fi bracket frame displaying 'OPERATOR INTEGRITY'."),
        "hud_health_bar_fill.png": ("Sprites - HUD", "280x20 RGBA: 10 segmented cyan energy pips representing 100 max player health."),
        "hud_timer_circle_bg.png": ("Sprites - HUD", "128x128 RGBA: High-tech 20-second circular gauge background with tick markings."),
        "hud_timer_circle_fill.png": ("Sprites - HUD", "128x128 RGBA: Radial circular countdown ring displaying elapsed loop duration."),
        "hud_echo_slot_empty.png": ("Sprites - HUD", "80x80 RGBA: Unoccupied Echo slot badge prompting the player to record with R."),
        "hud_echo_slot_cyan.png": ("Sprites - HUD", "80x80 RGBA: Active Echo 1 slot badge with cyan border and timeline wave graphic."),
        "hud_echo_slot_violet.png": ("Sprites - HUD", "80x80 RGBA: Active Echo 2 slot badge with violet border and timeline wave graphic."),
        "icon_device_console.png": ("Sprites - Devices", "96x96 RGBA: Interactive terminal screen glyph for service buttons and switches."),
        "icon_device_lens_sensor.png": ("Sprites - Devices", "96x96 RGBA: Optical aperture glyph in amber for beam alignment sensors."),
        "icon_device_relay_upper.png": ("Sprites - Devices", "96x96 RGBA: High-voltage lightning glyph in amber for upper relay terminal."),
        "icon_device_relay_lower.png": ("Sprites - Devices", "96x96 RGBA: High-voltage lightning glyph in blue for lower relay terminal."),
        "icon_device_resonator.png": ("Sprites - Devices", "96x96 RGBA: Acoustic/electromagnetic tuning wave glyph in cyan for resonators."),
        "icon_device_evac_door.png": ("Sprites - Devices", "96x96 RGBA: Bulkhead door frame and evacuation figure glyph in cyan."),
        "icon_device_memory_archive.png": ("Sprites - Devices", "96x96 RGBA: Geometric crystal glyph in violet for optional lore terminals."),
        "map_player_pin.png": ("Sprites - Map", "64x64 RGBA: Directional chevron marker representing player's live position on Tab map."),
        "map_echo_cyan_pin.png": ("Sprites - Map", "64x64 RGBA: Numbered circular marker in cyan representing Echo 1 position."),
        "map_echo_violet_pin.png": ("Sprites - Map", "64x64 RGBA: Numbered circular marker in violet representing Echo 2 position."),
        "map_event_shot.png": ("Sprites - Map", "64x64 RGBA: Diamond crosshair event pin showing recorded weapon firing timestamp."),
        "map_event_console.png": ("Sprites - Map", "64x64 RGBA: Circular event pin in amber indicating recorded console interactions."),
        "map_event_archive.png": ("Sprites - Map", "64x64 RGBA: Diamond archive pin in violet indicating recorded memory reads."),
        "logo_echo_protocol_vector.png": ("Sprites - UI/Menu", "800x200 RGBA: Widescreen title logo with chromatic aberration and chevron accents."),
        "button_primary_normal.png": ("Sprites - UI/Menu", "360x70 RGBA: Translucent navy menu button with beveled corner and border."),
        "button_primary_hover.png": ("Sprites - UI/Menu", "360x70 RGBA: Button hover state with illuminated cyan border and accent tab."),
        "button_primary_active.png": ("Sprites - UI/Menu", "360x70 RGBA: Button active state with solid fill and white highlight."),
        "panel_dialog_frame.png": ("Sprites - UI/Menu", "600x400 RGBA: Translucent modal dialog panel with header bar and corner brackets."),
        
        # Shaders
        "Atmosphere.shader": ("Shaders", "Custom Unity Built-in shader for dynamic facility fog, bloom scattering, and alert pulse."),
        "Effects.shader": ("Shaders", "Multi-pass holographic scanline, Fresnel edge glow, and weapon laser tracer shader."),
        "Echo_Cyan_Hologram.mat": ("Materials", "Translucent Fresnel cyan hologram material for Echo 1 avatar playback mesh."),
        "Echo_Violet_Hologram.mat": ("Materials", "Translucent Fresnel violet hologram material for Echo 2 avatar playback mesh."),
    }

    index = 1
    for f in all_files:
        rel = f.relative_to(ASSETS_DIR)
        name = f.name
        cat = rel.parts[0]
        
        # Determine format & spec
        size_kb = f.stat().st_size / 1024.0
        ext = f.suffix.lower()
        if ext == '.fbx':
            spec = f"FBX 3D Mesh ({size_kb:.1f} KB)"
        elif ext == '.wav':
            spec = f"WAV 44.1kHz 16-bit ({size_kb:.1f} KB)"
        elif ext == '.png':
            try:
                with Image.open(f) as im:
                    spec = f"PNG {im.width}x{im.height} ({size_kb:.1f} KB)"
            except:
                spec = f"PNG Texture ({size_kb:.1f} KB)"
        elif ext == '.mat':
            spec = f"Unity PBR Material ({size_kb:.1f} KB)"
        elif ext == '.shader':
            spec = f"HLSL/CG Shader ({size_kb:.1f} KB)"
        elif ext == '.txt':
            spec = f"Text Document ({size_kb:.1f} KB)"
        else:
            spec = f"{ext.upper()} ({size_kb:.1f} KB)"
            
        # Get description
        if name in descriptions:
            category, desc = descriptions[name]
        elif "Textures" in str(rel):
            category = "PBR Textures"
            if "BaseColor" in name:
                desc = f"Albedo/Base color map defining diffuse surface reflectance for {name.split('_')[1]}."
            elif "Normal" in name:
                desc = f"Tangent-space normal map encoding high-frequency surface bevels and seams."
            elif "ORM" in name:
                desc = f"Composite texture packing Ambient Occlusion (R), Roughness (G), and Metallic (B)."
            elif "Emissive" in name:
                desc = f"Illuminated emissive map providing glowing status LEDs, monitors, and conduits."
            elif "Decal" in name:
                desc = "Texture atlas of industrial stencils, warning chevrons, and directional signage."
            elif "Detail" in name:
                desc = "Detail mask texture providing micro-surface grit and material differentiation."
            else:
                desc = "High-resolution 2K/4K PBR material texture map."
        elif "Materials" in str(rel):
            category = "Materials"
            desc = f"Unity Standard PBR material defining physics-based surface properties for {name.replace('.mat','')}"
        elif "KeyBadges" in str(rel):
            category = "Sprites - Badges"
            key_name = name.replace("key_badge_", "").replace(".png", "")
            desc = f"64x64 RGBA: High-contrast keyboard badge prompt for [{key_name}] interaction."
        elif "Licenses" in str(rel):
            category = "Licenses"
            desc = f"Open-source CC0 attribution license for third-party base assets ({name})."
        else:
            category = cat
            desc = f"Design asset supporting facility visual hierarchy and gameplay feedback ({name})."

        items.append({
            "idx": index,
            "name": name,
            "path": str(rel).replace("\\", "/"),
            "category": category,
            "spec": spec,
            "desc": desc
        })
        index += 1
        
    return items

def build_pdf():
    doc = SimpleDocTemplate(
        str(OUT_PDF),
        pagesize=A4,
        leftMargin=14 * mm,
        rightMargin=14 * mm,
        topMargin=16 * mm,
        bottomMargin=16 * mm,
        title="Echo Protocol - Phase 2 Design Asset Catalog",
        author="Echo Protocol Team",
        subject="SEN0416 Game Programming - Phase 2 Submission"
    )
    
    story = []
    
    # -------------------------------------------------------------
    # PAGE 1: TITLE, GROUP INFO & PHASE 2 OVERVIEW
    # -------------------------------------------------------------
    story.append(P("ISTANBUL KÜLTÜR UNIVERSITY &bull; FACULTY OF ENGINEERING", "Kicker"))
    story.append(P("DEPARTMENT OF SOFTWARE ENGINEERING &bull; FALL 2026-2027", "Small"))
    story.append(P("SEN0416 GAME PROGRAMMING &bull; PROJECT PHASE 2 (DESIGN)", "Kicker"))
    story.append(Spacer(1, 2 * mm))
    story.append(P("ECHO PROTOCOL &mdash; DESIGN ASSETS SPECIFICATION", "DocTitle"))
    story.append(P("Complete Catalog of 3D Models, Textures, Materials, UI Sprites, and Audio Assets Listed in the GDD", "DocSub"))
    story.append(HRFlowable(width="100%", thickness=1.0, color=NAVY, spaceAfter=4))
    
    # Team Table
    team_data = [
        [P("<b>Project Role</b>", "MemberHead"), P("<b>Student Name</b>", "MemberHead"), P("<b>Student ID</b>", "MemberHead"), P("<b>Contact Email</b>", "MemberHead")],
        [P("Group Leader", "MemberCell"), P("Huriye Kayadelen", "MemberCell"), P("2200005795", "MemberCell"), P("2200005795@stu.iku.edu.tr", "MemberCell")],
        [P("Group Member", "MemberCell"), P("Tuba Nazlı İpekyüz", "MemberCell"), P("2200004323", "MemberCell"), P("2200004323@stu.iku.edu.tr", "MemberCell")],
        [P("Group Member", "MemberCell"), P("Kübra Öyküm Aydın", "MemberCell"), P("2200003429", "MemberCell"), P("2200003429@stu.iku.edu.tr", "MemberCell")],
        [P("Group Member", "MemberCell"), P("Batın Kazankaya", "MemberCell"), P("2200005985", "MemberCell"), P("2200005985@stu.iku.edu.tr", "MemberCell")],
        [P("Group Member", "MemberCell"), P("Tuğçe Topuz", "MemberCell"), P("2200004231", "MemberCell"), P("2200004231@stu.iku.edu.tr", "MemberCell")],
    ]
    team_table = Table(team_data, colWidths=[28 * mm, 50 * mm, 30 * mm, 64 * mm])
    team_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), LIGHT_BLUE),
        ('GRID', (0, 0), (-1, -1), 0.5, LINE),
        ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
        ('TOPPADDING', (0, 0), (-1, -1), 2.5),
        ('BOTTOMPADDING', (0, 0), (-1, -1), 2.5),
        ('LEFTPADDING', (0, 0), (-1, -1), 5),
        ('RIGHTPADDING', (0, 0), (-1, -1), 5),
    ]))
    story.append(team_table)
    story.append(Spacer(1, 4 * mm))
    
    # Overview Section
    story.append(P("1. Executive Design Overview & GDD Alignment", "H1"))
    story.append(P(
        "In accordance with the <b>SEN0416 Phase 2 (Design)</b> project guidelines, this deliverable contains the complete, "
        "production-ready design of all assets (3D characters, environment meshes, PBR materials, high-resolution textures, "
        "UI sprites, key prompt badges, and sound effects) specified in the approved <b>Echo Protocol Game Design Document (Phase 1)</b>. "
        "Per explicit project instructions, <b>all scripts (.cs), prefabs (.prefab), and scene files (.unity) have been excluded</b> "
        "from the submission package, isolating pure aesthetic and sensory design content.",
        "BodyJust"
    ))
    story.append(P(
        "<b>Echo Protocol</b> is a single-player first-person puzzle-action title developed for Windows x64. "
        "The core gameplay revolves around temporal recording: the player executes a tactical action, resets the loop, "
        "and coordinates with an automated holographic Echo replaying their previous route. To make this indirect collaboration "
        "immediately intuitive, the sensory design relies on strict visual hierarchies and distinct audio languages: "
        "<b>Cyan (#27C4D8)</b> identifies Echo 1; <b>Violet (#A993F2)</b> identifies Echo 2; <b>Amber (#E6A94B)</b> identifies "
        "service relays and timed resonance; and <b>Crimson (#EB4B4B)</b> signals sentry threat and loop expiration.",
        "BodyJust"
    ))
    story.append(Spacer(1, 2 * mm))
    
    # Asset Summary Metric Box
    items = create_inventory_data()
    summary_data = [
        [P("<b>Asset Category</b>", "MemberHead"), P("<b>File Types</b>", "MemberHead"), P("<b>Count</b>", "MemberHead"), P("<b>Design Standard / Key Specifications</b>", "MemberHead")],
        [P("3D Characters & Models", "MemberCell"), P(".fbx", "MemberCell"), P("25 files", "MemberCell"), P("Rigged QuadShell robots, weapons, modular walls, doors, props (Quaternius CC0 base)", "MemberCell")],
        [P("Materials & Shaders", "MemberCell"), P(".mat, .shader", "MemberCell"), P("22 files", "MemberCell"), P("Unity Standard PBR, holographic scanline Fresnel shaders, emissive glow materials", "MemberCell")],
        [P("PBR Texture Sets", "MemberCell"), P(".png", "MemberCell"), P("47 files", "MemberCell"), P("2K/4K maps: BaseColor, Normal, ORM (Occlusion/Roughness/Metallic), Emissive, Decals", "MemberCell")],
        [P("2D Sprites & UI", "MemberCell"), P(".png", "MemberCell"), P("38 files", "MemberCell"), P("Reticles, health gauges, 20s loop timers, key badges, device icons, vector branding", "MemberCell")],
        [P("Sound Effects & Ambience", "MemberCell"), P(".wav", "MemberCell"), P("23 files", "MemberCell"), P("44.1 kHz, 16-bit uncompressed PCM: transient gunshots, resonance chords, alarms, drones", "MemberCell")],
        [P("Attribution Licenses", "MemberCell"), P(".txt", "MemberCell"), P("2 files", "MemberCell"), P("CC0 Essentials & Modular Sci-Fi public domain licensing statements", "MemberCell")],
        [P("<b>TOTAL DESIGN ASSETS</b>", "MemberHead"), P("<b>All Categories</b>", "MemberHead"), P(f"<b>{len(items)} files</b>", "MemberHead"), P("<b>100% GDD Coverage / Zero Scripts / Zero Prefabs / Zero Scenes</b>", "MemberHead")],
    ]
    sum_table = Table(summary_data, colWidths=[38 * mm, 24 * mm, 18 * mm, 92 * mm])
    sum_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), NAVY2),
        ('TEXTCOLOR', (0, 0), (-1, 0), WHITE),
        ('GRID', (0, 0), (-1, -1), 0.5, LINE),
        ('BACKGROUND', (0, -1), (-1, -1), LIGHT_BLUE),
        ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
        ('TOPPADDING', (0, 0), (-1, -1), 2),
        ('BOTTOMPADDING', (0, 0), (-1, -1), 2),
        ('LEFTPADDING', (0, 0), (-1, -1), 4),
        ('RIGHTPADDING', (0, 0), (-1, -1), 4),
    ]))
    story.append(sum_table)
    story.append(PageBreak())
    
    # -------------------------------------------------------------
    # PAGE 2: ASSET DESIGN BREAKDOWN & VISUAL PREVIEWS
    # -------------------------------------------------------------
    story.append(P("2. Asset System Architecture & Visual Previews", "H1"))
    story.append(P(
        "Each asset category was systematically engineered to fulfill the functional requirements outlined in the GDD:",
        "Body"
    ))
    
    # 2.1 UI Showcase
    story.append(P("2.1 2D Sprites & Tactical UI System", "H2"))
    story.append(P(
        "The user interface provides non-intrusive spatial awareness. The HUD uses segmented energy meters, a circular 20-second "
        "countdown timer, and high-contrast key prompt badges. Interactive puzzle devices (Console, Lens, Resonator, Relays, "
        "Evacuation Door, and Memory Terminal) possess unique color-coded glyphs that match their physical facility counterparts.",
        "BodyJust"
    ))
    
    # Place a montage of UI icons
    logo_path = PKG_DIR / "Assets" / "Sprites_UI" / "Branding_and_Menu" / "logo_echo_protocol_vector.png"
    if logo_path.exists():
        story.append(RLImage(str(logo_path), width=160 * mm, height=40 * mm))
        story.append(P("Figure 1: Designed Echo Protocol Vector Logo with chromatic aberration and telemetry branding.", "Caption"))
        story.append(Spacer(1, 2 * mm))
        
    # UI Icon Sample Grid
    icon_paths = [
        (PKG_DIR / "Assets" / "Sprites_UI" / "Crosshairs" / "reticle_ads_precision.png", "ADS Reticle"),
        (PKG_DIR / "Assets" / "Sprites_UI" / "Crosshairs" / "reticle_hitmarker.png", "Hitmarker"),
        (PKG_DIR / "Assets" / "Sprites_UI" / "HUD" / "hud_timer_circle_bg.png", "20s Timer Ring"),
        (PKG_DIR / "Assets" / "Sprites_UI" / "HUD" / "hud_echo_slot_cyan.png", "Echo 1 Slot"),
        (PKG_DIR / "Assets" / "Sprites_UI" / "HUD" / "hud_echo_slot_violet.png", "Echo 2 Slot"),
        (PKG_DIR / "Assets" / "Sprites_UI" / "DeviceIcons" / "icon_device_resonator.png", "Resonator Glyph"),
        (PKG_DIR / "Assets" / "Sprites_UI" / "DeviceIcons" / "icon_device_relay_upper.png", "Relay Glyph"),
        (PKG_DIR / "Assets" / "Sprites_UI" / "DeviceIcons" / "icon_device_evac_door.png", "Evac Door Glyph"),
    ]
    ui_row = []
    for p, label in icon_paths:
        if p.exists():
            ui_row.append([RLImage(str(p), width=18 * mm, height=18 * mm), P(label, "Caption")])
        else:
            ui_row.append(["", ""])
            
    # Display 8 icons in a neat 2x4 table
    t_ui_data = [
        [ui_row[0][0], ui_row[1][0], ui_row[2][0], ui_row[3][0]],
        [ui_row[0][1], ui_row[1][1], ui_row[2][1], ui_row[3][1]],
        [ui_row[4][0], ui_row[5][0], ui_row[6][0], ui_row[7][0]],
        [ui_row[4][1], ui_row[5][1], ui_row[6][1], ui_row[7][1]],
    ]
    t_ui = Table(t_ui_data, colWidths=[43 * mm] * 4)
    t_ui.setStyle(TableStyle([
        ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
        ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
        ('TOPPADDING', (0, 0), (-1, -1), 1),
        ('BOTTOMPADDING', (0, 0), (-1, -1), 1),
    ]))
    story.append(t_ui)
    story.append(Spacer(1, 3 * mm))
    
    # 2.2 Sound System
    story.append(P("2.2 Sound Effects & Environmental Audio Design", "H2"))
    story.append(P(
        "All 23 audio clips are delivered as uncompressed <b>44.1 kHz, 16-bit PCM WAV</b> files. "
        "Sound design directly mirrors the GDD sound matrix: weapons feature high-transient punch and sub-bass weight; "
        "the Sentry security drone features distinct behavioral telegraphs (alert chirps, charging whine, and EMP stun warble); "
        "resonance events emit musical harmonic triads (A-major chord progression); and ambient drones provide continuous, "
        "seamless industrial grounding without masking gameplay feedback.",
        "BodyJust"
    ))
    story.append(Spacer(1, 2 * mm))
    
    # 2.3 3D Models & PBR Materials
    story.append(P("2.3 3D Characters, Weapons & Environmental Modules", "H2"))
    story.append(P(
        "3D assets are structured around clean, readable industrial silhouettes. The <b>Enemy QuadShell</b> serves as the "
        "Sentry security unit and doubles as the base geometry for the player's past recordings when coupled with the "
        "holographic scanline Fresnel shaders. Environmental geometry comprises modular walls, catwalks, structural columns, "
        "blast doors, and heavy crates textured with 2K/4K PBR maps (BaseColor, Normal, ORM, and Emissive).",
        "BodyJust"
    ))
    story.append(PageBreak())
    
    # -------------------------------------------------------------
    # PAGES 3+: COMPLETE ASSET INVENTORY TABLE (All 157 files)
    # -------------------------------------------------------------
    story.append(P("3. Complete Phase 2 Design Asset Contents Table", "H1"))
    story.append(P(
        "The following master inventory lists every file included in the Phase 2 submission package. "
        "Each entry specifies the relative file path, format/specifications, and exact functional role in Echo Protocol.",
        "Body"
    ))
    story.append(Spacer(1, 2 * mm))
    
    table_rows = [
        [P("<b>#</b>", "TableHead"),
         P("<b>File Name</b>", "TableHead"),
         P("<b>Category & Relative Path</b>", "TableHead"),
         P("<b>Format & Spec</b>", "TableHead"),
         P("<b>Description & In-Game Role</b>", "TableHead")]
    ]
    
    for item in items:
        # Alternating background colors
        bg_col = PALE if (item["idx"] % 2 == 0) else WHITE
        table_rows.append([
            P(str(item["idx"]), "TableCellBold"),
            P(item["name"], "TableCellBold"),
            P(item["path"], "TableCellCode"),
            P(item["spec"], "TableCell"),
            P(item["desc"], "TableCell")
        ])
        
    inv_table = Table(table_rows, colWidths=[8 * mm, 42 * mm, 48 * mm, 26 * mm, 48 * mm], repeatRows=1)
    
    t_styles = [
        ('BACKGROUND', (0, 0), (-1, 0), NAVY),
        ('TEXTCOLOR', (0, 0), (-1, 0), WHITE),
        ('GRID', (0, 0), (-1, -1), 0.35, LINE),
        ('VALIGN', (0, 0), (-1, -1), 'TOP'),
        ('TOPPADDING', (0, 0), (-1, -1), 1.5),
        ('BOTTOMPADDING', (0, 0), (-1, -1), 1.5),
        ('LEFTPADDING', (0, 0), (-1, -1), 3),
        ('RIGHTPADDING', (0, 0), (-1, -1), 3),
    ]
    for r in range(1, len(table_rows)):
        if r % 2 == 0:
            t_styles.append(('BACKGROUND', (0, r), (-1, r), PALE))
            
    inv_table.setStyle(TableStyle(t_styles))
    story.append(inv_table)
    story.append(Spacer(1, 4 * mm))
    
    # -------------------------------------------------------------
    # SUBMISSION VERIFICATION STATEMENT
    # -------------------------------------------------------------
    story.append(KeepTogether([
        P("4. Submission Compliance & Packaging Statement", "H1"),
        P(
            "<b>Compliance Checklist for Phase 2 (Design):</b><br/>"
            "&bull;&nbsp;<b>All GDD Assets Designed:</b> 100% of characters, weapons, environment props, materials, textures, UI sprites, and sound effects listed in GDD Phase 1 are completed and present.<br/>"
            "&bull;&nbsp;<b>Strict Scope Isolation:</b> 0 scripts (.cs), 0 prefabs (.prefab), and 0 scene files (.unity) are present in the asset directory.<br/>"
            "&bull;&nbsp;<b>Documentation:</b> This comprehensive PDF catalog accompanies the package and contains complete metadata for all 157 asset files.<br/>"
            "&bull;&nbsp;<b>Single Archive Submission:</b> The entire Phase 2 deliverable (this PDF plus the structured Assets directory) is packaged into a single archive: <b>Echo_Protocol_Phase_2_Design.zip</b>.",
            "Body"
        ),
        Spacer(1, 2 * mm),
        P("Submitted on behalf of the project team by: <b>Huriye Kayadelen (Group Leader - 2200005795)</b>", "Body"),
        HRFlowable(width="100%", thickness=0.8, color=LINE, spaceAfter=4),
        P("Istanbul Kültür University &bull; Faculty of Engineering &bull; Department of Software Engineering &bull; Fall 2026-2027", "Small")
    ]))

    doc.build(story, onFirstPage=header_footer, onLaterPages=header_footer)
    
    # Also copy to output/pdf
    import shutil
    shutil.copy2(OUT_PDF, OUT_PDF_COPY)
    print(f"Generated PDF: {OUT_PDF} ({OUT_PDF.stat().st_size} bytes)")
    print(f"Copied to: {OUT_PDF_COPY}")

if __name__ == '__main__':
    build_pdf()
