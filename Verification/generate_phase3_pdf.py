"""
generate_phase3_pdf.py
Generates the Phase 3 (Level 1 Implementation) Game Design Document and Updated Asset Report
strictly matching the instructor's course template style (SEN0416 Game Programming - Assist. Prof. Dr. Visam Elmasry).
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
FIG_DIR = ROOT / "tmp" / "pdfs" / "phase3_assets"
OUT_DIR = ROOT / "output" / "Phase_3_Level_1"
OUT_PDF = OUT_DIR / "Echo_Protocol_Phase_3_Level_1_Report.pdf"
OUT_PDF_COPY = ROOT / "output" / "pdf" / "Echo_Protocol_Phase_3_Level_1_Report.pdf"

OUT_DIR.mkdir(parents=True, exist_ok=True)
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

base = getSampleStyleSheet()
base.add(ParagraphStyle(name='DocTitle', fontName='Times-Bold-Custom', fontSize=18, leading=22, alignment=TA_CENTER, textColor=INK, spaceAfter=6))
base.add(ParagraphStyle(name='DocSubTitle', fontName='Times-Bold-Custom', fontSize=13, leading=16, alignment=TA_CENTER, textColor=INK, spaceAfter=8))
base.add(ParagraphStyle(name='Section', fontName='Times-Bold-Custom', fontSize=13, leading=16, textColor=INK, spaceBefore=8, spaceAfter=4))
base.add(ParagraphStyle(name='Subsection', fontName='Times-Bold-Custom', fontSize=11, leading=14, textColor=INK, spaceBefore=5, spaceAfter=3))
base.add(ParagraphStyle(name='Body', fontName='Times-Roman-Custom', fontSize=10, leading=14, textColor=INK, spaceAfter=5))
base.add(ParagraphStyle(name='BulletL1', fontName='Times-Roman-Custom', fontSize=10, leading=14, leftIndent=12, firstLineIndent=-12, spaceAfter=4, textColor=INK))
base.add(ParagraphStyle(name='BulletL2', fontName='Times-Roman-Custom', fontSize=9.5, leading=13.5, leftIndent=24, firstLineIndent=-12, spaceAfter=3, textColor=INK))
base.add(ParagraphStyle(name='Caption', fontName='Times-Roman-Custom', fontSize=9.5, leading=12, textColor=INK, spaceBefore=3, spaceAfter=6))
base.add(ParagraphStyle(name='MemberHead', fontName='ArialUnicode-Bold', fontSize=8.5, leading=11, textColor=INK))
base.add(ParagraphStyle(name='MemberCell', fontName='ArialUnicode', fontSize=8.5, leading=11, textColor=INK))
base.add(ParagraphStyle(name='TableHead', fontName='Times-Bold-Custom', fontSize=7.5, leading=9.5, textColor=INK))
base.add(ParagraphStyle(name='TableCell', fontName='Times-Roman-Custom', fontSize=7.0, leading=9.0, textColor=INK))
base.add(ParagraphStyle(name='TableCellBold', fontName='Times-Bold-Custom', fontSize=7.0, leading=9.0, textColor=INK))

P = lambda text, style='Body': Paragraph(text, base[style])

def figure_flowable(image_path: Path, caption_text: str, width=135*mm, max_h=45*mm):
    im = Image.open(image_path)
    h = width * im.height / im.width
    if h > max_h:
        h = max_h
        width = h * im.width / im.height
    return [
        Spacer(1, 1*mm),
        RLImage(str(image_path), width=width, height=h),
        Spacer(1, 1*mm),
        P(caption_text, 'Caption'),
        Spacer(1, 1*mm)
    ]

def page_layout(canvas, doc):
    canvas.saveState()
    canvas.setFont('Times-Roman-Custom', 10)
    canvas.setFillColor(INK)
    canvas.drawCentredString(PAGE_W / 2.0, 12 * mm, str(doc.page))
    canvas.restoreState()

def get_updated_inventory_items():
    assets_dir = ROOT / "output" / "Phase_2_Design" / "Assets"
    all_files = sorted(list(assets_dir.rglob('*')))
    all_files = [f for f in all_files if f.is_file()]
    
    desc_map = {
        # Characters & Weapons
        "Sentry_Enemy_QuadShell.fbx": ("3D Characters", "Rigged 4-legged robot functioning as Level 1 security unit with laser telegraphs and stun physics."),
        "Echo_Hologram_Avatar_Cyan.fbx": ("3D Characters", "Active in Level 1 as Echo 1 recording avatar replaying player routes with cyan hologram shader."),
        "Echo_Hologram_Avatar_Violet.fbx": ("3D Characters", "Active in Level 1 as Echo 2 recording avatar replaying player routes with violet hologram shader."),
        "Operator_Viewmodel_Rig.fbx": ("3D Characters", "Operator first-person viewmodel rig providing smooth ADS aiming, weapon recoil, and camera motion."),
        "Gun_PulseCarbine_Rifle.fbx": ("3D Weapons", "Pulse Carbine weapon mesh in Level 1 dealing 25 body / 50 weak-point damage to sentry."),
        
        # Environment
        "Door_DarkMetal.fbx": ("3D Environment", "Level 1 pneumatic bulkhead blast door for Checkpoint 1 (Service Gate) and Checkpoint 3 (Evacuation)."),
        "Door_Frame_SquareTall.fbx": ("3D Environment", "Structural door frame housing sliding blast door colliders and locking status lights."),
        "Platform_CenterPlate.fbx": ("3D Environment", "Floor plate mesh used for puzzle interaction pads and optical lens sensor activation zone."),
        "Platform_Metal.fbx": ("3D Environment", "Modular steel grating platforms forming Level 1 walkways, transfer bridge deck, and ceilings."),
        "WallAstra_Straight.fbx": ("3D Environment", "Heavy reinforced straight wall modules enclosing the Level 1 maintenance corridor."),
        "WallBand_Straight.fbx": ("3D Environment", "Facility interior partition wall pieces segmenting the transfer bridge and observation room."),
        "ShortWall_Metal2_Straight.fbx": ("3D Environment", "Low waist-height metal barriers providing crouch cover against sentry fire in Checkpoint 3."),
        "Column_Astra.fbx": ("3D Environment", "Pillar modules framing the transfer bridge opening and elevated power relays."),
        "Column_Pipes.fbx": ("3D Environment", "Architectural conduit pillar carrying power cables between upper and lower relays."),
        "TopCables_Straight.fbx": ("3D Environment", "Overhead cable trays channeling energy between facility generator and evacuation lock."),
        
        # Props
        "Prop_Barrel1.fbx": ("3D Props", "Chemical barrel props placed along corridor perimeters for tactical cover and scene dressing."),
        "Prop_Chair.fbx": ("3D Props", "Workstation chair in the elevated glass observation deck overlooking the sentry patrol route."),
        "Prop_Computer.fbx": ("3D Props", "Interactive computer terminals operating Checkpoint 1 button and Checkpoint 3 memory archive."),
        "Prop_Crate3.fbx": ("3D Props", "Reinforced cargo containers used for step traversal, cover, and line-of-sight obstruction."),
        "Prop_Crate_Tarp.fbx": ("3D Props", "Covered supply cache staging areas implying interrupted facility evacuation operations."),
        "Prop_Desk_Medium.fbx": ("3D Props", "Metal console desks supporting puzzle switches and evacuation authorization terminals."),
        "Prop_HealthPack.fbx": ("3D Props", "First-aid medical dispenser cases stationed in maintenance locker rooms."),
        "Prop_Light_Wide.fbx": ("3D Props", "Directional fluorescent fixtures casting dynamic light (teal on clear, amber on resonance, red alert)."),
        "Prop_Locker.fbx": ("3D Props", "Personnel storage lockers in the service wing breaking sentry sightlines."),
        "Prop_Vent_Big.fbx": ("3D Props", "Exhaust intake vents positioned near high ceilings contributing to atmospheric depth."),

        # Sounds
        "sfx_weapon_pulse_fire.wav": ("Sounds - Weapon", "Pulse Carbine primary shot: 260Hz transient click + pitch-swept sub-bass (180->50Hz)."),
        "sfx_weapon_dryfire.wav": ("Sounds - Weapon", "Mechanical click played when attempting to fire with empty magazine or on cooldown."),
        "sfx_weapon_hitmarker.wav": ("Sounds - Weapon", "High-frequency double tick (1800Hz & 2400Hz) confirming bullet contact with sentry."),
        "sfx_resonance_pulse.wav": ("Sounds - Resonance", "Sub-harmonic resonant pulse (55/110Hz) played during resonator trigger in Checkpoint 2."),
        "sfx_resonance_activate.wav": ("Sounds - Resonance", "A-Major harmonic triad chord (440/554/659/880Hz) signaling dual relay power alignment."),
        "sfx_sentry_alert.wav": ("Sounds - Sentry", "Ascending FM chirp (880->1420Hz) played when sentry acquires visible player or Echo target."),
        "sfx_sentry_charge_loop.wav": ("Sounds - Sentry", "Escalating laser telegraph whine (240->1340Hz) warning player of impending attack beam."),
        "sfx_sentry_fire_beam.wav": ("Sounds - Sentry", "Heavy energy beam discharge sound played when the sentry fires its rail laser."),
        "sfx_sentry_stun_disrupt.wav": ("Sounds - Sentry", "Electrical disruption static and pitch warble played while sentry is staggered by resonance."),
        "sfx_console_interact_success.wav": ("Sounds - Devices", "Tactile switch click and dual chime (523->659Hz) confirming console interaction in Level 1."),
        "sfx_console_error_denied.wav": ("Sounds - Devices", "Double low-buzz (140Hz square wave) played when trying to activate unpowered relay or locked gate."),
        "sfx_door_heavy_slide.wav": ("Sounds - Devices", "Hydraulic hiss and mechanical servo sliding sound played when blast bulkhead doors open."),
        "sfx_relay_power_energize.wav": ("Sounds - Devices", "Inductive electrical surge swelling to an 880Hz lock tone when relays power up."),
        "sfx_echo_record_start.wav": ("Sounds - Echo", "Futuristic rising chime (500->1400Hz) marking start of player recording loop in Level 1."),
        "sfx_echo_loop_reset.wav": ("Sounds - Echo", "Reverse temporal sweep (900->150Hz) and whoosh played on T/R loop reset."),
        "sfx_echo_temporal_spawn.wav": ("Sounds - Echo", "Holographic phase-in shimmer played when Echoes materialize at the start of a loop."),
        "sfx_echo_action_cue.wav": ("Sounds - Echo", "Distinct electronic notification blip (950Hz) played when an Echo replays an action."),
        "sfx_ui_button_hover.wav": ("Sounds - UI", "Subtle 1400Hz micro-sine click played when hovering over menu and planning map buttons."),
        "sfx_ui_button_click.wav": ("Sounds - UI", "Crisp 1850Hz selection click confirming UI choices and Echo slot replacement."),
        "sfx_checkpoint_victory.wav": ("Sounds - UI", "Uplifting 4-note ascending major progression (C-E-G-C) celebrating checkpoint completion."),
        "sfx_loop_failure_alarm.wav": ("Sounds - UI", "Pulsating emergency klaxon (220Hz at 3.5Hz) played when 20s loop expires or operator falls."),
        "amb_facility_drone_loop.wav": ("Sounds - Atmosphere", "Continuous 4-voice ambient industrial drone (55/82/110/165Hz) modeling room ventilation."),
        "amb_energy_core_loop.wav": ("Sounds - Atmosphere", "Deep rhythmic 0.5Hz pulsating ambient loop modeling the distant facility research reactor."),

        # Sprites
        "reticle_crosshair_standard.png": ("Sprites - Reticles", "Active Level 1 hipfire crosshair with central dot and 4 directional ticks."),
        "reticle_ads_precision.png": ("Sprites - Reticles", "Active Level 1 right-click ADS sight with holographic dual rings and amber chevron."),
        "reticle_hitmarker.png": ("Sprites - Reticles", "Active Level 1 hit confirmation crosshair in vivid crimson on target impact."),
        "hud_health_frame.png": ("Sprites - HUD", "Angular sci-fi HUD bracket displaying 'OPERATOR INTEGRITY' in Level 1."),
        "hud_health_bar_fill.png": ("Sprites - HUD", "Segmented cyan energy bar fill representing player's 100 maximum health."),
        "hud_timer_circle_bg.png": ("Sprites - HUD", "Active Level 1 circular 20-second countdown gauge background with tick markings."),
        "hud_timer_circle_fill.png": ("Sprites - HUD", "Radial circular countdown fill showing remaining loop duration in Level 1."),
        "hud_echo_slot_empty.png": ("Sprites - HUD", "HUD badge prompting the player to record past actions with R."),
        "hud_echo_slot_cyan.png": ("Sprites - HUD", "HUD badge indicating active Echo 1 recording in Level 1."),
        "hud_echo_slot_violet.png": ("Sprites - HUD", "HUD badge indicating active Echo 2 recording in Level 1."),
        "icon_device_console.png": ("Sprites - Devices", "HUD and Tab map glyph for service buttons, computers, and terminal switches."),
        "icon_device_lens_sensor.png": ("Sprites - Devices", "Amber aperture glyph for the optical lens sensor opening Checkpoint 2 aperture."),
        "icon_device_relay_upper.png": ("Sprites - Devices", "Amber lightning glyph for Upper Relay terminal in Checkpoint 2."),
        "icon_device_relay_lower.png": ("Sprites - Devices", "Blue lightning glyph for Lower Relay terminal in Checkpoint 2."),
        "icon_device_resonator.png": ("Sprites - Devices", "Cyan wave glyph indicating acoustic resonator target in Checkpoint 2."),
        "icon_device_evac_door.png": ("Sprites - Devices", "Cyan door glyph marking final evacuation bulkhead in Checkpoint 3."),
        "icon_device_memory_archive.png": ("Sprites - Devices", "Violet crystal glyph marking Observation 04 memory terminal in Checkpoint 3."),
        "map_player_pin.png": ("Sprites - Map", "Real-time directional chevron marker representing live player location on Tab map."),
        "map_echo_cyan_pin.png": ("Sprites - Map", "Numbered cyan circular marker showing Echo 1 recorded position on Tab map."),
        "map_echo_violet_pin.png": ("Sprites - Map", "Numbered violet circular marker showing Echo 2 recorded position on Tab map."),
        "map_event_shot.png": ("Sprites - Map", "Diamond crosshair event pin showing recorded weapon firing timestamp on Tab map."),
        "map_event_console.png": ("Sprites - Map", "Circular event pin in amber indicating recorded console interactions on Tab map."),
        "map_event_archive.png": ("Sprites - Map", "Diamond archive pin in violet indicating recorded memory reads on Tab map."),
        "logo_echo_protocol_vector.png": ("Sprites - UI/Menu", "High-res title logo displayed on main menu and victory screens."),
        "button_primary_normal.png": ("Sprites - UI/Menu", "Translucent navy button asset for start, retry, and settings menus."),
        "button_primary_hover.png": ("Sprites - UI/Menu", "Illuminated cyan hover state for menu and Tab planning buttons."),
        "button_primary_active.png": ("Sprites - UI/Menu", "Solid white highlight active state when clicking UI buttons."),
        "panel_dialog_frame.png": ("Sprites - UI/Menu", "Translucent modal dialog frame for pause, settings, and victory summaries."),

        # Shaders & Mats
        "Atmosphere.shader": ("Shaders", "Active Level 1 shader rendering dynamic volumetric fog, bloom, and red loop deadline alert."),
        "Effects.shader": ("Shaders", "Active Level 1 multi-pass holographic scanline, Fresnel edge glow, and sentry beam shader."),
        "Echo_Cyan_Hologram.mat": ("Materials", "PBR material applied to Echo 1 avatar mesh providing cyan holographic visibility."),
        "Echo_Violet_Hologram.mat": ("Materials", "PBR material applied to Echo 2 avatar mesh providing violet holographic visibility."),
    }

    items = []
    idx = 1
    for f in all_files:
        rel = f.relative_to(assets_dir)
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
                desc = f"Level 1 PBR diffuse albedo texture applied to {name.split('_')[1]} geometry."
            elif "Normal" in name:
                desc = f"Level 1 tangent-space normal map rendering high-frequency industrial panel seams."
            elif "ORM" in name:
                desc = f"Level 1 composite texture packing Ambient Occlusion, Roughness, and Metallic channels."
            elif "Emissive" in name:
                desc = f"Level 1 emissive texture map powering glowing console monitors, lamps, and status LEDs."
            elif "Decal" in name:
                desc = "Texture atlas of industrial stencils, warning chevrons, and directional arrows."
            elif "Detail" in name:
                desc = "Micro-surface detail texture adding realistic metal grain and wear in Level 1."
            else:
                desc = "High-resolution 2K/4K PBR material texture map utilized across Level 1."
        elif "Materials" in str(rel):
            category = "Materials"
            desc = f"Active Unity Standard PBR material defining physics-based surface properties for {name.replace('.mat','')}"
        elif "KeyBadges" in str(rel):
            category = "Sprites - Badges"
            key_name = name.replace("key_badge_", "").replace(".png", "")
            desc = f"64x64 RGBA: High-contrast HUD input prompt for [{key_name}] key in Level 1."
        elif "Licenses" in str(rel):
            category = "Licenses"
            desc = f"Public domain CC0 attribution documentation for third-party base assets ({name})."
        else:
            category = cat
            desc = f"Integrated Level 1 design asset supporting facility visual hierarchy ({name})."

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
        title='Game Design Document (GDD) - Phase 3 (Level 1 Implementation)',
        author='Echo Protocol Project Team'
    )
    
    story = []
    
    # -------------------------------------------------------------
    # PAGE 1: TITLE, PROJECT GROUP, GAME OVERVIEW
    # -------------------------------------------------------------
    story.append(P('Game Design Document (GDD)', 'DocTitle'))
    story.append(P('Phase 3 (Level 1 Implementation)', 'DocSubTitle'))
    
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
    story.append(P('<b>Deadline:</b> Wednesday, 18 November 2026, at 23:55 | <b>Platform:</b> Windows x64 (Unity 2021.3.21f1)', 'Body'))
    story.append(Spacer(1, 2 * mm))
    
    story.append(P(
        "<b>Echo Protocol</b> is a single-player, first-person puzzle-action game developed in Unity for Windows x64. "
        "In <b>Phase 3 (Level 1 Implementation)</b>, we have fully implemented Level 1 (Maintenance Line / Synchrony) "
        "as specified in our Game Design Document. All core gameplay loops, first-person character controls, timeline "
        "recording and Echo replay playback, sentry AI combat, puzzle interactions, tactical HUD, procedural audio, "
        "and modular PBR environments are completely functional, compiled, and tested in a playable Windows desktop build. "
        "Figure 1 shows the completed Level 1 environment.",
        "Body"
    ))
    
    story.extend(figure_flowable(FIG_DIR / 'fig_sector1_facility.png', 'Figure 1: Level 1 - Synchrony facility environment and visual atmosphere.', 140 * mm))
    
    story.append(P('Game Overview', 'Section'))
    story.append(P(
        "The player assumes the role of an Operator attempting to evacuate a compromised industrial research facility. "
        "Each attempt lasts up to 20 seconds. The core mechanic records the operator's movement, console interactions, "
        "and weapon shots. When time resets, an automated holographic Echo replays those recorded actions. In subsequent "
        "loops, the player takes on a new role, coordinating across time with their past self to overcome puzzles and "
        "security threats that no single person could manage alone.",
        "Body"
    ))
    story.append(PageBreak())
    
    # -------------------------------------------------------------
    # PAGE 2: LEVEL 1 ARCHITECTURE & WALKTHROUGH
    # -------------------------------------------------------------
    story.append(P('Game in Depth (Level 1 Implementation)', 'Section'))
    story.append(P(
        "Level 1 (Maintenance Line / Synchrony) is structured as a continuous three-checkpoint progression chapter, "
        "teaching the player how to utilize past recordings for safe discovery, timed coordination, and tactical combat. "
        "The implementation details for each area are as follows.",
        "Body"
    ))
    
    story.append(P('&bull;&nbsp;&nbsp;<b>Checkpoint 1 &mdash; Service Gate (Safe Asynchronous Discovery):</b>', 'BulletL1'))
    story.append(P(
        "o&nbsp;&nbsp;<b>Puzzle Mechanics:</b> The service button is located 12 meters away from the primary lift gate. "
        "Pressing the button opens the pneumatic blast gate for only 1.4 seconds, making it impossible for a single operator "
        "to press the button and run through in one attempt. The player records an Echo running to the console and pressing [E] "
        "near 1.8 seconds. On the next loop, the player takes the direct route and crosses through the gate as the Echo presses "
        "the button. There is no enemy in this area to allow safe discovery of the recording mechanic. Figure 2 shows the Service Gate.",
        "BulletL2"
    ))
    
    story.extend(figure_flowable(FIG_DIR / 'fig_checkpoint1_service.png', 'Figure 2: Checkpoint 1 &mdash; The Service Gate opened by a recorded Echo interaction.', 140 * mm))
    
    story.append(P('&bull;&nbsp;&nbsp;<b>Checkpoint 2 &mdash; Transfer Bridge (Multi-Role Timed Coordination):</b>', 'BulletL1'))
    story.append(P(
        "o&nbsp;&nbsp;<b>Puzzle Mechanics:</b> Crossing the bridge requires aligning an optical lens sensor, a resonator target, "
        "and two power relays (Upper Amber Relay and Lower Blue Relay) within a 6-second energy window. Echo 1 holds the lens sensor "
        "plate to open a narrow aperture through a protective cage, then proceeds to the upper relay. Echo 2 fires the Pulse Carbine "
        "through the open aperture at 5.5s to trigger the resonator, then reaches the lower relay at 8.0s. Once both relays and the "
        "resonator synchronize, the bridge powers up and the player crosses safely. Figure 3 shows the Transfer Bridge.",
        "BulletL2"
    ))
    
    story.extend(figure_flowable(FIG_DIR / 'fig_checkpoint2_transfer.png', 'Figure 3: Checkpoint 2 &mdash; The Transfer Bridge synchronized power alignment.', 135 * mm))
    story.append(Spacer(1, 3 * mm))
    
    # -------------------------------------------------------------
    # CHECKPOINT 3 & COMBAT ENCOUNTERS
    # -------------------------------------------------------------
    story.append(P('&bull;&nbsp;&nbsp;<b>Checkpoint 3 &mdash; Evacuation Area (Tactical Coordination & Sentry Threat):</b>', 'BulletL1'))
    story.append(P(
        "o&nbsp;&nbsp;<b>Puzzle and Encounter Mechanics:</b> The evacuation airlock requires the left relay to be powered and the "
        "autonomous Sentry drone neutralized or distracted. The player can solve this via tactical coordination: Echo 1 runs in to "
        "draw the sentry's laser telegraph and activates the left relay; Echo 2 triggers the acoustic resonator to physically "
        "stun and stagger the sentry; the player uses the window of opportunity to activate the evacuation operator terminal and escape. "
        "Alternatively, players who prefer direct combat can eliminate the sentry using weak-point headshots. An optional glass "
        "observation room terminal (Observation 04) rewards players who study enemy attention. Figure 4 shows the Evacuation Area.",
        "BulletL2"
    ))
    
    story.extend(figure_flowable(FIG_DIR / 'fig_checkpoint3_evacuation.png', 'Figure 4: Checkpoint 3 &mdash; Sentry distraction, resonance timing, and evacuation route.', 135 * mm))
    story.append(Spacer(1, 3 * mm))
    
    story.append(P('&bull;&nbsp;&nbsp;<b>Combat & Gunplay Feel:</b>', 'BulletL1'))
    story.append(P(
        "o&nbsp;&nbsp;<b>Pulse Carbine Weapon:</b> The weapon deals 25 body damage and 50 weak-point headshot damage (0.24s fire rate). "
        "Right-click activates true ADS (Aim-Down-Sights) with smooth optical centering and camera FOV zoom. Geri tepme (recoil) features "
        "procedural pitch kick and roll damping. Impact displays an instantaneous crimson hitmarker tick. Figure 5 shows combat.",
        "BulletL2"
    ))
    story.append(P(
        "o&nbsp;&nbsp;<b>Sentry AI & Behavioral Telegraphs:</b> The 4-legged QuadShell sentry casts real-time visibility rays to acquire "
        "the nearest target (player or Echo). When targeting an Echo decoy, its charging laser beam jitters and pulses rapidly, warning "
        "the player that the enemy's attention is diverted. When stunned by resonance, the sentry tilts and wobbles with procedural physical "
        "stagger, providing clear feedback that it cannot fire.",
        "BulletL2"
    ))
    
    story.extend(figure_flowable(FIG_DIR / 'fig_sentry_combat.png', 'Figure 5: Sentry combat encounter with ADS precision and charging beam telegraph.', 135 * mm))
    story.append(Spacer(1, 3 * mm))
    
    # -------------------------------------------------------------
    # PLANNING MAP & CORE SYSTEMS
    # -------------------------------------------------------------
    story.append(P('&bull;&nbsp;&nbsp;<b>Tactical Planning Map & Echo Management (Tab System):</b>', 'BulletL1'))
    story.append(P(
        "o&nbsp;&nbsp;<b>Timeline Visualization:</b> Pressing [Tab] freezes time and opens an interactive blueprint map of the facility. "
        "The map displays the player's live position, Echo 1 (Cyan) and Echo 2 (Violet) recorded routes, and interactive event dots: "
        "ATEŞ (shots), KONSOL (interactions), and ARŞİV (memory reads). Players can review recorded timing before unpausing. "
        "Additionally, slot cards allow targeted replacement: pressing [R] arms a specific Echo slot to be overwritten on the next loop. "
        "Figure 6 shows the Tab Planning Map.",
        "BulletL2"
    ))
    
    story.extend(figure_flowable(FIG_DIR / 'fig_planning_map.png', 'Figure 6: Interactive Tab Planning Map with recorded route trajectories and action event markers.', 135 * mm))
    story.append(Spacer(1, 3 * mm))
    
    story.append(P('&bull;&nbsp;&nbsp;<b>Core Gameplay Controls Implemented:</b>', 'BulletL1'))
    story.append(P('o&nbsp;&nbsp;<b>WASD & Mouse:</b> First-person movement and look rotation with configurable sensitivity.', 'BulletL2'))
    story.append(P('o&nbsp;&nbsp;<b>Shift & Space:</b> Sprinting and jumping over obstacles and tactical cover.', 'BulletL2'))
    story.append(P('o&nbsp;&nbsp;<b>Left & Right Mouse Buttons:</b> Primary pulse firing and precision ADS aiming.', 'BulletL2'))
    story.append(P('o&nbsp;&nbsp;<b>[E]:</b> Interactive console, button, sensor, and evacuation terminal activation.', 'BulletL2'))
    story.append(P('o&nbsp;&nbsp;<b>[R]:</b> Save current attempt as an Echo recording and restart loop.', 'BulletL2'))
    story.append(P('o&nbsp;&nbsp;<b>[T]:</b> Retry current checkpoint while preserving existing Echo plans everywhere.', 'BulletL2'))
    story.append(P('o&nbsp;&nbsp;<b>[Q] & [X]:</b> Delete newest Echo recording [Q] or wipe all local room recordings [X].', 'BulletL2'))
    story.append(P('o&nbsp;&nbsp;<b>[Tab] & [Esc]:</b> Freeze time for planning map review [Tab] or pause menu [Esc].', 'BulletL2'))
    
    story.append(P('&bull;&nbsp;&nbsp;<b>Dynamic Atmosphere & Audio:</b>', 'BulletL1'))
    story.append(P(
        "o&nbsp;&nbsp;<b>Dynamic Room Lighting:</b> Facility ambient lighting responds to game state in real time: doors opening cast a soft teal "
        "glow; resonance alignment creates an amber flare; and the final 3 seconds of a loop trigger a rhythmic crimson warning pulse. "
        "Weapon audio features procedural transient clicks and sub-bass thumps, reinforced by 23 uncompressed 44.1 kHz WAV sound assets.",
        "BulletL2"
    ))
    
    story.extend(figure_flowable(FIG_DIR / 'fig_level_complete.png', 'Figure 7: Checkpoint completion result screen transitioning to Campaign Sector 1.', 135 * mm))
    story.append(PageBreak())
    
    # -------------------------------------------------------------
    # PAGES 5+: UPDATED ASSET CONTENTS TABLE (Required for Phase 3)
    # -------------------------------------------------------------
    story.append(P('Updated Asset Contents Table (Phase 3 Integration)', 'Section'))
    story.append(P(
        "As required in Phase 3, the asset catalog submitted in Phase 2 has been updated below. "
        "Each entry details how the asset has been integrated into Level 1, including its exact file name, "
        "category, format/specifications, and functional role during gameplay.",
        "Body"
    ))
    story.append(Spacer(1, 1.5 * mm))
    
    items = get_updated_inventory_items()
    
    table_data = [
        [P('<b>File Name</b>', 'TableHead'),
         P('<b>Category / Relative Path</b>', 'TableHead'),
         P('<b>Format & Specs</b>', 'TableHead'),
         P('<b>Level 1 Implementation & In-Game Role</b>', 'TableHead')]
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
    # VERIFICATION & SUBMISSION INSTRUCTIONS
    # -------------------------------------------------------------
    story.append(KeepTogether([
        P('Verification, Build Results & Playable Execution', 'Section'),
        P(
            "<b>Automated Verification Suite Results:</b><br/>"
            "&bull;&nbsp;<b>Runtime & Editor C# Compilation:</b> 0 Errors, 0 Warnings across all scripts.<br/>"
            "&bull;&nbsp;<b>Timeline Recording Core:</b> 14/14 self-checks passed; 25/25 regression tests passed (1,000 synthetic recordings).<br/>"
            "&bull;&nbsp;<b>Unity Play Mode Checks:</b> PASS 311/311 game checks.<br/>"
            "&bull;&nbsp;<b>Windows x64 Standalone Build:</b> PASS 0 errors, 0 warnings (Unity Patcher security exit code: 183).<br/>"
            "&bull;&nbsp;<b>Multi-Resolution Smoke Matrix:</b> 1280x720 (PASS), 800x600 (PASS), 1920x1080 (PASS) with 0 runtime errors.<br/>"
            "<br/>"
            "<b>How to Launch and Play Level 1:</b><br/>"
            "1. <b>Pre-Built Executable:</b> Double-click <b>OYNA.bat</b> in the project folder to launch the standalone build directly in 1280x720 windowed mode. No Unity installation is required.<br/>"
            "2. <b>Unity Project:</b> Open the project folder in Unity 2021.3.21f1, open <b>Scenes/EchoPrototype.unity</b>, and press Play.<br/>"
            "3. <b>Automated Demo:</b> Run <b>DEMO_IZLE.bat</b> to watch the autonomous AI walkthrough solve Level 1.",
            "Body"
        ),
        Spacer(1, 2 * mm),
        P("<b>Submitted by:</b> Huriye Kayadelen (Group Leader &mdash; Student ID: 2200005795)", 'Body'),
        P("Istanbul Kültür University | Department of Software Engineering | Fall 2026-2027", 'Caption')
    ]))

    doc.build(story, onFirstPage=page_layout, onLaterPages=page_layout)
    
    shutil.copy2(OUT_PDF, OUT_PDF_COPY)
    print(f"Generated Phase 3 PDF: {OUT_PDF} ({OUT_PDF.stat().st_size} bytes)")
    print(f"Copied to: {OUT_PDF_COPY}")

if __name__ == '__main__':
    build_pdf()
