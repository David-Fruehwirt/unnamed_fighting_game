# Responsive window layout

Adapt the viewport and HUD on every window resize, including while settings pause gameplay. Preserve world coordinates, collisions, character size, animations and artwork. Expand the visible area for different aspect ratios; retain nearest-neighbor integer scaling when the window fits 960x540, and use proportional downscaling for smaller windows to prevent clipping. Keep the title, damage, state, settings and controls inside the viewport. Position offscreen indicators using current viewport bounds. Validate landscape, ultrawide, portrait and small windows, including an open settings panel. Commit implementation and verification, then push and launch.

Godot reference: https://docs.godotengine.org/en/stable/tutorials/rendering/multiple_resolutions.html

## Verification

- C# build: zero warnings/errors.
- Responsive rendered suite: 72 checks across 960x540, 1400x600, 800x800, 360x640, 640x360 and 1280x720; no failures. Resize tested with the settings menu paused/open. Portrait and wide captures inspected.
- Movement: 370 checks; training/combat/camera: 113 checks; no failures. Baseline tests explicitly select 960x540 because the headless display defaults to 64x64.
- All artwork and world collision coordinates are unchanged. Small windows use nearest-neighbor proportional downscaling; below native resolution some source-pixel detail is necessarily lost. Integer scaling remains active when the window can fit the native resolution.
- Source: `game/scripts/ResponsiveLayout.cs`; regression scene: `game/tests/responsive_smoke.tscn`.
