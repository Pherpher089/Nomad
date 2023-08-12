### Bug Fixes 0.0.8.c
# 8/11/23
- Fixed time saving and loading
- Fixed issue with apples not spending correctly
- Fixed issue with double hitting objects when online
- Fixed fire pit bugs


### Object Spawn/Pickup sync improvements 0.0.8.b
# 8/7/23
- Unified all of the objects rigidbody and collider configuration to ensure no variations during spawning in position or rotation. All spawned objects are now triggers. 
- Fixed issue with picking up logs and leaving axe in the air
- Added some error catching in the audio manager.
- Added spawn points for players were they left off
- Added time saving

### Added The beast 0.0.8.a
# 8/7/23
- Added dev keys for resetting save data
- Added temp crafting recipe for beast stick - stick + stick

### Added The beast 0.0.7.a
# 8/7/23
- Beast follows beast stick
- Beast auto spawns with players and is a photon object
- Removed rigidbody movement from AiMover.cs - This was to prevent the beast from needing a rigidbody which provided unwanted behavior

### Added Online Multiplayer 0.0.6.a
# 8/1/23
- Added online multiplayer and start screen

### Prepping Play Test Build:Fixes 0.0.5b
# 6/23/23
- Adjusted wood cut sound
- changed sounds to regular hit when not holding a weapon
- fixed bug where hands are also hitting when holding a weapon


### Prepping Play Test Build:Fixes 0.0.5
# 6/23/23
- Fixed issues with fire
- added sound effects for cutting trees and bolder

### Prepping Play Test Build:Fixes 0.0.4b
# 6/20/23
- added development build to the play test build

### Prepping Play Test Build:Fixes 0.0.4a
# 6/18/23
- Stopped logs from being picked up
- Added a quit functionality
- Ensured build pieces appear above the players feet
- 
### Prepping Play Test Build 0.0.4
# 6/18/23
- Added Controls UI
- Added toggle for controls ui and single player game pad
- Added text to status bars for players
- Added list of crafting recipes to pause menu
- fixed bugs with fire pit
- fixed rocks exploding when breaking boulders
- removed water plane
- fixing issues with enemy spawning

### Adding Stone building 0.0.3
# 6/15/23
- Added 3 stone building pieces
- Adjusted biome spawning to ensure that the player starts in the Evergreen Vale (working title)

### fixing-enemy-attack
#  1/18/23
- Fixing some item issues and bugs
- Adjusted enemy AI to trigger attack from enemeyManager rather than the tool
- Moved equipped item back to gameObject

### Brasaving-player-with-scriptable-object
#  1/18/23
- Reimported animations to restore their animation events
- Added ItemMaster to keep a single list of the games items in one place
- Moved GenerateLevel script to GameMaster obj and removed Level Master obj