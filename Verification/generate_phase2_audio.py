"""
generate_phase2_audio.py
Synthesizes high-fidelity 44.1 kHz, 16-bit uncompressed WAV sound assets
for Echo Protocol Phase 2 (Design Phase) submission.
"""

import math
import struct
import wave
from pathlib import Path

SAMPLE_RATE = 44100

def write_wav(path: Path, samples: list[float]):
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), 'wb') as wav:
        wav.setnchannels(1)  # Mono
        wav.setsampwidth(2)  # 16-bit
        wav.setframerate(SAMPLE_RATE)
        raw = bytearray()
        for s in samples:
            # Clamp between -1.0 and 1.0
            clamped = max(-1.0, min(1.0, s))
            val = int(clamped * 32767.0)
            raw.extend(struct.pack('<h', val))
        wav.writeframes(raw)
    print(f"Generated: {path.name} ({len(samples)/SAMPLE_RATE:.2f}s, {len(raw)} bytes)")

def generate_all_sounds(target_dir: Path):
    target_dir.mkdir(parents=True, exist_ok=True)
    
    # -------------------------------------------------------------
    # 1. WEAPON SOUNDS
    # -------------------------------------------------------------
    # 1.1 Pulse Carbine Fire
    dur = 0.35
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        frac = t / dur
        # Transient click (first 15ms)
        click = math.sin(2 * math.pi * 320 * t) * math.exp(-t * 90) * 0.95
        # Sub-bass thump (pitch sweep 180Hz -> 50Hz)
        pitch = 180 - frac * 130
        thump = math.sin(2 * math.pi * pitch * t) * math.exp(-t * 18) * 0.65
        # Energy laser sizzle
        noise = (math.sin(t * 14500) * math.cos(t * 8900)) * math.exp(-t * 25) * 0.25
        samples.append(click + thump + noise)
    write_wav(target_dir / "Weapon" / "sfx_weapon_pulse_fire.wav", samples)

    # 1.2 Pulse Carbine Dryfire
    dur = 0.12
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        click = (math.sin(2 * math.pi * 1250 * t) + 0.4 * math.sin(2 * math.pi * 2800 * t)) * math.exp(-t * 60) * 0.7
        samples.append(click)
    write_wav(target_dir / "Weapon" / "sfx_weapon_dryfire.wav", samples)

    # 1.3 Hitmarker Tick
    dur = 0.06
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        tick = (math.sin(2 * math.pi * 1800 * t) + math.sin(2 * math.pi * 2400 * t)) * 0.5 * math.exp(-t * 80)
        samples.append(tick)
    write_wav(target_dir / "Weapon" / "sfx_weapon_hitmarker.wav", samples)

    # -------------------------------------------------------------
    # 2. RESONANCE SOUNDS
    # -------------------------------------------------------------
    # 2.1 Resonance Pulse
    dur = 1.4
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        env = math.sin(math.pi * (t / dur)) if t < dur else 0
        lfo = 1.0 + 0.25 * math.sin(2 * math.pi * 4.5 * t)
        wave_val = (math.sin(2 * math.pi * 65 * t) * 0.6 + 
                    math.sin(2 * math.pi * 130 * t) * 0.35 +
                    math.sin(2 * math.pi * 195 * t) * 0.15) * lfo * env * math.exp(-t * 1.5)
        samples.append(wave_val)
    write_wav(target_dir / "Resonance" / "sfx_resonance_pulse.wav", samples)

    # 2.2 Resonance Activation (Harmonic Chord)
    dur = 1.2
    n = int(SAMPLE_RATE * dur)
    samples = []
    frequencies = [440.0, 554.37, 659.25, 880.0]  # A Major chord
    for i in range(n):
        t = i / SAMPLE_RATE
        env = math.exp(-t * 2.8) * (1.0 - math.exp(-t * 35.0))
        shimmer = sum(math.sin(2 * math.pi * f * t) for f in frequencies) / len(frequencies)
        high_ring = math.sin(2 * math.pi * 1760 * t) * 0.15 * math.exp(-t * 4.0)
        samples.append((shimmer * 0.8 + high_ring) * env)
    write_wav(target_dir / "Resonance" / "sfx_resonance_activate.wav", samples)

    # -------------------------------------------------------------
    # 3. SENTRY DEFENSE SYSTEM
    # -------------------------------------------------------------
    # 3.1 Sentry Alert Chirp
    dur = 0.28
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        freq = 880 + (t / dur) * 540  # 880Hz -> 1420Hz sweep
        fm = math.sin(2 * math.pi * 60 * t) * 0.2
        wave_val = math.sin(2 * math.pi * freq * t + fm) * (1 - t/dur) * 0.75
        samples.append(wave_val)
    write_wav(target_dir / "Sentry" / "sfx_sentry_alert.wav", samples)

    # 3.2 Sentry Charge Telegraph Loop
    dur = 1.0
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        frac = t / dur
        freq = 240 + (frac ** 2) * 1100  # Up to 1340 Hz
        vibrato = math.sin(2 * math.pi * (20 + frac * 25) * t) * 0.35
        wave_val = math.sin(2 * math.pi * freq * t + vibrato) * (0.3 + 0.6 * frac)
        samples.append(wave_val * 0.8)
    write_wav(target_dir / "Sentry" / "sfx_sentry_charge_loop.wav", samples)

    # 3.3 Sentry Fire Beam
    dur = 0.45
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        zap = math.sin(2 * math.pi * (750 - t * 900) * t) * math.exp(-t * 12) * 0.7
        crackle = (math.sin(t * 18000) * math.sin(t * 9400)) * math.exp(-t * 22) * 0.3
        thump = math.sin(2 * math.pi * 95 * t) * math.exp(-t * 14) * 0.5
        samples.append(zap + crackle + thump)
    write_wav(target_dir / "Sentry" / "sfx_sentry_fire_beam.wav", samples)

    # 3.4 Sentry Stun / Disrupt
    dur = 0.85
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        freq = 420 - (t / dur) * 320
        mod = math.sin(2 * math.pi * 35 * t) * 0.5
        static = (math.sin(t * 12000) * math.cos(t * 7000)) * 0.25 * (1 - t/dur)
        tone = math.sin(2 * math.pi * freq * t + mod) * 0.6 * math.exp(-t * 3.5)
        samples.append(tone + static)
    write_wav(target_dir / "Sentry" / "sfx_sentry_stun_disrupt.wav", samples)

    # -------------------------------------------------------------
    # 4. PUZZLE DEVICES & INTERACTION
    # -------------------------------------------------------------
    # 4.1 Console Interact Success
    dur = 0.28
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        click = math.sin(2 * math.pi * 1400 * t) * math.exp(-t * 110) * 0.4
        f = 523.25 if t < 0.12 else 659.25
        t_tone = t if t < 0.12 else (t - 0.12)
        tone = math.sin(2 * math.pi * f * t_tone) * math.exp(-t_tone * 14) * 0.6
        samples.append(click + tone)
    write_wav(target_dir / "Devices" / "sfx_console_interact_success.wav", samples)

    # 4.2 Console Error Denied
    dur = 0.36
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        # Double low buzz
        is_buzz = (t < 0.14) or (0.20 < t < 0.34)
        if is_buzz:
            # Low square-ish wave
            v = math.sin(2 * math.pi * 140 * t)
            buzz = (1.0 if v > 0 else -1.0) * 0.5
        else:
            buzz = 0.0
        samples.append(buzz)
    write_wav(target_dir / "Devices" / "sfx_console_error_denied.wav", samples)

    # 4.3 Door Heavy Slide
    dur = 1.35
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        air = (math.sin(t * 7500) * math.cos(t * 3200)) * math.exp(-t * 4.5) * 0.35
        rumble = (math.sin(2 * math.pi * 58 * t) + 0.4 * math.sin(2 * math.pi * 116 * t)) * (0.6 - 0.4 * (t/dur))
        clunk = math.sin(2 * math.pi * 85 * t) * math.exp(-(dur - t) * 15) * 0.7 if t > dur - 0.2 else 0
        samples.append(air + rumble * 0.5 + clunk)
    write_wav(target_dir / "Devices" / "sfx_door_heavy_slide.wav", samples)

    # 4.4 Relay Power Energize
    dur = 1.1
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        frac = t / dur
        hum = math.sin(2 * math.pi * (100 + frac * 220) * t) * frac * 0.55
        spark = (math.sin(t * 11000) * math.cos(t * 6500)) * (0.2 * math.sin(frac * math.pi))
        lock = math.sin(2 * math.pi * 880 * t) * math.exp(-(t - 0.8) * 15) * 0.6 if t >= 0.8 else 0
        samples.append(hum + spark + lock)
    write_wav(target_dir / "Devices" / "sfx_relay_power_energize.wav", samples)

    # -------------------------------------------------------------
    # 5. ECHO PLAYBACK & TIME MANIPULATION
    # -------------------------------------------------------------
    # 5.1 Echo Record Start
    dur = 0.4
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        shimmer = math.sin(2 * math.pi * (500 + t * 900) * t) * (1 - t/dur) * 0.6
        ding = math.sin(2 * math.pi * 1200 * t) * math.exp(-t * 12) * 0.4
        samples.append(shimmer + ding)
    write_wav(target_dir / "EchoPlayback" / "sfx_echo_record_start.wav", samples)

    # 5.2 Echo Loop Reset (Time Rewind Sweep)
    dur = 0.95
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        frac = t / dur
        freq = 900 - frac * 750  # 900Hz -> 150Hz
        whoosh = (math.sin(t * 5400) * math.cos(t * 2200)) * math.sin(math.pi * frac) * 0.4
        sweep = math.sin(2 * math.pi * freq * t) * math.sin(math.pi * frac) * 0.6
        samples.append(whoosh + sweep)
    write_wav(target_dir / "EchoPlayback" / "sfx_echo_loop_reset.wav", samples)

    # 5.3 Echo Temporal Spawn
    dur = 0.65
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        mod = math.sin(2 * math.pi * 18 * t) * 0.3
        hologram = math.sin(2 * math.pi * 640 * t + mod) * math.exp(-t * 4.0) * (1 - math.exp(-t * 25)) * 0.7
        samples.append(hologram)
    write_wav(target_dir / "EchoPlayback" / "sfx_echo_temporal_spawn.wav", samples)

    # 5.4 Echo Action Cue
    dur = 0.18
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        cue = (math.sin(2 * math.pi * 950 * t) + 0.5 * math.sin(2 * math.pi * 1425 * t)) * math.exp(-t * 22) * 0.65
        samples.append(cue)
    write_wav(target_dir / "EchoPlayback" / "sfx_echo_action_cue.wav", samples)

    # -------------------------------------------------------------
    # 6. UI FEEDBACK & MISSION STATUS
    # -------------------------------------------------------------
    # 6.1 UI Button Hover
    dur = 0.05
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        hover = math.sin(2 * math.pi * 1400 * t) * math.exp(-t * 120) * 0.45
        samples.append(hover)
    write_wav(target_dir / "UI_Feedback" / "sfx_ui_button_hover.wav", samples)

    # 6.2 UI Button Click
    dur = 0.08
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        click = math.sin(2 * math.pi * 1850 * t) * math.exp(-t * 90) * 0.6
        samples.append(click)
    write_wav(target_dir / "UI_Feedback" / "sfx_ui_button_click.wav", samples)

    # 6.3 Checkpoint Victory Jingle
    dur = 1.7
    n = int(SAMPLE_RATE * dur)
    samples = []
    # Major triad: C5 (523.25), E5 (659.25), G5 (783.99), C6 (1046.5)
    notes = [
        (0.00, 0.28, 523.25),
        (0.20, 0.48, 659.25),
        (0.40, 0.70, 783.99),
        (0.60, 1.70, 1046.50),
    ]
    raw_s = [0.0] * n
    for start, end, freq in notes:
        start_idx = int(start * SAMPLE_RATE)
        end_idx = min(n, int(end * SAMPLE_RATE))
        for idx in range(start_idx, end_idx):
            t_loc = (idx - start_idx) / SAMPLE_RATE
            env = math.exp(-t_loc * 3.2) * (1 - math.exp(-t_loc * 40))
            tone = (math.sin(2 * math.pi * freq * t_loc) + 0.3 * math.sin(2 * math.pi * freq * 2 * t_loc)) * env * 0.35
            raw_s[idx] += tone
    write_wav(target_dir / "UI_Feedback" / "sfx_checkpoint_victory.wav", raw_s)

    # 6.4 Loop Failure Alarm
    dur = 1.15
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        alarm = math.sin(2 * math.pi * 220 * t) * (0.5 + 0.5 * math.sin(2 * math.pi * 3.5 * t)) * (1 - t/dur * 0.4) * 0.5
        sub = math.sin(2 * math.pi * 75 * t) * math.exp(-t * 1.5) * 0.3
        samples.append(alarm + sub)
    write_wav(target_dir / "UI_Feedback" / "sfx_loop_failure_alarm.wav", samples)

    # -------------------------------------------------------------
    # 7. ATMOSPHERE & AMBIENCE (Seamless Loops)
    # -------------------------------------------------------------
    # 7.1 Facility Drone Loop
    dur = 4.0
    n = int(SAMPLE_RATE * dur)
    samples = []
    voices = [55.0, 82.41, 110.0, 164.81]
    for i in range(n):
        t = i / SAMPLE_RATE
        val = 0.0
        for idx, freq in enumerate(voices):
            val += math.sin(2 * math.pi * freq * t + idx * 1.2) * (0.25 / (idx + 1))
        # Subtle air noise
        air = (math.sin(t * 3100) * math.cos(t * 1700)) * 0.04
        samples.append(val * 0.5 + air)
    write_wav(target_dir / "Atmosphere" / "amb_facility_drone_loop.wav", samples)

    # 7.2 Energy Core Resonant Loop
    dur = 4.0
    n = int(SAMPLE_RATE * dur)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        lfo = 0.6 + 0.4 * math.sin(2 * math.pi * 0.5 * t)  # 2-second breath cycle
        core = (math.sin(2 * math.pi * 45 * t) * 0.6 + math.sin(2 * math.pi * 90 * t) * 0.3) * lfo
        samples.append(core * 0.6)
    write_wav(target_dir / "Atmosphere" / "amb_energy_core_loop.wav", samples)

if __name__ == '__main__':
    out_dir = Path(r"C:\Users\batin\OneDrive\Masaüstü\unityproje\EchoProtocol\output\Phase_2_Design\Assets\Sounds")
    generate_all_sounds(out_dir)
    print("All Phase 2 sound assets generated successfully!")
