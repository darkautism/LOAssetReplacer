# LastOrigin Asset Replace Mod

This project is a LastOrigin asset replace mod. It can replace game asset.

## Why we need this mod?

Because Lastorigin jp (DMM) modified in-game pictures. Although he is to comply with Japanese regulations, we still want him to keep the most pristine asset.

An HongRyeon(¬õ½¬) example:

```
DMM JP version:
https://cdn.discordapp.com/attachments/729585197326139413/979294986803773450/unknown.png
KR version:
https://cdn.discordapp.com/attachments/729585197326139413/979295512895295508/unknown.png
```

## Install guide

1. Install [BepInEx6](https://github.com/BepInEx/BepInEx/releases)
2. Edit doorstop_config.ini, And add MonoBackend
```
[UnityDoorstop]
# Specifies whether assembly executing is enabled
enabled=true
# Specifies the path (absolute, or relative to the game's exe) to the DLL/EXE that should be executed by Doorstop
targetAssembly=BepInEx\core\BepInEx.IL2CPP.dll
# Specifies whether Unity's output log should be redirected to <current folder>\output_log.txt
redirectOutputLog=false

[MonoBackend]
runtimeLib=mono\MonoBleedingEdge\EmbedRuntime\mono-2.0-sgen.dll
configDir=mono\MonoBleedingEdge\etc
corlibDir=mono\Managed
# Specifies whether the mono soft debugger is enabled
debugEnabled=false
# Specifies whether the mono soft debugger should suspend the process and wait for the remote debugger
debugSuspend=false
# Specifies the listening address the soft debugger
debugAddress=127.0.0.1:10000
```
3. Place `LOAssetReplacer.dll` into `BepInEx\plugins`
4. Create `mod` dir in your game root dir.
5. Place your asset mod into mod dir.(You can get these asset from LastOrigin kr version unity download cache).

