# Block Blast Solver

Recreate an 8×8 board, choose up to three pieces, and calculate a placement sequence. A standalone browser implementation lives in `WebApp/index.html`; the Unity implementation is in `Assets/Scripts/`.

[Open the existing browser app](https://grloper.github.io/Block-Blast-Solver/)

## Verified browser workflow

The audit opened the actual browser app, painted a cell, selected the 1×1, 1×2 and 1×3 pieces, and solved the hand. The observed result was three placements, no line clears, score 6. Desktop and 390px mobile layout were inspected. This smoke result does not establish exhaustive solver correctness or a universal speed claim.

The reviewed update adds keyboard board editing: Tab enters the board, arrow keys move between cells, and Space/Enter toggles a blocked cell. Pointer/touch painting remains available. Cells expose row/column/state labels and status changes are announced.

## Implementation

The browser `solve` function enumerates piece orderings and board positions, applies simultaneous row/column clears, and compares placement count and score. This is a search over the supplied hand, not a strategy that guarantees optimal future gameplay. No board data is uploaded by this self-contained page.

## Run locally

```sh
python -m http.server 8798 --bind 127.0.0.1
```

Open `http://127.0.0.1:8798/WebApp/`. Native source targets the Unity version recorded in `ProjectSettings/ProjectVersion.txt`; native builds and actual Unity gameplay were not executed in this Windows audit. Release/download availability, iOS distribution, browser compatibility beyond the tested Chromium session and C#/JavaScript parity remain unverified.

The live GitHub Pages app may lag the reviewed branch until the owner merges it.
