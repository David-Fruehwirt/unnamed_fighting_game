# Responsive window layout

Adapt the viewport and HUD on every window resize, including while settings pause gameplay. Preserve world coordinates, collisions, character size, animations and artwork. Expand the visible area for different aspect ratios; retain nearest-neighbor integer scaling when the window fits 960x540, and use proportional downscaling for smaller windows to prevent clipping. Keep the title, damage, state, settings and controls inside the viewport. Position offscreen indicators using current viewport bounds. Validate landscape, ultrawide, portrait and small windows, including an open settings panel. Commit implementation and verification, then push and launch.

Godot reference: https://docs.godotengine.org/en/stable/tutorials/rendering/multiple_resolutions.html
