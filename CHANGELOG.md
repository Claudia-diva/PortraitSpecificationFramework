# Changelog

## 0.12.0 - 2026-10-11

- Portrait grids size themselves to the space they get. Columns, minimum cell width and spacing are settings, with 0 columns meaning as many as fit.
- Each grid shows which portraits are on screen, such as 13-24 / 892, in its corner.
- Window size is a setting.
- Picker settings: whether a hand-picked portrait locks the character, and whether Reroll Entire Map skips party members and pets.
- Hover names, the on-screen counter and the character name buttons can be turned off. Long character names shrink to fit their button.
- All settings are in the BepInEx config, in Mod Options, and the on/off ones in the Settings tab.

## 0.11.1 - 2026-10-06

Works with Elin EA 23.352.

- Portrait Tools open instantly. Portrait thumbnails are cached on disk after the first launch, and the grids only load what is on screen.
- Hand-picked portraits stay. Picking or resetting a portrait locks the character, and a Lock / Unlock button toggles it. Map rerolls skip locked characters, party members and pets.
- Hand-picked portraits show on unique characters and on characters set up with the game's own character editor.
- Search boxes filter the character list and the portrait grids as you type. Hovering a portrait shows its name.
- Preview matches on this map lists the characters a pool filter would apply to before you add it.
- Closing Portrait Tools with unsaved pool edits asks whether to save them.
- Pools for the game's own portraits and for workshop packs are saved in the local settings package, so an update cannot wipe them. Old link files are converted automatically.
- Age limits are inclusive, and pools made with the Japanese interface save correct filters.
- Characters the game gives a fixed portrait (little girl, little sister, exile) keep it unless a special pool is made for them. The player character is never given a pool portrait.
- Settings appear in the Mod Options menu, with a button that opens Portrait Tools. The tools also live under the mod submenu of the tools menu.
- Requires YK Framework. The game reports it when the dependency is missing.
