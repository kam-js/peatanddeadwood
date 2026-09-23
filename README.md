# Immersive Firewood: Peat & Deadwood

A companion mod (modid `peatanddeadwood`) to [Immersive Firewood](https://mods.vintagestory.at/vsfirewood) for Vintage Story 1.22.x.

Immersive Firewood already does per-log wood seasoning well. This mod adds the pieces it has no equivalent for:

- **Peat seasoning** — dug peat dries in place over one season into dried peat (145°C / 60s), a cheap bulk fuel: cool, smoky, and never hot enough to cook meat, fish, poultry or bread at any tech level. Vegetable and grain dishes still cook in a pot, just slowly, and a clay oven bakes on peat exactly as well as on anything else, since oven temperature doesn't depend on fuel. What peat buys you is volume — dig a shed full of it and heat a room for a very long time without ever touching your woodpile. Fresh (undried) peat is an emergency-only fuel: 95°C, below the 100°C floor needed to boil a pot at all.
- **Deadwood logs** — fallen logs scattered in forest worldgen, breakable by hand from day one (500°C / 28s).
- **Driftwood splitting** — restores the three flotsam→firewood recipes that Immersive Firewood's own recipe override silently deletes, now outputting deadwood.
- **Per-species seasoning times** — a JSON patch giving Immersive Firewood's 12 wood species distinct seasoning durations (4–18 months by density), instead of its flat 12-month default.
- **Inverted wood burn-temperature curve** — a JSON patch that makes soft, fast-growing wood (kapok, pine, redwood...) burn *hotter* and hardwood (oak, ebony...) burn *longer*, the reverse of Immersive Firewood's own curve. This is a deliberate, debatable call, not a bugfix — see "Why the wood curve is inverted" below.

### Why the wood curve is inverted

Immersive Firewood ties a wood's burn temperature to its density: denser wood burns hotter, as well as longer. Physically that's defensible — burn temperature is a sustained-equilibrium property, and a bigger coal bed runs hotter. But it's a weak reading of the *game*: it produces almost no gameplay effect (every seasoned wood already cooks meat at the same 4x rate) except in one place — preheating a firepit, where a hotter fuel banks real, spendable heat before you switch to charcoal. Under Immersive Firewood's own curve, the *best* preheater is ebony, the hardest-won hardwood in the game — exactly backwards from where a "cheap fast kindling" role belongs.

This mod inverts the curve instead: soft, fast-growing wood burns hot and fast (kapok tops out around 725°C for a 20s split), hardwood burns long and steady (ebony around 600°C for 114s). Every seasoned wood still cooks meat at 4x — nothing about ordinary cooking changes — but now softwood is the fuel worth banking a preheat split with, and hardwood is the fuel worth banking a long, steady burn with. No kiln-dried wood reaches copper's 1084°C melting point under either curve, so charcoal stays mandatory for all metalwork.

One side effect: kiln-dried kapok crosses the 825°C quicklime threshold that it misses under Immersive Firewood's own numbers. That's an accepted consequence of inverting the curve, not an oversight.

### Kiln-dried wood and the 0.72x energy ratio

Immersive Firewood's kiln-dried wood carries roughly 0.72x the total energy (temperature × duration) of the air-seasoned tier for the same species — kiln-drying should, if anything, raise a log's calorific value by drying out moisture, not cut it by 28%. That's Immersive Firewood's own balance decision, not something this mod introduces or touches: kiln-dried wood is meant to be read as a premium short-burst crafting fuel (hot fast, good for one job), not an efficient heating fuel. We leave it as shipped and note it here so the numbers don't look like an oversight of ours.

## Requirements

- Vintage Story 1.22.0+
- [Immersive Firewood](https://mods.vintagestory.at/vsfirewood) 0.4.2 (hard dependency)
- [Drying Speed Overhaul](https://mods.vintagestory.at/dryingspeedoverhaul) recommended, not required

## Install

Unzip into your `Mods` folder, or install from the mod's ModDB page.

## License

MIT — see [LICENSE](LICENSE).
