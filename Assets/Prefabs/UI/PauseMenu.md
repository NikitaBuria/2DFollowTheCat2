# Pause menu art prefab

Place PauseMenu.prefab directly under your existing HUD Canvas (Scale With Screen Size). It stretches to cover the screen with a dimmer; the central panel is 440 x 550 reference units. Recommended reference resolution: 1280 x 720 or larger. Use the existing EventSystem and InputSystemUIInputModule; the prefab does not create duplicates.

Assign the instance to UIManager.pauseMenuPanel. UIManager already shows it for GameState.Paused. No scene or gameplay scripts were changed.

Panel contains Title, ResumeButton, SettingsButton and ExitButton. Labels are editable TextMeshPro text. Wire each Button.onClick yourself. Settings screen and exit behavior are not implemented by this visual prefab. When opening the menu, your UI code should select ResumeButton through the EventSystem for immediate keyboard/controller navigation; explicit Up/Down navigation then loops through the three buttons.

Resume must restore the appropriate game state through your GameStateManager. Keep input in PlayerInputReader and visibility in UIManager. Avoid adding polling or time-scale logic to the visual prefab.

Optimization: one shared button sprite; one static Cinzel font atlas/material for all labels; 1024px panel and 512px button import limits; no mipmaps or CPU texture copies; no extra Canvas, Animator, layout groups, per-frame scripts or image-based text. Dimmer and the three buttons are the only raycast targets. Button fades use Unity UI's unscaled transition timing.

Verified in an isolated Unity render at 1280 x 720, with label fit, button count, and explicit navigation checked. Gameplay actions have not been tested because they are intentionally unbound.
