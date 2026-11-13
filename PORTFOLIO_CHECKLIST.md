# ArcaneSurvivor — Portfolio publication checklist

## Before making the repository public
- [ ] Add the Unity project `Assets/`, `Packages/`, `ProjectSettings/` folders.
- [ ] Keep the `.meta` files.
- [ ] Place `.gitignore` at the Unity project root BEFORE staging files.
- [ ] Verify no `Library/`, `Temp/`, `Builds/`, `.vs/`, or personal files are staged.
- [ ] Review all third-party assets (especially KayKit) for source redistribution rights, terms, and credits.
- [ ] Review imported sounds, fonts and textures for licensing and required attributions.
- [ ] Check that no secret tokens, personal paths, or private files appear in the repository.
- [ ] Confirm current Unity version and scene path in README.

- [ ] Verify that the language links in `README.md`, `README.zh-CN.md`, and `README.ja.md` work.

## Showcase media
- [ ] Record 30–60 s gameplay footage (controls, normal combat, upgrades, key VFX).
- [ ] Capture 2–4 clean screenshots at 16:9.
- [ ] Add images to `docs/media/` and embed them in README.
- [ ] Explain personal technical contributions accurately.

## Windows release
- [x] Rebuilt after Tank HP and player/icon settings changes; Windows build tested locally (per development log).
- [ ] Run the Windows build from outside Unity.
- [ ] Test start menu, languages, level-up, Victory, Endless, restart and sound.
- [ ] ZIP the complete build folder, not only the EXE.
- [ ] Create GitHub Release `v0.1.0` and attach the ZIP.
