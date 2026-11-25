# ECHO: Loop Protocol Presentation and Code Guide

**Course** SEN0416 Game Programming, İstanbul Kültür University  
**Team** Huriye Kayadelen, Tuba Nazlı İpekyüz, Kübra Öyküm Aydın, Batın Kazankaya and Tuğçe Topuz

## Presentation walkthrough

Open the project in Unity 2021.3.21f1. Open Assets/Scenes/MainMenu.unity and press Play. The left panel has new game, continue, controls, settings, team and exit buttons. The right panel shows four level cards.

| Scene | What to demonstrate |
| --- | --- |
| MainMenu | Start a new game, continue a saved game, open settings or view the Memory Archive. |
| Level1_ArastirmaLabi | Activate the console, leave an Echo on the pressure plate and pass through the gate. The loop lasts 20 seconds. |
| Level2_BakimHatti | Show the service route, cover, console and Sentry encounter. The loop lasts 18 seconds. |
| Level3_Eszamanlilik | Show the Service Gate, Transfer Bridge with its lens sensor and two relays, and Evacuation with the required Observation Memory. Each room has a 20-second loop. |
| Level4_Reaktor | Defeat two Sentries, use Echo recordings to coordinate two consoles and two pressure plates, and reach the exit. The final loop lasts 16 seconds. |

R saves a run as an Echo and resets the loop. T restarts the loop with existing recordings. Tab pauses the game and switches from the minimap to the planning map. The player and Echoes can take different roles in the same loop. A Sentry can target the player or a visible Echo. The Pulse Carbine uses regenerating energy.

## What the 29 C# files do

These folders are under Assets/EchoProtocol/Runtime. Some files are partial definitions of the same EchoPrototypeGame class, so only the attachable components appear separately in the Inspector.

### 01_Oyun_Yoneticisi

| File | Role |
| --- | --- |
| EchoPrototypeGame.cs | Runs the campaign, scene changes, the time loop, interactions and exit conditions. |
| EchoInput.cs | Collects keyboard and mouse input into a shared input structure. |
| EchoPreferences.cs | Stores settings, unlocked levels and progress in PlayerPrefs. |
| EchoSaveStore.cs | Validates, reads and writes JSON saves, and supports importing older saves. |
| EchoSession.cs | Collects session data, restores Echo recordings and handles checkpoints. |

### 02_Oyuncu_ve_Silah

| File | Role |
| --- | --- |
| EchoPlayer.cs | Uses CharacterController for movement, jumping, sprinting and crouching, and handles camera look. |
| EchoWeaponSettings.cs | Holds the Pulse Carbine's damage, range, fire interval, ADS, recoil and audio references. |
| EchoGameCombat.cs | Handles shot raycasts, weak-point hits, player damage and hit feedback. |
| EchoGameplayPolish.cs | Handles weapon energy, EMP overcharge, camera shake and visual combat feedback. |

### 03_Echo_Zaman_Dongusu

| File | Role |
| --- | --- |
| EchoAction.cs | Represents recorded shots and console, archive and device interactions. |
| EchoReplayActor.cs | Holds the Echo prefab's head, robot motion, color and shot material references. |
| EchoTactics.cs | Tracks tactical previews and recent replay information for Echo routes and events. |
| ReplayTimeline.cs | Stores time-stamped poses and actions, samples poses and replays actions in order. |

### 04_Dusman_ve_Animasyon

| File | Role |
| --- | --- |
| EchoEnemy.cs | Handles patrols, visibility, target selection, attacks, health and stun. |
| EchoRobotMotion.cs | Updates Animator states and motion feedback for Sentries and Echoes. |
| EchoCameraFinish.cs | Applies camera atmosphere and visual feedback for the loop. |

### 05_Bolumler_ve_Mekanikler

| File | Role |
| --- | --- |
| EchoGameWorld.cs | Connects authored room objects and loads door and world state. |
| EchoLevelDefinition.cs | Defines the names, objectives and colors of the main sectors. |
| EchoRoomLayout.cs | Holds room timing and references to the door, spawn point, plates, consoles and effects. |
| EchoResonance.cs | Handles the EMP pulse, Sentry stun, timed energy window and Data Vault interaction. |
| EchoSampleLevel.cs | Runs the Synchrony rooms, service button, lens sensor, relays and evacuation conditions. |

### 06_Arayuz_Harita_Hikaye

| File | Role |
| --- | --- |
| EchoCanvasBindings.cs | Holds references to TextMeshPro text, panels and other UI objects in the shared Canvas. |
| EchoCanvasMenus.cs | Handles menus, settings, level cards, briefings, results and credits. |
| EchoGameUI.cs | Updates the gameplay HUD, objectives, health and Echo indicators. |
| EchoPlanningMap.cs | Updates the minimap, Tab planning view, recorded routes and event markers. |

### 07_Ses_ve_Efektler

| File | Role |
| --- | --- |
| EchoAudioBank.cs | Plays assigned AudioSource clips and transitions between menu and gameplay music. |
| EchoAudioBindings.cs | Holds scene references for music, UI sounds and world audio. |
| EchoEffectsBindings.cs | Holds references and timing for shot trails, impacts, EMP arcs and Echo previews. |
| EchoImmersion.cs | Updates shot and impact effects and the weapon's visual movement. |

## What to show in the Inspector

| GameObject or prefab | Component and settings |
| --- | --- |
| EchoProtocolRuntime | EchoPrototypeGame holds Scene Entry, Max Echoes, Recording Interval and the Canvas, audio and effect references. |
| Player Operator | EchoPlayer holds Max Health, Walk Speed, Sprint Speed, Jump Speed and Gravity. |
| FPS Camera | The Camera under Player Operator has Field of View. EchoCameraFinish has Finish Template. Game settings can update the FOV. |
| Pulse Carbine | EchoWeaponSettings holds Damage, Fire Interval, Range, Aim Offset, recoil settings and Muzzle. |
| Sentry / Sentry 1 and Sentry 2 | EchoEnemy holds Base Health, Base Damage, Base Detection Range, Base Fire Interval and the patrol point references. Robot States 1.controller holds the Animator states. |
| Echo Cyan and Echo Violet | EchoReplayActor holds the actor references. Color and material distinguish the two Echoes. |
| Room root | EchoRoomLayout holds Loop Seconds, Service Window Seconds, and door and room device references. |
| Shared Campaign Canvas | Show EchoCanvasBindings, Canvas, TextMeshPro labels and Button events. The left panel alignment in MainMenu is a scene override on the prefab instance. |
| Audio and effects components | Show the AudioSource, ParticleSystem and LineRenderer references in EchoAudioBindings and EchoEffectsBindings. |

When presenting checkpoints and Continue, show how EchoSession restores the session, EchoSaveStore handles the JSON file and EchoPreferences stores settings and level progress.
