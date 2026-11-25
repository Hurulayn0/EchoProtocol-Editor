# ECHO: Loop Protocol Asset Credits and Licenses

We built the environments from modular pieces in Unity. The level layouts are edited in the scenes under Assets/Scenes.

## Quaternius Modular Sci-Fi MegaKit Standard

- Creator: Quaternius.
- Source: https://quaternius.com/packs/modularscifimegakit.html
- Creator's distribution page: https://opengameart.org/content/modular-sci-fi-megakit
- License: CC0 1.0 Universal. The original text is in Assets/ThirdParty/Quaternius/LICENSE_ModularSciFi.txt.
- Use: Modular walls and floors, doors and frames, columns, cables, light fixtures and console computers. The project uses the free Standard edition.

## Quaternius Sci-Fi Essentials Kit Standard

- Creator: Quaternius.
- Source: https://quaternius.com/packs/scifiessentialskit.html
- Creator's distribution page: https://opengameart.org/content/sci-fi-essentials-kit
- License: CC0 1.0 Universal. The original text is in Assets/ThirdParty/Quaternius/LICENSE_Essentials.txt.
- Use: The QuadShell robot and its idle and walk animations, the Rifle model, desks, chairs, lockers, barrels and crates. The Rifle is the Pulse Carbine's visual model. Sentry is the game's enemy type.

## Unity setup

The project includes 22 selected FBX models and their textures. The materials use Unity's Built-in Standard shader with albedo, normal and available emissive maps. The prepared prefabs and materials are under Assets/EchoProtocol/Resources/EchoArt.

## Sound and music

Assets/Sesler/Resources/EchoAudio contains 32 WAV files in Music, Movement, Weapon, Sentry, Echo, Devices and UI. All 32 clips are assigned to AudioSource components in Unity scenes or prefabs. The game plays those clips, adjusts volume and pitch, and manages music transitions. It does not create audio files at runtime.

The Phase 2 sound catalog describes the intended use and technical format of the sounds. The creator and distribution license could not be verified for each current WAV file.

## TextMesh Pro

The Liberation Sans license is at Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt. The EmojiOne attribution is at Assets/TextMesh Pro/Sprites/EmojiOne Attribution.txt. Both files are preserved in their original locations.
