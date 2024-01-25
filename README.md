# Rogoto

This is the source code and all assets for [Rogoto](https://wardergrip.itch.io/rogoto) which was a group project for the subject "Group projects" at [Digital Arts and Entertainment](https://digitalartsandentertainment.be/).

## Credits 
### Programming
- Ruckebusch Bas
- Messely Reï (That's me!)
- Van de velde Jorrit
### Art
- Adriaens Naomi
- Blaase Mikkel
- Tiraferri Simone
### Game design
- Van de Velde Jorrit
### Music
- Keyaert Lea
### SFX
- Babinsky Patrick-Raul
- Derycke Raf

# My contribution

## Prototyping
- Movement
    - Camera
    - Character
- Game AI (using BehaviorTree)
- Enemy nests, enemy spawning, enemy navigation (using Navmesh)
- Shape based mining
- Different hotbars

## Gameplay
- Turret system
    - Designed overall code architecture
    - Turrets implemented:
        - Artillery turret
        - Booster turret
        - Basic turret
        - Laser turret
        - Toxic waste turret
- Effect implementations (end of round rewards)
- Variant implementations (modifiers for turrets)
- Hediff system (Hediff name is inspired by Rimworld, meaning health difference)
    - Handles modifiers (OnApply, OnTick, OnReapply, OnRemove)
        - Bleed
        - Slow
        - Tagging
        - ...
    - Access point for health, movementspeed, ...
- Camera controller
- Hotbar
- Damage number (pop ups)
- Implementing SFX (making audiopatches, mixing, hooking events)

## Internal tools
- Audiopatch (adapted and extended)
    - ScriptableObject based, event based, simple SFX system
    - Allows non programmers to implement & tweak SFX 
- Particlepatch
    - ScriptableObject based, event based, simple VFX system
    - Allows non programmers to implement & tweak VFX 
- Effect & blueprint tester
- Organisation & maintaining "Trivial" scripts
    - Simple, decoupled, reusable scripts
    - Examples:
        - LookAtCamera
        - `SortByDistance(this List<T> list, Vector3 point)`
        - DestroyTimer
        - EventProxy
        - DebugProxy
        - ...

## Misc.
- Info menu's (hover over hotbar slot, hover over turret in world)
- Main menu
- How to play menu (art & implementation)
- Tooltip (hover over UI gives info)
- Objective flavour text ("Build a path to the next cave")
- Objective tooltip (More indepth version of flavour text)
- Playtesting (for balancing and bug hunting)
- Enemy path preview (logic)
- Bug fixes

## Scrapped features
- Trap system
- Minimap
