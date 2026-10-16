# AcolyteBetterEcho

Reworks the Acolyte's **Anima Echo** special. Dark Summons is unchanged.

## Changes

- **Fresh start:** activating Anima Echo resets all skill cooldowns, including Sacrifice and Blade Slice quickstrike charges.
- **Stronger Sacrifice:** with Anima Echo selected, each Sacrifice grants **3** Convictions instead of 2. Kills still grant 1 extra.
- **Stored Convictions:** Convictions earned during the Echo Litany are stored and granted when it ends, instead of extending it. The meter still caps at 7, so anything above that is lost, and stored Convictions are lost if you die during the Litany.
- **Litany chaining:** storing 7 Convictions during one Litany readies Anima Echo again the moment it ends, with another full skill reset. This is intended.
- **Updated descriptions** in every language the game supports.

## Multiplayer

All players should install the mod and use the same config, since Conviction gains are handled by the host.

## Configuration

Settings are in `BepInEx/config/com.EviLord.AcolyteBetterEcho.cfg` after the first launch.

- `Enabled`: turns all of the mod's changes on or off (default on). Requires a restart.
- `EchoSacrificeConvictions`: Convictions granted per Sacrifice with Anima Echo selected (default 3, vanilla 2). Gameplay uses a new value immediately; the skill descriptions update after a restart.
