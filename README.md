# Yes Chef!

A single-chef kitchen game built for the Tentworks Interactive developer test.

**Unity 6000.3.10f1 · Universal Render Pipeline · Input System**

Open `Assets/_Project/Scenes/Kitchen.unity` and press Play.

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

| Input | Action |
|---|---|
| `W A S D` / arrow keys | Move |
| `E` or `Space` | Use the station you are facing |
| `Esc` | Pause |

The controls screen reads these labels straight from the bound Input System actions, so
rebinding `Assets/_Project/Input/KitchenControls.inputactions` updates the UI automatically.

---

## Architecture

The guiding rule is **logic never touches presentation**. Every gameplay type holds state
and raises events; a separate component listens and draws. That is what makes the visual
half swappable — replace the primitives with real art and not one line of gameplay code
changes.

```
Assets/_Project/
├── Scripts/
│   ├── Core/          GameManager (round lifecycle), GameState, KitchenCameraFitter
│   ├── Controls/      GameInput — the only file that references UnityEngine.InputSystem
│   ├── Characters/    PlayerController (logic)  +  PlayerVisual (bob/squash)
│   ├── Ingredients/   IngredientSO, Ingredient (logic)  +  IngredientVisual (colour/scale)
│   ├── Stations/      BaseStation and the five station types  +  StationSelectedVisual
│   ├── Orders/        Order, OrderGenerator (plain C#), OrderBoard
│   ├── Scoring/       ScoreManager, IHighScoreRepository, PlayerPrefsHighScoreRepository
│   └── UI/            HUD, screens, world-space tickets and progress bars
├── Tests/EditMode/    Scoring and order-generation tests
├── Data/Ingredients/  One IngredientSO per ingredient
├── Prefabs/           One prefab per ingredient
└── Scenes/Kitchen.unity
```

### Key decisions

**Ingredients are data, not code.** `IngredientSO` carries the score, the required
preparation and both colour/scale pairs. Adding a fourth ingredient means creating one
asset and dropping it into a refrigerator shelf and the order pool — no code changes, no
switch statements to extend.

**One place moves an ingredient.** `IIngredientHolder` is implemented by the player's
hands, chopping tables and stove burners. All the actual moving happens in
`Ingredient.SetHolder`, which detaches from the old holder, registers with the new one and
re-parents the transform. The two sides can never disagree about who is holding what, and
`SetIngredient`/`ClearIngredient` are explicit interface implementations so nothing else
can call them by accident.

**Stations decide what an interaction means.** The player only knows how to say "interact
with that"; `BaseStation.Interact` is where a refrigerator dispenses, a table starts
chopping and a window takes delivery. Adding a station type touches no existing file.

**Progress bars know nothing about stoves.** `IHasProgress` is the whole contract, so
`WorldProgressBarUI` serves chopping tables and burners alike, and any future timed station
for free.

**Scoring is testable.** `Order` and `OrderGenerator` are plain C# classes with no
MonoBehaviour lifecycle, so the fiddly rules — floored time penalty, duplicate ingredients,
negative totals — are covered by EditMode tests rather than by playing the game and hoping.
`Assets/_Project/Tests/EditMode/OrderTests.cs` encodes the worked example from the brief
(cheese + meat delivered in 14s = 26 points).

**Pausing is one line.** `GameManager` is the only object that touches `Time.timeScale`.
Every gameplay timer runs on `Time.deltaTime`, so freezing the timescale pauses cooking,
chopping and order ages simultaneously with no per-system pause handling.

**High score persists behind an interface.** `ScoreManager` depends on
`IHighScoreRepository`, not on PlayerPrefs. It commits at the end of a round rather than
during play, so a run that peaks mid-round and then bleeds points to slow orders is
recorded honestly.

---

## Design decisions worth calling out

These were ambiguous in the brief; here is what I chose and why.

**Refrigerators are per-ingredient shelves.** The brief says refrigerators hold every raw
ingredient. A single fridge dispensing three things needs a selection menu, which is
miserable mid-service. Instead each shelf dispenses one ingredient and three shelves sit
side by side as a colour-coded bank, so the player takes what they are standing in front
of. There are two banks so both halves of the kitchen have supply.

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

**Restart returns to the controls screen.** "Resets the game" is ambiguous; reloading the
scene is the unambiguous, leak-free interpretation and lets the player re-read the controls.

**The camera fits itself.** A hard-coded field of view only frames the kitchen at one
aspect ratio. `KitchenCameraFitter` measures the kitchen's bounding box in camera space and
derives the FOV that contains it, so "the entirety of the kitchen is visible" holds at any
window shape. The camera itself never moves.

---

## Scope

Per the brief, sound is absent and the art is primitives and colour — enough to read the
game state at a glance and nothing more. Only default Unity Package Manager packages are
used (URP, Input System, TextMeshPro, Test Framework); there are no third-party plug-ins.

## Tests

`Window ▸ General ▸ Test Runner ▸ EditMode ▸ Run All`, or:

```bash
Unity.exe -runTests -batchmode -projectPath "." -testPlatform EditMode
```
