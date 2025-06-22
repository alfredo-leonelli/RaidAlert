# Raid Alert

**Version:** 1.1.0  
**Author:** AnotherPanda  
**Description:** Notifies authorized players when their base is being raided by enemies.

---

## Features

- Sends a chat message to all authorized players when their base is being raided.
- Only alerts if the raider is **not** authorized in the Tool Cupboard or a **clan member** of someone who is (to avoid sending the alert when the raiders place down a new TC).
- Customizable cooldown to avoid spam.
- Customizable detection radius for TC.
- Fully configurable raid weapons list.
- **Debug Mode**: Forces alerts to always trigger regardless of who the raider is. Useful for testing.

---

## Configuration

Upon first launch, the plugin will generate the following config file:

```json
{
  "AlertCooldown": 300.0,
  "WorldSize": 3000.0,
  "TCRange": 30.0,
  "DebugMode": false,
  "RaidWeapons": [
    "rocket_basic",
    "rocket_hv",
    "explosive.satchel.deployed",
    "explosive.timed.deployed",
    "grenade.f1.deployed",
    "grenade.beancan.deployed",
    "ammo.rocket.mlrs"
  ]
}
```

### Config Parameters

| Key             | Type  | Description                                                         |
| --------------- | ----- | ------------------------------------------------------------------- |
| `AlertCooldown` | float | Cooldown in seconds before a player can receive another raid alert. |
| `WorldSize`     | float | The size of your Rust map. Used to calculate grid positions.        |
| `TCRange`       | float | Detection range (in meters) to find the nearest TC.                 |
| `DebugMode`     | bool  | If true, always triggers alerts regardless of attacker status.      |
| `RaidWeapons`   | array | List of weapon prefab names that trigger the raid alert.            |

---

## Usage

1. When a structure is destroyed by a configured raid weapon, the plugin searches for a nearby Tool Cupboard.
2. If the attacker is **not authorized** and **not in the same clan** as someone who is authorized in that TC, an alert is sent.
   - If `DebugMode` is enabled, this check is skipped and alerts are always sent.
3. Alerts include the **grid location** of the raid and are sent only once per cooldown period to prevent spam.

---

## Dependencies

- Requires the [Clans](https://umod.org/plugins/clans) plugin.

---

## Notes

- Be sure to set the correct `WorldSize` that matches your server map (e.g., 3000, 3500, 4500).
- Debug Mode is for testing and should be disabled in production environments.
