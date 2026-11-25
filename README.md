# ECHO: Loop Protocol

A first-person puzzle and action game in which you record your actions in a time loop. Echoes replay those actions automatically in later loops.

## Open the Unity project

1. Add the project root to Unity Hub and open it with **Unity 2021.3.21f1**.
2. Open **Assets/Scenes/MainMenu.unity** and press Play.
3. Start a new game or select Continue if you have a saved game.

| Order | Scene | Level |
| --- | --- | --- |
| Menu | MainMenu | Main menu |
| 1 | Level1_ArastirmaLabi | Research Laboratory |
| 2 | Level2_BakimHatti | Maintenance Line |
| 3 | Level3_Eszamanlilik | Synchrony |
| 4 | Level4_Reaktor | Reactor |

Each level has its own scene under Assets/Scenes. The C# files are grouped by role under Assets/EchoProtocol/Runtime. Use Player Operator for player settings, Pulse Carbine for weapon settings, and the room's EchoRoomLayout component for loop duration.

## Controls

| Input | Action |
| --- | --- |
| WASD / mouse | Move and look |
| Space / Shift / Ctrl | Jump, sprint and crouch |
| Left / right mouse button | Fire and aim |
| E | Interact |
| R | Save the run as an Echo and start a new loop |
| T | Restart the loop and keep existing Echo recordings |
| Q / X | Remove the last Echo or clear all recordings |
| Tab / Esc | Open the planning map or pause |

## Run the Windows game

Extract the entire submission ZIP and open Windows_Game/EchoProtocol.exe. Keep EchoProtocol_Data, MonoBleedingEdge and the DLL files next to the EXE. To run the project in the editor, add the extracted ZIP's root to Unity Hub and follow the Unity steps above.

[Asset credits and licenses](ASSET_CREDITS.md)

## Phase 1 GDD and the current campaign

The Service Gate, Transfer Bridge and Evacuation from the Phase 1 GDD are the three checkpoints in Level3_Eszamanlilik. The released Service Gate has a Sentry. The Transfer Bridge EMP window lasts up to 10 seconds, depending on the time left in the loop. The Evacuation checkpoint requires the Observation Memory.

The campaign runs from Research Laboratory through Maintenance Line and Synchrony to Reactor. The final challenge combines two Sentries with a two-console and two-pressure-plate puzzle. Use Echo recordings to coordinate the puzzle and reach the exit.
