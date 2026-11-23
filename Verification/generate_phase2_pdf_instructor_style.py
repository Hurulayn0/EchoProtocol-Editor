"""
generate_phase2_pdf_instructor_style.py
Generates the Phase 2 (Design) Game Design Document and Asset Contents Catalog
strictly matching the instructor's template style (SEN0416 Game Programming - Assist. Prof. Dr. Visam Elmasry).
"""

import shutil
from pathlib import Path
from PIL import Image
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT, TA_JUSTIFY
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
FIG_DIR = ROOT / "tmp" / "pdfs" / "gdd_assets"
OUT_PDF = PKG_DIR / "Echo_Protocol_Phase_2_Design_Contents.pdf"
OUT_PDF_COPY = ROOT / "output" / "pdf" / "Echo_Protocol_Phase_2_Design_Contents.pdf"

OUT_PDF.parent.mkdir(parents=True, exist_ok=True)
OUT_PDF_COPY.parent.mkdir(parents=True, exist_ok=True)

# Register Fonts
pdfmetrics.registerFont(TTFont('Times-Roman-Custom', r'C:\Windows\Fonts\times.ttf'))
pdfmetrics.registerFont(TTFont('Times-Bold-Custom', r'C:\Windows\Fonts\timesbd.ttf'))
pdfmetrics.registerFont(TTFont('Times-Italic-Custom', r'C:\Windows\Fonts\timesi.ttf'))
pdfmetrics.registerFont(TTFont('ArialUnicode', r'C:\Windows\Fonts\arial.ttf'))
pdfmetrics.registerFont(TTFont('ArialUnicode-Bold', r'C:\Windows\Fonts\arialbd.ttf'))

PAGE_W, PAGE_H = A4
INK = colors.HexColor('#111111')
GREY = colors.HexColor('#555555')
LIGHT_GREY = colors.HexColor('#F2F2F2')
BORDER_GREY = colors.HexColor('#999999')
TABLE_LINE = colors.HexColor('#CCCCCC')

base = getSampleStyleSheet()
base.add(ParagraphStyle(name='DocTitle', fontName='Times-Bold-Custom', fontSize=18, leading=22, alignment=TA_CENTER, textColor=INK, spaceAfter=8))
base.add(ParagraphStyle(name='DocSubTitle', fontName='Times-Bold-Custom', fontSize=13, leading=16, alignment=TA_CENTER, textColor=INK, spaceAfter=10))
base.add(ParagraphStyle(name='Section', fontName='Times-Bold-Custom', fontSize=13, leading=16, textColor=INK, spaceBefore=10, spaceAfter=4))
base.add(ParagraphStyle(name='Subsection', fontName='Times-Bold-Custom', fontSize=11, leading=14, textColor=INK, spaceBefore=6, spaceAfter=3))
base.add(ParagraphStyle(name='Body', fontName='Times-Roman-Custom', fontSize=10, leading=14, textColor=INK, spaceAfter=5))
base.add(ParagraphStyle(name='BulletL1', fontName='Times-Roman-Custom', fontSize=10, leading=14, leftIndent=12, firstLineIndent=-12, spaceAfter=4, textColor=INK))
base.add(ParagraphStyle(name='BulletL2', fontName='Times-Roman-Custom', fontSize=9.5, leading=13.5, leftIndent=24, firstLineIndent=-12, spaceAfter=3, textColor=INK))
base.add(ParagraphStyle(name='Caption', fontName='Times-Roman-Custom', fontSize=9.5, leading=12, textColor=INK, spaceBefore=3, spaceAfter=7))
base.add(ParagraphStyle(name='MemberHead', fontName='ArialUnicode-Bold', fontSize=8.5, leading=11, textColor=INK))
base.add(ParagraphStyle(name='MemberCell', fontName='ArialUnicode', fontSize=8.5, leading=11, textColor=INK))
base.add(ParagraphStyle(name='TableHead', fontName='Times-Bold-Custom', fontSize=7.5, leading=9.5, textColor=INK))
base.add(ParagraphStyle(name='TableCell', fontName='Times-Roman-Custom', fontSize=7.0, leading=9.0, textColor=INK))
base.add(ParagraphStyle(name='TableCellBold', fontName='Times-Bold-Custom', fontSize=7.0, leading=9.0, textColor=INK))

P = lambda text, style='Body': Paragraph(text, base[style])

def figure_flowable(image_path: Path, caption_text: str, width=145*mm):
    im = Image.open(image_path)
    h = width * im.height / im.width
    # Cap height if it's too tall
    if h > 75*mm:
        h = 75*mm
        width = h * im.width / im.height
    return [
        Spacer(1, 2*mm),
        RLImage(str(image_path), width=width, height=h),
        Spacer(1, 1.5*mm),
        P(caption_text, 'Caption'),
        Spacer(1, 2*mm)
    ]

def page_layout(canvas, doc):
    canvas.saveState()
    canvas.setFont('Times-Roman-Custom', 10)
    canvas.setFillColor(INK)
    # Page number at bottom center
    canvas.drawCentredString(PAGE_W / 2.0, 12 * mm, str(doc.page))
    canvas.restoreState()

def get_inventory_items():
    all_files = sorted(list(ASSETS_DIR.rglob('*')))
    all_files = [f for f in all_files if f.is_file()]
    
    desc_map = {
        # Characters & Weapons
        "Sentry_Enemy_QuadShell.fbx": ("3D Characters", "Rigged 4-legged robotic sentry mesh with rotating sensor turret and laser mount."),
        "Echo_Hologram_Avatar_Cyan.fbx": ("3D Characters", "Playback avatar model for Echo 1 run, rendered with cyan holographic material."),
        "Echo_Hologram_Avatar_Violet.fbx": ("3D Characters", "Playback avatar model for Echo 2 run, rendered with violet holographic material."),
        "Operator_Viewmodel_Rig.fbx": ("3D Characters", "First-person operator weapon and arm rig for responsive aiming and firing."),
        "Gun_PulseCarbine_Rifle.fbx": ("3D Weapons", "Pulse Carbine energy rifle mesh featuring holographic sight and power cell."),
        
        # Environment
        "Door_DarkMetal.fbx": ("3D Environment", "Pneumatic sliding blast door bulkhead for service corridors and evacuation exit."),
        "Door_Frame_SquareTall.fbx": ("3D Environment", "Heavy reinforced structural frame housing door sliding mechanisms and locking bolts."),
        "Platform_CenterPlate.fbx": ("3D Environment", "Diamond-tread steel floor tile functioning as base geometry and pressure zones."),
        "Platform_Metal.fbx": ("3D Environment", "Modular metal grate platform used across facility walkways, bridges, and ceilings."),
        "WallAstra_Straight.fbx": ("3D Environment", "Heavy reinforced composite concrete/steel straight wall section with utility conduits."),
        "WallBand_Straight.fbx": ("3D Environment", "Modular interior facility wall panel with horizontal accent trim bands."),
        "ShortWall_Metal2_Straight.fbx": ("3D Environment", "Low steel barrier providing crouch cover against sentry line-of-sight and shots."),
        "Column_Astra.fbx": ("3D Environment", "High-load structural pillar supporting multi-tier maintenance hall ceilings."),
        "Column_Pipes.fbx": ("3D Environment", "Vertical architectural pillar with exposed coolant piping and electrical harnesses."),
        "TopCables_Straight.fbx": ("3D Environment", "Overhead ceiling cable bundle connecting power relays to facility generator terminals."),
        
        # Props
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

        # Sounds
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

        # Shaders & Mats
        "Atmosphere.shader": ("Shaders", "Custom Unity Built-in shader for dynamic facility fog, bloom scattering, and alert pulse."),
        "Effects.shader": ("Shaders", "Multi-pass holographic scanline, Fresnel edge glow, and weapon laser tracer shader."),
        "Echo_Cyan_Hologram.mat": ("Materials", "Translucent Fresnel cyan hologram material for Echo 1 avatar playback mesh."),
        "Echo_Violet_Hologram.mat": ("Materials", "Translucent Fresnel violet hologram material for Echo 2 avatar playback mesh."),
    }

    items = []
    idx = 1
    for f in all_files:
        rel = f.relative_to(ASSETS_DIR)
        name = f.name
        cat = rel.parts[0]
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
            
        if name in desc_map:
            category, desc = desc_map[name]
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
            desc = f"Unity Standard PBR material defining surface properties for {name.replace('.mat','')}"
        elif "KeyBadges" in str(rel):
            category = "Sprites - Badges"
            key_name = name.replace("key_badge_", "").replace(".png", "")
            desc = f"64x64 RGBA: Keyboard badge prompt for [{key_name}] interaction."
        elif "Licenses" in str(rel):
            category = "Licenses"
            desc = f"Open-source CC0 attribution license for third-party base assets ({name})."
        else:
            category = cat
            desc = f"Design asset supporting facility visual hierarchy and gameplay feedback ({name})."

        items.append({
            "idx": idx,
            "name": name,
            "path": str(rel).replace("\\", "/"),
            "category": category,
            "spec": spec,
            "desc": desc
        })
        idx += 1
    return items

def build_pdf():
    doc = SimpleDocTemplate(
        str(OUT_PDF),
        pagesize=A4,
        leftMargin=20 * mm,
        rightMargin=20 * mm,
        topMargin=18 * mm,
        bottomMargin=20 * mm,
        title='Game Design Document (GDD) - Phase 2 (Design)',
        author='Echo Protocol Project Team'
    )
    
    story = []
    
    # -------------------------------------------------------------
    # PAGE 1: TITLE, PROJECT GROUP, GAME OVERVIEW
    # -------------------------------------------------------------
    story.append(P('Game Design Document (GDD)', 'DocTitle'))
    story.append(P('Phase 2 (Design)', 'DocSubTitle'))
    
    # Project Group Table matching the course reference style
    group_rows = [
        [P('<b>Role</b>', 'MemberHead'), P('<b>Student Name</b>', 'MemberHead'), P('<b>Student ID</b>', 'MemberHead')],
        [P('Group Leader', 'MemberCell'), P('Huriye Kayadelen', 'MemberCell'), P('2200005795', 'MemberCell')],
        [P('Member', 'MemberCell'), P('Tuba Nazlı İpekyüz', 'MemberCell'), P('2200004323', 'MemberCell')],
        [P('Member', 'MemberCell'), P('Kübra Öyküm Aydın', 'MemberCell'), P('2200003429', 'MemberCell')],
        [P('Member', 'MemberCell'), P('Batın Kazankaya', 'MemberCell'), P('2200005985', 'MemberCell')],
        [P('Member', 'MemberCell'), P('Tuğçe Topuz', 'MemberCell'), P('2200004231', 'MemberCell')],
    ]
    group_table = Table(group_rows, colWidths=[36 * mm, 80 * mm, 54 * mm], repeatRows=1)
    group_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), LIGHT_GREY),
        ('GRID', (0, 0), (-1, -1), 0.5, BORDER_GREY),
        ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
        ('LEFTPADDING', (0, 0), (-1, -1), 6),
        ('RIGHTPADDING', (0, 0), (-1, -1), 6),
        ('TOPPADDING', (0, 0), (-1, -1), 3),
        ('BOTTOMPADDING', (0, 0), (-1, -1), 3),
    ]))
    story.append(group_table)
    story.append(Spacer(1, 1.5 * mm))
    story.append(P('<b>Group Leader Email:</b> 2200005795@stu.iku.edu.tr | <b>Course:</b> SEN0416 Game Programming', 'Body'))
    story.append(Spacer(1, 2 * mm))
    
    story.append(P(
        "<b>Echo Protocol</b> is a first-person puzzle-action game being developed in Unity for Windows x64. "
        "In the project Phase 2 (Design), we have completed the design of all assets (sprites, 3D characters, "
        "sounds, materials, and textures) that were listed in our Phase 1 Game Design Document. "
        "As strictly specified in the Phase 2 requirements, all scripts, prefabs, and scenes are not required "
        "and have been excluded from the submission package. Figure 1 shows the completed game design and atmosphere.",
        "Body"
    ))
    
    story.extend(figure_flowable(FIG_DIR / 'simple-final.png', 'Figure 1: Echo Protocol tactical design and visual atmosphere.', 140 * mm))
    
    story.append(P('Game Overview', 'Section'))
    story.append(P(
        "The player controls an Operator navigating an industrial research facility in first person. "
        "Each loop lasts up to 20 seconds. The core mechanic records the player's movement, console interactions, "
        "and shots. When time resets, an Echo automatically replays that recorded path, allowing the player to "
        "assume a different role and cooperate with their past attempt to solve the puzzle. Combat and puzzle solving "
        "are balanced: the player is equipped with a Pulse Carbine, and an autonomous Sentry guards the facility. "
        "Direct combat, diversion, cover, and resonance frequencies can all be used to bypass security.",
        "Body"
    ))
    story.append(PageBreak())
    
    # -------------------------------------------------------------
    # PAGE 2: GAME IN DEPTH (CHARACTERS & 3D MODELS)
    # -------------------------------------------------------------
    story.append(P('Game in Depth', 'Section'))
    story.append(P(
        "In Phase 2, we have completed the design of all characters, 3D models, user interface components, "
        "materials, and sound effects, as follows.",
        "Body"
    ))
    
    # Characters bullet
    story.append(P('&bull;&nbsp;&nbsp;<b>Characters and 3D Models:</b> There are three main actors and several modular environment models in the game, as follows.', 'BulletL1'))
    story.append(P('o&nbsp;&nbsp;<b>The Operator:</b> It is the main character in the game, fully controlled by the player in first person. The operator can walk, sprint, jump, aim down sights, fire the Pulse Carbine, and interact with facility consoles.', 'BulletL2'))
    story.append(P('o&nbsp;&nbsp;<b>The Sentry:</b> It is the security obstacle for the player. It is a 4-legged autonomous robot that tracks the nearest visible player or Echo. It telegraphs its attack with a charging laser beam before firing. It can be stunned with resonance or defeated with accurate shots. Figure 2 shows the Sentry and Echo models.', 'BulletL2'))
    
    story.extend(figure_flowable(FIG_DIR / 'simple-characters.png', 'Figure 2: The Sentry robot and the holographic Echo avatars.', 140 * mm))
    
    story.append(P('o&nbsp;&nbsp;<b>The Echoes:</b> They are holographic playback recordings of the player\'s past attempts. They repeat the recorded route and actions automatically. Echo 1 is rendered with a cyan hologram shader, and Echo 2 is rendered with a violet hologram shader so that the player can easily distinguish them.', 'BulletL2'))
    story.append(P('o&nbsp;&nbsp;<b>The Pulse Carbine:</b> It is the tactical weapon designed for the operator. It fires precise energy bolts, deals 25 body damage and 50 weak-point damage, and features animated ADS sight alignment. Figure 3 shows the Pulse Carbine.', 'BulletL2'))
    
    story.extend(figure_flowable(FIG_DIR / 'simple-combat.png', 'Figure 3: The Pulse Carbine energy rifle in first-person combat.', 140 * mm))
    story.append(PageBreak())
    
    # -------------------------------------------------------------
    # PAGE 3: ENVIRONMENT, MATERIALS, TEXTURES
    # -------------------------------------------------------------
    story.append(P('&bull;&nbsp;&nbsp;<b>Environment and Puzzle Objects:</b> The facility contains modular architectural pieces and interactive devices, as follows.', 'BulletL1'))
    story.append(P('o&nbsp;&nbsp;<b>Blast Doors:</b> Heavy dark metal bulkhead doors that open pneumatically when service buttons or power relays are energized.', 'BulletL2'))
    story.append(P('o&nbsp;&nbsp;<b>Interactive Consoles:</b> Computer desks housing buttons that trigger timed gates and memory archives.', 'BulletL2'))
    story.append(P('o&nbsp;&nbsp;<b>Relays and Resonators:</b> Energy alignment terminals (Upper Amber Relay and Lower Blue Relay) and acoustic resonance targets that open bridges and stun nearby sentries. Figure 4 shows the maintenance environment and puzzle objects.', 'BulletL2'))
    
    story.extend(figure_flowable(FIG_DIR / 'simple-transfer.png', 'Figure 4: The maintenance facility environment and interactive puzzle objects.', 140 * mm))
    
    story.append(P('&bull;&nbsp;&nbsp;<b>Materials and Textures:</b> High-fidelity surface visuals are achieved using PBR textures and shaders, as follows.', 'BulletL1'))
    story.append(P('o&nbsp;&nbsp;<b>PBR Materials and Shaders:</b> We designed 20 Unity Standard PBR materials representing worn metals, rubber padding, painted trims, and emissive panels. Two custom shaders provide atmospheric volumetric lighting and holographic scanline effects for the Echoes.', 'BulletL2'))
    story.append(P('o&nbsp;&nbsp;<b>PBR Texture Sets:</b> There are 47 high-resolution 2K/4K texture maps, including BaseColor (Albedo), Normal maps, ORM (composite Ambient Occlusion, Roughness, Metallic) maps, Emissive maps, and warning Decal sheets.', 'BulletL2'))
    story.append(Spacer(1, 2 * mm))
    
    # -------------------------------------------------------------
    # PAGE 4: USER INTERFACE (UI) SPRITES
    # -------------------------------------------------------------
    story.append(P('&bull;&nbsp;&nbsp;<b>User Interface (UI) Sprites:</b> The game includes tactical HUD gauges, reticles, device icons, key badges, and menus, as follows.', 'BulletL1'))
    story.append(P('o&nbsp;&nbsp;<b>Reticles and Crosshairs:</b> There are three reticle sprites: a clean standard crosshair, a holographic ADS precision optic sight, and an angled red hitmarker for target confirmation.', 'BulletL2'))
    story.append(P('o&nbsp;&nbsp;<b>HUD Elements:</b> The HUD includes a segmented 10-pip health bar, a 20-second circular countdown timer, and Echo slot status badges (Empty, Cyan Echo 1, and Violet Echo 2). Figure 5 shows the reticles and HUD sprites.', 'BulletL2'))
    
    story.extend(figure_flowable(FIG_DIR / 'figure_ui_hud.png', 'Figure 5: Reticles, crosshairs, and tactical HUD sprites.', 140 * mm))
    
    story.append(P('o&nbsp;&nbsp;<b>Puzzle Device Glyphs and Key Badges:</b> Distinct color-coded icons represent each facility device (Console, Lens Sensor, Upper Relay, Lower Relay, Resonator, and Evacuation Door). Keyboard badges ([E], [R], [T], [Q], [X], [Tab], [Space], [Shift], [LMB], [RMB]) guide player input. Figure 6 shows the device icons and key badges.', 'BulletL2'))
    
    story.extend(figure_flowable(FIG_DIR / 'figure_devices_keys.png', 'Figure 6: Interactive device icons and key prompt badges.', 140 * mm))
    
    story.append(P('o&nbsp;&nbsp;<b>Branding and Menus:</b> A widescreen vector title logo with chromatic aberration, modular dialog panel frames, and multi-state menu buttons (normal, hover, active). Figure 7 shows the branding and UI menu panels.', 'BulletL2'))
    
    story.extend(figure_flowable(FIG_DIR / 'figure_branding_menu.png', 'Figure 7: Title logo, button states, and dialog panel frame.', 140 * mm))
    story.append(PageBreak())
    
    # -------------------------------------------------------------
    # PAGE 5: THE SOUND EFFECTS (Matching Instructor's Style!)
    # -------------------------------------------------------------
    story.append(P('&bull;&nbsp;&nbsp;<b>The Sound Effects:</b> There are 23 sound clips designed for Phase 2 to provide auditory feedback and add atmosphere during gameplay. All clips are uncompressed 44.1 kHz, 16-bit WAV files, as follows.', 'BulletL1'))
    
    sounds_list = [
        ('Pulse Fire', 'It is a sound clip played when the player or an Echo fires the Pulse Carbine. It features a sharp 260 Hz transient punch and a sub-bass thump.'),
        ('Dryfire', 'It is a sound clip played each time the player pulls the trigger with an empty magazine or on cooldown.'),
        ('Hitmarker', 'It is a sound clip played each time a shot successfully hits the enemy robot or weak point.'),
        ('Resonance Pulse', 'It is a deep sub-harmonic pulse (55/110 Hz) played during resonance activation.'),
        ('Resonance Activate', 'It is an A-Major harmonic triad chord (440/554/659/880 Hz) played when power relays align.'),
        ('Sentry Alert', 'It is an ascending FM chirp played when the sentry detects a visible player or Echo target.'),
        ('Sentry Charge', 'It is an escalating whine played while the sentry charges its beam telegraph before firing.'),
        ('Sentry Fire Beam', 'It is a heavy energy discharge sound played when the sentry shoots its laser rail.'),
        ('Sentry Stun', 'It is an electrical disruption buzz and pitch-drop sound played when the sentry is staggered.'),
        ('Console Interact', 'It is a positive electronic chime played when pressing a valid facility button or switch.'),
        ('Console Error', 'It is a double low-buzz sound played when attempting to use a locked or unavailable terminal.'),
        ('Door Slide', 'It is a heavy pneumatic hiss and mechanical servo sound played when blast doors open or close.'),
        ('Relay Power', 'It is an inductive electrical hum swelling into an energizing lock tone when activating a relay.'),
        ('Echo Record Start', 'It is a high-tech rising chime played when initiating an Echo recording loop.'),
        ('Echo Loop Reset', 'It is a time-rewind whoosh and frequency sweep played when resetting the loop with T or R.'),
        ('Echo Temporal Spawn', 'It is a holographic phase-in shimmer played when an Echo materializes at the start of a run.'),
        ('Echo Action Cue', 'It is a distinct electronic blip played when an Echo replays a recorded interaction or shot.'),
        ('UI Button Hover', 'It is a subtle micro-sine click played when the cursor hovers over interface buttons.'),
        ('UI Button Click', 'It is a crisp tactile click played when confirming a button selection or changing settings.'),
        ('Checkpoint Victory', 'It is an uplifting 4-note ascending major melody celebrating checkpoint completion.'),
        ('Loop Failure Alarm', 'It is a pulsating emergency klaxon played when the 20-second loop deadline expires.'),
        ('Facility Drone Loop', 'It is a continuous 4-voice ambient industrial drone (55/82/110/165 Hz) modeling room ventilation.'),
        ('Energy Core Loop', 'It is a deep rhythmic pulsating ambient loop modeling the distant facility research reactor.'),
    ]
    
    for s_name, s_desc in sounds_list:
        story.append(P(f'o&nbsp;&nbsp;<b>{s_name}:</b> {s_desc}', 'BulletL2'))
        
    story.append(PageBreak())
    
    # -------------------------------------------------------------
    # PAGES 6+: CONTENTS INVENTORY TABLE (Required for Phase 2)
    # -------------------------------------------------------------
    story.append(P('Content Inventory Table', 'Section'))
    story.append(P(
        "As required in Phase 2, the table below lists all 157 design assets included in the Phase 2 submission folder "
        "([Assets/](file:///C:/Users/batin/OneDrive/Masaüstü/Echo_Protocol_Phase_2_Design.zip)), including their exact file names, relative paths, formats, and descriptions.",
        "Body"
    ))
    story.append(Spacer(1, 1.5 * mm))
    
    items = get_inventory_items()
    
    table_data = [
        [P('<b>File Name</b>', 'TableHead'),
         P('<b>Category / Relative Path</b>', 'TableHead'),
         P('<b>Format & Specs</b>', 'TableHead'),
         P('<b>Description & In-Game Role</b>', 'TableHead')]
    ]
    
    for it in items:
        table_data.append([
            P(it['name'], 'TableCellBold'),
            P(it['path'], 'TableCell'),
            P(it['spec'], 'TableCell'),
            P(it['desc'], 'TableCell')
        ])
        
    inv_table = Table(table_data, colWidths=[42 * mm, 46 * mm, 28 * mm, 54 * mm], repeatRows=1)
    
    t_styles = [
        ('BACKGROUND', (0, 0), (-1, 0), LIGHT_GREY),
        ('GRID', (0, 0), (-1, -1), 0.4, BORDER_GREY),
        ('VALIGN', (0, 0), (-1, -1), 'TOP'),
        ('TOPPADDING', (0, 0), (-1, -1), 1.6),
        ('BOTTOMPADDING', (0, 0), (-1, -1), 1.6),
        ('LEFTPADDING', (0, 0), (-1, -1), 3),
        ('RIGHTPADDING', (0, 0), (-1, -1), 3),
    ]
    for r in range(1, len(table_data)):
        if r % 2 == 0:
            t_styles.append(('BACKGROUND', (0, r), (-1, r), colors.HexColor('#FAFAFA')))
            
    inv_table.setStyle(TableStyle(t_styles))
    story.append(inv_table)
    story.append(Spacer(1, 4 * mm))
    
    # -------------------------------------------------------------
    # SUBMISSION STATEMENT
    # -------------------------------------------------------------
    story.append(KeepTogether([
        P('Submission Summary', 'Section'),
        P(
            "<b>Phase 2 Compliance:</b><br/>"
            "1. <b>Complete Asset Design:</b> All sprites, 3D characters, sounds, materials, and textures listed in the GDD have been fully designed.<br/>"
            "2. <b>Exclusion Rule:</b> 0 scripts, 0 prefabs, and 0 scenes are included in the asset package.<br/>"
            "3. <b>Content Table:</b> This PDF document contains the complete content table (file name, relative path, format, description) for all 157 design assets.<br/>"
            "4. <b>Single ZIP Submission:</b> All Phase 2 work (this PDF and the complete Assets folder) is packaged in one single archive: <b>Echo_Protocol_Phase_2_Design.zip</b>.",
            "Body"
        ),
        Spacer(1, 2 * mm),
        P("<b>Submitted by:</b> Huriye Kayadelen (Group Leader - 2200005795)", 'Body'),
        P("Istanbul Kültür University | Department of Software Engineering | Fall 2026-2027", 'Caption')
    ]))

    doc.build(story, onFirstPage=page_layout, onLaterPages=page_layout)
    
    # Copy to output/pdf as well
    shutil.copy2(OUT_PDF, OUT_PDF_COPY)
    print(f"Generated PDF: {OUT_PDF} ({OUT_PDF.stat().st_size} bytes)")
    print(f"Copied to: {OUT_PDF_COPY}")

if __name__ == '__main__':
    build_pdf()
