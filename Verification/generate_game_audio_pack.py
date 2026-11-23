"""
generate_game_audio_pack.py
Generates 32 rich, seamless 44.1 kHz 16-bit PCM WAV audio assets directly into
Assets/Sounds/Resources/EchoAudio/ for Echo Protocol:
- 2 full looping musical tracks (Main Menu BGM + Gameplay Tactical BGM)
- 30 distinct gameplay, UI, movement, weapon, sentry, echo, and device SFX
"""

import math
import random
import struct
import wave
from pathlib import Path

SAMPLE_RATE = 44100
OUT_ROOT = Path(r"c:\Users\batin\OneDrive\Masaüstü\unityproje\EchoProtocol\Assets\Sounds\Resources\EchoAudio")


def write_wav(path: Path, samples: list[float]):
    path.parent.mkdir(parents=True, exist_ok=True)
    # Soft limiter & normalize headroom
    peak = max((abs(x) for x in samples), default=1.0)
    scale = 0.88 / peak if peak > 0.88 else 1.0
    raw = bytearray(len(samples) * 2)
    for idx, s in enumerate(samples):
        v = max(-1.0, min(1.0, s * scale))
        struct.pack_into('<h', raw, idx * 2, int(v * 32767.0))
    with wave.open(str(path), 'wb') as wf:
        wf.setnchannels(1)
        wf.setsampwidth(2)
        wf.setframerate(SAMPLE_RATE)
        wf.writeframes(raw)
    print(f"WAV: {path.relative_to(OUT_ROOT)} ({len(samples)/SAMPLE_RATE:.2f}s)")


def lowpass(samples: list[float], alpha: float) -> list[float]:
    out = [0.0] * len(samples)
    y = 0.0
    for i, x in enumerate(samples):
        y += alpha * (x - y)
        out[i] = y
    return out


def crossfade_loop(samples: list[float], fade_sec: float = 0.35) -> list[float]:
    n = len(samples)
    f = min(int(SAMPLE_RATE * fade_sec), n // 6)
    if f <= 1:
        return samples
    out = list(samples)
    for i in range(f):
        w = i / float(f)
        out[i] = samples[i] * w + samples[n - f + i] * (1.0 - w)
    return out[: n - f]


def generate_main_menu_music():
    # 16-second rich atmospheric sci-fi pad + arpeggio (D minor: Dm9 -> Bbmaj7 -> Fmaj7 -> Asus4)
    # 4 chords x 4 seconds = 16s + 0.4s tail for seamless crossfade
    chord_sec = 4.0
    total_sec = 16.4
    n = int(SAMPLE_RATE * total_sec)
    chords = [
        [73.42, 146.83, 220.00, 261.63, 329.63],  # Dm9
        [58.27, 116.54, 174.61, 220.00, 293.66],  # Bbmaj7
        [87.31, 174.61, 220.00, 261.63, 329.63],  # Fmaj7
        [55.00, 110.00, 164.81, 220.00, 293.66],  # Asus4
    ]
    arp_notes = [
        [293.66, 349.23, 440.00, 523.25, 440.00, 349.23, 329.63, 440.00],
        [233.08, 293.66, 349.23, 440.00, 349.23, 293.66, 261.63, 349.23],
        [261.63, 329.63, 392.00, 523.25, 440.00, 349.23, 329.63, 261.63],
        [220.00, 293.66, 329.63, 440.00, 392.00, 329.63, 293.66, 220.00],
    ]
    buf = [0.0] * n
    for i in range(n):
        t = i / SAMPLE_RATE
        c_idx = int(t / chord_sec) % 4
        c_local = t - int(t / chord_sec) * chord_sec
        # Smooth chord envelope
        c_env = math.sin(math.pi * min(1.0, c_local / chord_sec)) ** 0.45
        c_env = 0.35 + 0.65 * c_env
        pad = 0.0
        for idx, freq in enumerate(chords[c_idx]):
            detune = 1.0 + (0.003 if idx % 2 == 0 else -0.003)
            w = math.sin(2 * math.pi * freq * t) + 0.35 * math.sin(2 * math.pi * freq * detune * t)
            pad += w * (0.24 if idx == 0 else 0.12)
        # Gentle breathing LFO
        breath = 0.8 + 0.2 * math.sin(2 * math.pi * 0.25 * t)
        # Crystalline sci-fi arpeggio (0.5s steps)
        step_idx = int(c_local / 0.5) % 8
        step_t = c_local - step_idx * 0.5
        arp_f = arp_notes[c_idx][step_idx]
        arp_env = math.exp(-step_t * 5.2) * (1.0 - math.exp(-step_t * 60.0))
        arp = (math.sin(2 * math.pi * arp_f * t) + 0.25 * math.sin(2 * math.pi * arp_f * 2.0 * t)) * arp_env * 0.16
        # Soft bass pulse every 2s
        beat_t = t % 2.0
        sub_env = math.exp(-beat_t * 3.0)
        sub = math.sin(2 * math.pi * chords[c_idx][0] * t) * sub_env * 0.18
        buf[i] = (pad * c_env * breath + arp + sub) * 0.48
    buf = lowpass(buf, 0.22)
    buf = crossfade_loop(buf, 0.4)
    write_wav(OUT_ROOT / "Music" / "bgm_main_menu.wav", buf)


def generate_gameplay_music():
    # 16-second tense tactical electronic pulse (120 BPM = 0.5s/beat, 0.25s eighth notes)
    total_sec = 16.25
    n = int(SAMPLE_RATE * total_sec)
    rng = random.Random(20261121)
    bass_seq = [
        73.42, 73.42, 87.31, 73.42, 65.41, 65.41, 73.42, 82.41,
        58.27, 58.27, 73.42, 58.27, 55.00, 55.00, 65.41, 73.42,
    ]
    pad_chords = [
        [146.83, 220.00, 261.63],
        [130.81, 196.00, 261.63],
        [116.54, 174.61, 220.00],
        [110.00, 164.81, 220.00],
    ]
    buf = [0.0] * n
    for i in range(n):
        t = i / SAMPLE_RATE
        bar = int(t / 4.0) % 4
        # Eighth-note driving synth bass
        eighth = int(t / 0.25)
        et = t - eighth * 0.25
        bf = bass_seq[eighth % len(bass_seq)]
        b_env = math.exp(-et * 9.5) * (1.0 - math.exp(-et * 120.0))
        bass = (math.sin(2 * math.pi * bf * t) * 0.75 + math.sin(2 * math.pi * bf * 2 * t) * 0.25) * b_env * 0.30
        # Soft kick on beats 0, 2
        beat = int(t / 0.5)
        bt = t - beat * 0.5
        kick = 0.0
        if beat % 2 == 0:
            kf = 115.0 * math.exp(-bt * 24.0) + 42.0
            kick = math.sin(2 * math.pi * kf * bt) * math.exp(-bt * 14.0) * 0.28
        # Subtle closed hi-hat on off-beats
        hat = 0.0
        if eighth % 2 == 1 and et < 0.05:
            hat = (rng.random() * 2.0 - 1.0) * math.exp(-et * 85.0) * 0.06
        # Dark tension pad
        pad = sum(math.sin(2 * math.pi * f * t) for f in pad_chords[bar]) * 0.06
        # Subtle high sonar ping at start of each 4s bar
        bar_t = t % 4.0
        ping = math.sin(2 * math.pi * 880.0 * t) * math.exp(-bar_t * 6.0) * 0.055 if bar_t < 1.0 else 0.0
        buf[i] = (bass + kick + hat + pad + ping) * 0.45
    buf = lowpass(buf, 0.28)
    buf = crossfade_loop(buf, 0.25)
    write_wav(OUT_ROOT / "Music" / "bgm_gameplay_loop.wav", buf)


def generate_sfx():
    rng = random.Random(4242)

    # --- MOVEMENT ---
    for idx, pitch in enumerate([105.0, 92.0], 1):
        dur = 0.11
        n = int(SAMPLE_RATE * dur)
        s = []
        for i in range(n):
            t = i / SAMPLE_RATE
            thud = math.sin(2 * math.pi * (pitch - t * 300) * t) * math.exp(-t * 42) * 0.55
            grit = (rng.random() * 2 - 1) * math.exp(-t * 65) * 0.22
            clack = math.sin(2 * math.pi * 780 * t) * math.exp(-t * 95) * 0.14
            s.append(thud + grit + clack)
        write_wav(OUT_ROOT / "Movement" / f"sfx_footstep_{idx}.wav", lowpass(s, 0.45))

    # Jump
    dur = 0.22
    n = int(SAMPLE_RATE * dur)
    s = [
        (math.sin(2 * math.pi * (140 + (i / SAMPLE_RATE) * 420) * (i / SAMPLE_RATE)) * 0.4
         + (rng.random() * 2 - 1) * 0.18) * math.exp(-(i / SAMPLE_RATE) * 14)
        for i in range(n)
    ]
    write_wav(OUT_ROOT / "Movement" / "sfx_player_jump.wav", lowpass(s, 0.4))

    # Land
    dur = 0.18
    n = int(SAMPLE_RATE * dur)
    s = [
        (math.sin(2 * math.pi * (95 - (i / SAMPLE_RATE) * 280) * (i / SAMPLE_RATE)) * 0.7
         + (rng.random() * 2 - 1) * 0.25) * math.exp(-(i / SAMPLE_RATE) * 26)
        for i in range(n)
    ]
    write_wav(OUT_ROOT / "Movement" / "sfx_player_land.wav", lowpass(s, 0.35))

    # Player Hurt
    dur = 0.32
    n = int(SAMPLE_RATE * dur)
    s = []
    for i in range(n):
        t = i / SAMPLE_RATE
        hit = math.sin(2 * math.pi * (190 - t * 240) * t) * math.exp(-t * 14) * 0.65
        buzz = math.sin(2 * math.pi * 92 * t + math.sin(2 * math.pi * 45 * t)) * math.exp(-t * 9) * 0.35
        noise = (rng.random() * 2 - 1) * math.exp(-t * 25) * 0.25
        s.append(hit + buzz + noise)
    write_wav(OUT_ROOT / "Movement" / "sfx_player_hurt.wav", s)

    # --- WEAPON ---
    # Pulse fire
    dur = 0.32
    n = int(SAMPLE_RATE * dur)
    s = []
    for i in range(n):
        t = i / SAMPLE_RATE
        f = t / dur
        click = math.sin(2 * math.pi * 310 * t) * math.exp(-t * 85) * 0.85
        thump = math.sin(2 * math.pi * (185 - f * 135) * t) * math.exp(-t * 18) * 0.65
        sizzle = (rng.random() * 2 - 1) * math.exp(-t * 28) * 0.28
        s.append(click + thump + sizzle)
    write_wav(OUT_ROOT / "Weapon" / "sfx_weapon_pulse_fire.wav", s)

    # ADS zoom in click
    dur = 0.10
    n = int(SAMPLE_RATE * dur)
    s = [
        (math.sin(2 * math.pi * (900 + (i / SAMPLE_RATE) * 2400) * (i / SAMPLE_RATE)) * 0.35
         + math.sin(2 * math.pi * 2200 * (i / SAMPLE_RATE)) * 0.2) * math.exp(-(i / SAMPLE_RATE) * 55)
        for i in range(n)
    ]
    write_wav(OUT_ROOT / "Weapon" / "sfx_weapon_ads_in.wav", s)

    # Hitmarker
    dur = 0.07
    n = int(SAMPLE_RATE * dur)
    s = [
        (math.sin(2 * math.pi * 1750 * (i / SAMPLE_RATE)) + math.sin(2 * math.pi * 2450 * (i / SAMPLE_RATE))) * 0.45 * math.exp(-(i / SAMPLE_RATE) * 75)
        for i in range(n)
    ]
    write_wav(OUT_ROOT / "Weapon" / "sfx_weapon_hitmarker.wav", s)

    # Headshot critical chime
    dur = 0.18
    n = int(SAMPLE_RATE * dur)
    s = [
        (math.sin(2 * math.pi * 1320 * (i / SAMPLE_RATE)) * 0.45
         + math.sin(2 * math.pi * 1980 * (i / SAMPLE_RATE)) * 0.4
         + math.sin(2 * math.pi * 2640 * (i / SAMPLE_RATE)) * 0.3) * math.exp(-(i / SAMPLE_RATE) * 22)
        for i in range(n)
    ]
    write_wav(OUT_ROOT / "Weapon" / "sfx_weapon_headshot.wav", s)

    # Dryfire
    dur = 0.09
    n = int(SAMPLE_RATE * dur)
    s = [(math.sin(2 * math.pi * 1300 * (i / SAMPLE_RATE)) + 0.4 * math.sin(2 * math.pi * 2900 * (i / SAMPLE_RATE))) * 0.5 * math.exp(-(i / SAMPLE_RATE) * 70) for i in range(n)]
    write_wav(OUT_ROOT / "Weapon" / "sfx_weapon_dryfire.wav", s)

    # --- SENTRY ---
    # Alert
    dur = 0.26
    n = int(SAMPLE_RATE * dur)
    s = [math.sin(2 * math.pi * (860 + (i / n) * 580) * (i / SAMPLE_RATE) + 0.2 * math.sin(2 * math.pi * 55 * (i / SAMPLE_RATE))) * (1 - i / n) * 0.65 for i in range(n)]
    write_wav(OUT_ROOT / "Sentry" / "sfx_sentry_alert.wav", s)

    # Charge
    dur = 0.65
    n = int(SAMPLE_RATE * dur)
    s = []
    for i in range(n):
        t = i / SAMPLE_RATE
        f = i / n
        freq = 260 + (f ** 1.8) * 1050
        s.append(math.sin(2 * math.pi * freq * t + 0.3 * math.sin(2 * math.pi * 26 * t)) * (0.25 + 0.55 * f) * (1.0 - max(0.0, (f - 0.85) * 6.0)))
    write_wav(OUT_ROOT / "Sentry" / "sfx_sentry_charge.wav", s)

    # Sentry fire beam
    dur = 0.40
    n = int(SAMPLE_RATE * dur)
    s = []
    for i in range(n):
        t = i / SAMPLE_RATE
        zap = math.sin(2 * math.pi * (780 - t * 950) * t) * math.exp(-t * 13) * 0.65
        crackle = (rng.random() * 2 - 1) * math.exp(-t * 24) * 0.28
        thump = math.sin(2 * math.pi * 92 * t) * math.exp(-t * 15) * 0.5
        s.append(zap + crackle + thump)
    write_wav(OUT_ROOT / "Sentry" / "sfx_sentry_fire.wav", s)

    # Sentry stun
    dur = 0.72
    n = int(SAMPLE_RATE * dur)
    s = []
    for i in range(n):
        t = i / SAMPLE_RATE
        freq = 440 - (i / n) * 330
        s.append((math.sin(2 * math.pi * freq * t + 0.5 * math.sin(2 * math.pi * 36 * t)) * 0.55 + (rng.random() * 2 - 1) * 0.2) * math.exp(-t * 3.8))
    write_wav(OUT_ROOT / "Sentry" / "sfx_sentry_stun.wav", s)

    # Sentry destroy
    dur = 0.90
    n = int(SAMPLE_RATE * dur)
    s = []
    for i in range(n):
        t = i / SAMPLE_RATE
        boom = math.sin(2 * math.pi * (120 * math.exp(-t * 4) + 32) * t) * math.exp(-t * 4.5) * 0.75
        debris = (rng.random() * 2 - 1) * math.exp(-t * 6.5) * 0.42
        powerdown = math.sin(2 * math.pi * max(40, 520 - t * 540) * t) * math.exp(-t * 3.5) * 0.35
        s.append(boom + debris + powerdown)
    write_wav(OUT_ROOT / "Sentry" / "sfx_sentry_destroy.wav", lowpass(s, 0.45))

    # --- ECHO & LOOP ---
    # Record start
    dur = 0.36
    n = int(SAMPLE_RATE * dur)
    s = [(math.sin(2 * math.pi * (520 + (i / SAMPLE_RATE) * 950) * (i / SAMPLE_RATE)) * 0.5 + math.sin(2 * math.pi * 1240 * (i / SAMPLE_RATE)) * math.exp(-(i / SAMPLE_RATE) * 14) * 0.35) * (1 - i / n) for i in range(n)]
    write_wav(OUT_ROOT / "Echo" / "sfx_echo_record_start.wav", s)

    # Loop reset
    dur = 0.62
    n = int(SAMPLE_RATE * dur)
    s = []
    for i in range(n):
        t = i / SAMPLE_RATE
        f = i / n
        env = math.sin(math.pi * f)
        s.append((math.sin(2 * math.pi * (920 - f * 720) * t) * 0.55 + (rng.random() * 2 - 1) * 0.2) * env)
    write_wav(OUT_ROOT / "Echo" / "sfx_echo_loop_reset.wav", s)

    # Echo spawn
    dur = 0.48
    n = int(SAMPLE_RATE * dur)
    s = [math.sin(2 * math.pi * 660 * (i / SAMPLE_RATE) + 0.3 * math.sin(2 * math.pi * 18 * (i / SAMPLE_RATE))) * math.exp(-(i / SAMPLE_RATE) * 5.0) * (1 - math.exp(-(i / SAMPLE_RATE) * 30)) * 0.55 for i in range(n)]
    write_wav(OUT_ROOT / "Echo" / "sfx_echo_spawn.wav", s)

    # Echo action cue
    dur = 0.16
    n = int(SAMPLE_RATE * dur)
    s = [(math.sin(2 * math.pi * 980 * (i / SAMPLE_RATE)) + 0.5 * math.sin(2 * math.pi * 1470 * (i / SAMPLE_RATE))) * math.exp(-(i / SAMPLE_RATE) * 24) * 0.5 for i in range(n)]
    write_wav(OUT_ROOT / "Echo" / "sfx_echo_action_cue.wav", s)

    # Loop warning tick (last 3 seconds)
    dur = 0.12
    n = int(SAMPLE_RATE * dur)
    s = [(math.sin(2 * math.pi * 740 * (i / SAMPLE_RATE)) * 0.5 + math.sin(2 * math.pi * 1480 * (i / SAMPLE_RATE)) * 0.25) * math.exp(-(i / SAMPLE_RATE) * 38) for i in range(n)]
    write_wav(OUT_ROOT / "Echo" / "sfx_loop_warning_tick.wav", s)

    # --- DEVICES ---
    # Console interact
    dur = 0.26
    n = int(SAMPLE_RATE * dur)
    s = []
    for i in range(n):
        t = i / SAMPLE_RATE
        f = 523.25 if t < 0.11 else 659.25
        tl = t if t < 0.11 else t - 0.11
        s.append(math.sin(2 * math.pi * f * tl) * math.exp(-tl * 15) * 0.55 + math.sin(2 * math.pi * 1450 * t) * math.exp(-t * 110) * 0.3)
    write_wav(OUT_ROOT / "Devices" / "sfx_console_interact.wav", s)

    # Console error
    dur = 0.32
    n = int(SAMPLE_RATE * dur)
    s = []
    for i in range(n):
        t = i / SAMPLE_RATE
        on = (t < 0.12) or (0.18 < t < 0.30)
        s.append((0.4 if math.sin(2 * math.pi * 145 * t) > 0 else -0.4) if on else 0.0)
    write_wav(OUT_ROOT / "Devices" / "sfx_console_error.wav", lowpass(s, 0.35))

    # Door open
    dur = 0.95
    n = int(SAMPLE_RATE * dur)
    s = []
    for i in range(n):
        t = i / SAMPLE_RATE
        air = (rng.random() * 2 - 1) * math.exp(-t * 5.0) * 0.28
        rumble = (math.sin(2 * math.pi * 62 * t) + 0.4 * math.sin(2 * math.pi * 124 * t)) * (0.6 - 0.45 * (i / n)) * 0.5
        clunk = math.sin(2 * math.pi * 88 * t) * math.exp(-abs(t - 0.82) * 35) * 0.5
        s.append(air + rumble + clunk)
    write_wav(OUT_ROOT / "Devices" / "sfx_door_open.wav", lowpass(s, 0.4))

    # Resonance pulse
    dur = 1.10
    n = int(SAMPLE_RATE * dur)
    s = []
    for i in range(n):
        t = i / SAMPLE_RATE
        env = math.sin(math.pi * (i / n)) * math.exp(-t * 1.6)
        lfo = 1.0 + 0.25 * math.sin(2 * math.pi * 5.0 * t)
        s.append((math.sin(2 * math.pi * 68 * t) * 0.6 + math.sin(2 * math.pi * 136 * t) * 0.35) * lfo * env)
    write_wav(OUT_ROOT / "Devices" / "sfx_resonance_pulse.wav", s)

    # Resonance activate
    dur = 1.00
    n = int(SAMPLE_RATE * dur)
    freqs = [440.0, 554.37, 659.25, 880.0]
    s = []
    for i in range(n):
        t = i / SAMPLE_RATE
        env = math.exp(-t * 3.0) * (1.0 - math.exp(-t * 35.0))
        s.append((sum(math.sin(2 * math.pi * f * t) for f in freqs) / 4.0) * env * 0.75)
    write_wav(OUT_ROOT / "Devices" / "sfx_resonance_activate.wav", s)

    # Relay power
    dur = 0.75
    n = int(SAMPLE_RATE * dur)
    s = []
    for i in range(n):
        t = i / SAMPLE_RATE
        f = i / n
        hum = math.sin(2 * math.pi * (110 + f * 240) * t) * f * 0.45
        lock = math.sin(2 * math.pi * 880 * t) * math.exp(-(t - 0.55) * 18) * 0.5 if t >= 0.55 else 0.0
        s.append(hum + lock)
    write_wav(OUT_ROOT / "Devices" / "sfx_relay_power.wav", s)

    # --- UI ---
    # UI click
    dur = 0.07
    n = int(SAMPLE_RATE * dur)
    s = [(math.sin(2 * math.pi * 1750 * (i / SAMPLE_RATE)) * 0.45 + math.sin(2 * math.pi * 880 * (i / SAMPLE_RATE)) * 0.25) * math.exp(-(i / SAMPLE_RATE) * 85) for i in range(n)]
    write_wav(OUT_ROOT / "UI" / "sfx_ui_click.wav", s)

    # Checkpoint victory
    dur = 1.45
    n = int(SAMPLE_RATE * dur)
    s = [0.0] * n
    for start, end, freq in [(0.00, 0.26, 523.25), (0.18, 0.44, 659.25), (0.36, 0.64, 783.99), (0.54, 1.45, 1046.50)]:
        i0, i1 = int(start * SAMPLE_RATE), min(n, int(end * SAMPLE_RATE))
        for idx in range(i0, i1):
            tl = (idx - i0) / SAMPLE_RATE
            env = math.exp(-tl * 3.4) * (1.0 - math.exp(-tl * 45.0))
            s[idx] += (math.sin(2 * math.pi * freq * tl) + 0.28 * math.sin(2 * math.pi * freq * 2 * tl)) * env * 0.38
    write_wav(OUT_ROOT / "UI" / "sfx_checkpoint_win.wav", s)

    # Loop fail
    dur = 0.95
    n = int(SAMPLE_RATE * dur)
    s = [(math.sin(2 * math.pi * 210 * (i / SAMPLE_RATE)) * (0.5 + 0.5 * math.sin(2 * math.pi * 4.0 * (i / SAMPLE_RATE))) * 0.45 + math.sin(2 * math.pi * 75 * (i / SAMPLE_RATE)) * 0.3) * math.exp(-(i / SAMPLE_RATE) * 1.4) for i in range(n)]
    write_wav(OUT_ROOT / "UI" / "sfx_loop_fail.wav", s)

    # Archive unlock
    dur = 0.85
    n = int(SAMPLE_RATE * dur)
    s = [0.0] * n
    for start, end, freq in [(0.00, 0.30, 587.33), (0.14, 0.45, 880.00), (0.28, 0.85, 1174.66)]:
        i0, i1 = int(start * SAMPLE_RATE), min(n, int(end * SAMPLE_RATE))
        for idx in range(i0, i1):
            tl = (idx - i0) / SAMPLE_RATE
            env = math.exp(-tl * 4.2) * (1.0 - math.exp(-tl * 50.0))
            s[idx] += math.sin(2 * math.pi * freq * tl) * env * 0.42
    write_wav(OUT_ROOT / "UI" / "sfx_archive_unlock.wav", s)


if __name__ == "__main__":
    generate_main_menu_music()
    generate_gameplay_music()
    generate_sfx()
    print("All 32 game audio assets generated in Assets/Sounds/Resources/EchoAudio!")
