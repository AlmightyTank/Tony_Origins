YATMMedved - Medved Cell

Custom boss and rival-cell expansion for SPT 4.0.13.

Overview

YATMMedved adds Medved Cell to SPT: a hostile BEAR-linked rival faction tied to Tony Volkov and the YetAnotherTraderMod questline.

Medved Cell is not a normal trader add-on. It is a progression-based boss system that makes Tony’s enemies grow stronger as the player advances through Tony’s main questline.

At first, Medved Cell operates quietly around Reserve. As Tony’s network expands and the player completes more of his work, Medved Cell begins appearing on more maps, in more zones, and with stronger difficulty settings.

The mod adds a custom boss squad built around Sokol, with Buran and Kedr acting as his support.

Current Medved Cell Identity

- Faction name: Medved Cell
- Main boss: Sokol
- Followers: Buran and Kedr
- Theme: BEAR-linked rival cell
- Role: Tony’s enemy faction / pressure system
- Progression: Tied to Tony main quest completion
- Main folder: "YATMMedved"

Medved Cell is designed as a side/rival system for Tony. It does not replace Tony’s main questline.

Current Features

- Custom Medved Cell boss system
- Custom boss: Sokol
- Custom followers: Buran and Kedr
- Quest-based progression
- Stage-based map expansion
- Stage-based difficulty changes
- Configurable spawn behavior
- Configurable boss and follower settings
- Custom bot profiles
- Custom loadouts
- Custom equipment pools
- Custom weapon pools
- MoreBotsAPI custom bot type support
- Designed to work with YetAnotherTraderMod / Tony
- WTT support requirement

How Medved Cell Works

Medved Cell uses Tony quest progression to decide how active the boss squad should be.

As the player completes key Tony main quests, Medved Cell unlocks new stages. Each stage can change:

- Which maps Medved Cell can appear on
- Which zones Medved Cell can spawn in
- How difficult Sokol and his followers are
- Whether the group feels early-game, mid-game, or late-game
- How much pressure Medved Cell applies to the player

The progression system should only control boss activity, map availability, spawn zones, and difficulty. Tony’s main questline stays separate.

Default Progression Direction

Stage| Unlock Condition| Maps| Difficulty
Stage 0| Default / before progression| Reserve| Easy
Stage 1| After Tony Quest 15| Reserve, Customs| Easy
Stage 2| After Tony Quest 19| Reserve, Customs| Normal
Stage 3| After Tony Quest 23| Reserve, Customs, Interchange| Normal
Stage 4| After Tony Quest 28| Reserve, Customs, Interchange, Streets| Hard
Stage 5| Late Tony progression| Reserve, Customs, Interchange, Streets| Hard / Endgame tuning

The exact quest IDs can be changed in the config.

Boss Squad

Sokol

Sokol is the leader of Medved Cell and the main boss of the squad.

He represents the organized side of the group: better planning, stronger gear, and the command role inside the cell.

Buran

Buran is a heavy support follower.

He is intended to make the squad harder to push directly and gives Sokol a stronger front-line presence.

Kedr

Kedr is a mobile support follower.

He is intended to make the group less predictable and give the squad more pressure during fights.

Requirements

- SPT 4.0.13 server
- YetAnotherTraderMod / Tony
- MoreBotsAPI
- WTT - Server Common Library

Make sure all dependencies match the SPT version used by this release.

Install

1. Close the SPT server.

2. Install WTT - Server Common Library.

3. Install MoreBotsAPI.

4. Install YetAnotherTraderMod / Tony.

5. Download the latest YATMMedved / Medved Cell release.

6. Extract the release into your SPT root folder.

Your SPT folder should contain files and folders like:

EscapeFromTarkov.exe
SPT.Server.exe
user/

The final installed server mod path should look like:

SPT/user/mods/YATMMedved

The installed folder should contain the mod files, package file, config files, database files, and compiled DLL, depending on the package setup.

Typical files and folders may include:

package.json
config/
db/
src/
YATMMedved.dll

7. Start the SPT server.

8. Check the server console for Medved Cell startup logs.

9. Launch the game.

Configuration

The main config is located in the Medved Cell mod folder.

Typical path:

SPT/user/mods/YATMMedved/config/config.json

The exact config file name may vary depending on the release.

Quest Progression Config

Quest progression controls when Medved Cell expands.

Example:

{
  "QuestProgression": {
    "Enabled": true,
    "Stages": [
      {
        "Name": "Stage 1 - First Medved Movement",
        "QuestId": "TONY_MAIN_QUEST_15_ID"
      },
      {
        "Name": "Stage 2 - Medved Pressure",
        "QuestId": "TONY_MAIN_QUEST_19_ID"
      },
      {
        "Name": "Stage 3 - Medved Expansion",
        "QuestId": "TONY_MAIN_QUEST_23_ID"
      },
      {
        "Name": "Stage 4 - Medved Lockdown",
        "QuestId": "TONY_MAIN_QUEST_28_ID"
      }
    ]
  }
}

Replace the placeholder quest IDs with the actual Tony quest IDs used by your install.

Common Config Options

Depending on the release, the config may include options like:

{
  "Enabled": true,
  "Debug": false,
  "QuestProgression": {
    "Enabled": true
  },
  "Spawns": {
    "Enabled": true
  },
  "Bosses": {
    "Sokol": {
      "Enabled": true
    },
    "Buran": {
      "Enabled": true
    },
    "Kedr": {
      "Enabled": true
    }
  }
}

Spawn Tuning

Spawn settings may include:

- Boss spawn chance
- Allowed maps
- Allowed zones
- Boss difficulty
- Escort count
- Follower settings
- Stage-based overrides

If Medved Cell is too common, lower the spawn chance.

If Medved Cell is too easy, increase later-stage difficulty or allow harder stages earlier.

If Sokol spawns without support, check the follower and escort settings.

Recommended Load Order

Recommended install order:

1. SPT
2. WTT - Server Common Library
3. MoreBotsAPI
4. YetAnotherTraderMod / Tony
5. YATMMedved / Medved Cell

Mods that edit boss waves, bot types, or map spawn settings may conflict with Medved Cell.

Troubleshooting

Sokol spawns alone

Check that:

- MoreBotsAPI is installed and loading
- Buran and Kedr are enabled
- The follower bot type names match the spawn config
- Escort count is greater than "0"
- The active progression stage allows followers
- Another mod is not overwriting the boss wave

Medved Cell does not spawn

Check that:

- The mod is enabled
- Spawn chance is not set to "0"
- The current map is allowed by the active stage
- The required Tony quest stage has been completed
- The server console shows Medved Cell loading successfully
- MoreBotsAPI is installed correctly

Server errors on startup

Check that:

- You installed the correct version for your SPT version
- WTT - Server Common Library is installed
- MoreBotsAPI is installed
- YetAnotherTraderMod / Tony is installed
- JSON config files are valid
- No trailing commas were added to config files

Bot type errors

If the game reports missing or invalid bot types, check that:

- MoreBotsAPI is installed
- The Medved Cell package was extracted correctly
- Custom bot type files are present
- No other mod is replacing the same bot type setup

Compatibility Notes

Medved Cell may conflict with mods that heavily edit:

- Boss spawns
- Bot waves
- Custom bot types
- MoreBotsAPI registrations
- Map boss zone configs
- Bot loadouts
- Bot equipment generation

If you use multiple bot overhaul mods, check your server logs carefully.

Planned Features

The following features may be expanded in future versions:

- More Sokol, Buran, and Kedr loadout variety
- More map-specific spawn zones
- More progression-based gear tuning
- More Tony dialogue and lore tied to Medved Cell
- Better long-term balance
- Additional rewards or unlocks tied to defeating Medved Cell

Notes

This is a beta release. Spawn balance, difficulty, equipment, progression stages, and compatibility are subject to change.

Medved Cell is meant to create pressure during Tony’s questline. If the squad feels too common or too strong, adjust the config before changing other bot mods.

Credits

- Original Priscilu Origins foundation: Reis
- Work on The Trader Priscilu: Anigx
- Tony concept, code, and current development: AlMightyTank
- MoreBotsAPI team / contributors for custom bot type and spawn support
- WTT team / contributors for shared server-side mod support
- SPT community for ongoing support and feedback

Links

- SPT: "https://sp-tarkov.com/"
- MoreBotsAPI: "https://forge.sp-tarkov.com/mod/2426/morebotsapi"
- YetAnotherTraderMod / Tony: "https://forge.sp-tarkov.com/mod/2185/yetanothertradermod"
- WTT - Server Common Library: "https://forge.sp-tarkov.com/mod/2310/wtt-commonlib"
- Medved Cell: "ADD_MEDVED_CELL_LINK_HERE"
- Support / Discord: "https://discord.gg/bUuJ7JzgUb"

Disclaimer

This mod is made for SPT and is not affiliated with Battlestate Games.

Do not use this mod with live Escape from Tarkov.

Escape from Tarkov is property of Battlestate Games.