# Yes Chef!

A single-chef kitchen game built for the Tentworks Interactive developer test.

**Unity 6000.3.10f1 · URP · Input System · Cinemachine 3 · TextMeshPro**

Open `Assets/_Project/Scenes/Kitchen.unity` and press Play.

![Gameplay](Docs/gameplay.png)

---

## The game

You run a one-man kitchen for three minutes. Four customer windows each show an order;
fetch raw ingredients from the refrigerators, prepare them at the right station, and hand
them over before the order goes stale.

| Ingredient | Preparation | Value |
|---|---|---|
| Vegetable | Chop on a table (2s) | 20 |
| Cheese | None — deliver as-is | 10 |
| Meat | Cook on a stove (6s) | 30 |

An order scores **the sum of its ingredient values minus one point per whole second it has
been open**. Time is floored, so a 14.99-second delivery only costs 14 points, and a slow
order can finish negative. Windows refill five seconds after being cleared.

### Controls

| Keyboard | Gamepad | Action |
|---|---|---|
| `W A S D` / arrow keys | Left stick / d-pad | Move |
| `E` or `Space` | South button | Use the station you are facing |
| `Esc` | Start | Pause |

The briefing screen renders these straight from the bound Input System actions, so
rebinding `Assets/_Project/Input/KitchenControls.inputactions` updates the UI automatically.

| Main menu | Briefing |
|---|---|
| ![Main menu](Docs/main-menu.png) | ![Briefing](Docs/briefing.png) |

| Pause | Game over |
|---|---|
| ![Pause](Docs/pause.png) | ![Game over](Docs/game-over.png) |

---

## Architecture

The guiding rule is **logic never touches presentation**. Every gameplay type holds state
and raises events; a separate component listens and draws. That is what makes the visual
half swappable — replace the primitives with real art and not one line of gameplay code
changes.

```
Assets/_Project/
├── Scripts/
│   ├── Core/          GameManager (round lifecycle), GameState,
│   │                  KitchenCameraFitter, CameraDirector, CameraLeanTarget, OrderImpulseFeedback
│   ├── Controls/      GameInput — the only file that references UnityEngine.InputSystem
│   ├── Characters/    PlayerController (logic)  +  PlayerVisual (bob/squash)
│   ├── Ingredients/   IngredientSO, Ingredient (logic)  +  IngredientStateVisual (raw/prepared looks)
│   ├── Stations/      BaseStation, the five station types, PreparationTimer,
│   │                  StationSelectedVisual, StationFeedbackVisual
│   ├── Orders/        Order, OrderGenerator (plain C#), OrderBoard
│   ├── Scoring/       ScoreManager, IHighScoreRepository, PlayerPrefsHighScoreRepository
│   └── UI/            Main menu + briefing, HUD, pause, game over,
│                      world-space tickets and progress bars, controls list
├── Tests/EditMode/    54 tests: scoring, order generation, the preparation timer,
│                      every station's interaction rules, ingredient hand-off
├── Art/Icons/         Procedurally generated icon and UI sprites (see below)
├── Data/Ingredients/  One IngredientSO per ingredient
├── Prefabs/           One prefab per ingredient
└── Scenes/Kitchen.unity
```

### Key decisions

**Ingredients are data, not code.** `IngredientSO` carries the score, the required
preparation, the icon and the ticket colour; its prefab authors a raw look and a prepared
look that `IngredientStateVisual` swaps on state change. Adding a fourth ingredient means creating
one asset and dropping it into a refrigerator shelf and the order pool — no code changes,
no switch statements to extend.

**One place moves an ingredient.** `IIngredientHolder` is implemented by the player's
hands, chopping tables and stove burners. All the actual moving happens in
`Ingredient.SetHolder`, which detaches from the old holder, registers with the new one and
re-parents the transform. The two sides can never disagree about who is holding what, and
`SetIngredient`/`ClearIngredient` are explicit interface implementations so nothing else
can call them by accident.

**Stations decide what an interaction means.** The player only knows how to say "interact
with that"; `BaseStation.Interact` is where a refrigerator dispenses, a table starts
chopping and a window takes delivery. Every station raises `OnInteracted` /
`OnInteractionRejected`, and `StationFeedbackVisual` turns those into a scale punch or a
red flash-and-shake — the station never knows the feedback exists.

**One timer, two stations.** Chopping and cooking are the same job with different
durations, so they share `PreparationTimer` — a plain C# countdown with its own tests —
instead of each carrying a copy of the countdown. `IHasProgress` is the contract the bars
bind to, so any future timed station gets a progress bar for free.

**Scoring is testable.** `Order`, `OrderGenerator` and `PreparationTimer` have no
MonoBehaviour lifecycle, so the fiddly rules — floored time penalty, duplicate
ingredients, negative totals, exactly-once completion — are unit tests rather than
playtests. Station rules are tested too: `KitchenTestBase` builds a chef, definitions and
live ingredients, and each station's tests drive `Interact` directly.
`OrderTests` encodes the worked example from the brief (cheese + meat in 14s = 26).

**Pausing is one line.** `GameManager` is the only object that touches `Time.timeScale`.
Every gameplay timer runs on `Time.deltaTime`, so freezing the timescale pauses cooking,
chopping and order ages simultaneously with no per-system pause handling.

**High score persists behind an interface.** `ScoreManager` depends on
`IHighScoreRepository`, not on PlayerPrefs. It commits at the end of a round rather than
during play, so a run that peaks mid-round and then bleeds points to slow orders is
recorded honestly.

**Icons are generated, not imported.** `Art/Icons` holds sixteen sprites — broccoli,
cheese wedge, steak, knife, flame, bin, cloche, snowflake, the UI glyphs and the 9-slice
panels — rasterised from signed-distance fields by an editor script, with anti-aliased
edges and one consistent style. No third-party art, nothing to license, and the same
icons appear on tickets, fridge roofs, station markers and the HUD.

![Icons](Docs/icons.png)

---

## Camera

The brief asks that the whole kitchen stays visible and that no camera movement is
required. Cinemachine is used for what it is good at without breaking either rule:

- **Two shots, one blend.** An establishing camera (high, flat) is live on the menu and the
  game-over screen; the gameplay camera is live during service. `CameraDirector` swaps
  priorities on state changes and the brain eases between them.
- **A gentle lean.** The gameplay camera composes on `CameraLeanTarget`, the kitchen centre
  nudged 12% towards the chef. Cinemachine's position composer damps the follow.
- **A nudge on delivery.** `OrderImpulseFeedback` fires a small impulse when an order
  settles — heavier when it lost points. Only the gameplay camera listens.
- **The kitchen always fits.** A fixed field of view only frames the kitchen at one aspect
  ratio and one pose. `KitchenCameraFitter` sits on each virtual camera, measures the
  kitchen's bounding box in that camera's space every frame and derives the lens FOV that
  contains it, so the lean, the blend and any window shape all keep the room in frame.

---

## Design decisions worth calling out

These were ambiguous in the brief; here is what I chose and why.

**Refrigerators are per-ingredient shelves.** The brief says refrigerators hold every raw
ingredient. A single fridge dispensing three things needs a selection menu, which is
miserable mid-service. Instead each shelf dispenses one ingredient and three shelves sit
side by side as a colour-coded, icon-labelled bank, so the player takes what they are
standing in front of. There are two banks so both halves of the kitchen have supply.

**Chopping starts automatically and the player can walk away.** The brief only explicitly
frees the player from waiting at the stove, but requiring them to stand still for two
seconds adds an input mode without adding a decision. The table stays occupied for the full
two seconds, which is what enforces "one vegetable at a time".

**Two stoves, two burners each.** The brief specifies two slots per stove but not how many
stoves. With four orders that can each ask for three meats, four concurrent burners keeps
meat from becoming a hard bottleneck.

**Station targeting is a facing-weighted overlap, not a raycast.** In a top-down kitchen
where counters sit shoulder to shoulder, a ray that must land exactly feels fussy. The
player picks the best-aligned station within range, scored by facing alignment minus a
distance bias. The floor pad in front of the targeted station lights up.

**Tickets show live value, not just age.** The brief requires a timer per window. Showing
what the order is currently worth alongside it makes the decay rule legible while playing,
so the player can triage which window to serve first.

**The controls are always shown before service.** The main menu's Play and How To Play
both lead to the briefing, which lists the live bindings and ends in Start Service. That
keeps the brief's "show the controls, then click a button to begin" rule intact while still
giving the game a proper front door.

**Restart returns to the main menu.** "Resets the game" is ambiguous; reloading the scene
is the unambiguous, leak-free interpretation.

---

## Scope

Per the brief, there is no sound. Art is primitives, colour and generated sprites — enough
to read the game state at a glance. Only Unity registry packages are used (URP, Input
System, Cinemachine, TextMeshPro, Test Framework); there are no third-party plug-ins.

## Tests

`Window ▸ General ▸ Test Runner ▸ EditMode ▸ Run All`, or:

```bash
Unity.exe -runTests -batchmode -projectPath "." -testPlatform EditMode
```
