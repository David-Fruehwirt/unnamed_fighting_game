# Powered second jump

Approved requirements: one fresh midair jump press; replenish only on landing or respawn. Preserve the first jump, coyote time, landing buffer, combat, character design, palette and body-part sizes. Second jump targets 15% more height and reach: impulse 499, directional speed 253, release clamp 182; first jump remains 465/235/170.

Append eight discrete poses (37–44) using unchanged draw definitions and fixed idle bone lengths. Give the powered jump a tighter tuck and raised guard. Four-frame blue/cyan flames attach beneath both boots and both backpack outlets, only during second-jump ascent. Stop at apex, ceiling, hit, landing or reset; track motion independently of punches. No third jump before ground contact.

Workflow: document and commit; inspect structured source; author integer-grid poses with hash-guarded Code as Pixel Art operations; validate/export/review and commit animation; author separate effects and attachment metadata; process actual Pixelloid at 1:1 before game import and commit; integrate C# and test; verify all 592 previous part frames unchanged; commit, push and launch. No image generation. No body-part redesign, so existing per-part sources remain unchanged.

Baseline: 1b43084783baa66e8e86246be9f95e1dda1c2600. Preserve unrelated local edits, including the local run timing override.
