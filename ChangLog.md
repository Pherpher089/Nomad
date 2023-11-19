### ctutor - 0.1.6b
# 11/18/23
- Arrows now sync up correctly and should not ruin using inventory
- Arrow now move through the grass
- One player may use a chest at any given time. This is to prevent the chests from getting out of sync.
- Fixed issue with items disappearing and not stacking when inventory is full.
- Items now land on any surface, not just the terrain - But will hover when item is destroyed
- Added hit points to building pieces
- Added portals to the GymWorld
- 
### ghernandez- 0.1.6a
# 11/18/23
- Added Spell Shrines
- Added Spell resources
- Added Spell Nodes
- Updated Gym Level
- Added new Gym materials

### ctutor - 0.1.5a
# 11/14/23
- Added storage chests
- Enemies drop what they are holding and arrows
- Fixed dev keys in wilds
- Day night cycle should work, though I am not sure what was stopping it before
- Night time parameters for monster spawning appears to work as well
- Fixed health not resetting on respawn
  
### ctutor - 0.1.4b
# 11/12/23
- Fixing arrows not hurting enemies
- Arrows now stop above the terrain when hitting the ground and can be picked up

### ctutor - 0.1.4a
# 11/11/23
- added dodge roll for left control and right button on the 360 controller
- Modified enemies to not take collision physics from player but player can still not walk through enemy. 
  - This was an issue when rolling into enemies. It knocked them back pretty hard
- Fixed respawning with no camp fire


### ctutor - 0.1.3a
# 11/7/23
- Added bows and arrows
- changed the hub world to have more resources and enemies for testing 
- Added pick axe head and pick axe crafting recipe and items
- adjusted the sword to be a bit bigger and actually useful
- New axe enemy :)


### ctutor - 0.1.2a
# 11/7/23
- Added crafting benches with one recipe for now
- added rigidbodies to the beast to remove y axis value matching player
  - can now ride on the beast a bit more
- replaced snap value to .5
- fixed nav mesh generation to match the size of the terrain
- fixed craffing benches not disappearing
  
### ctutor - 0.1.1.d
- Added more value to apples
- Tress drop more logs
- upped the axe damage
- Adjusted wilds terrain so that it is centered.
- Added fixes to the transparency 
  
### ctutor - 0.1.1.c
- More file save fixes

### ctutor - 0.1.1.b
- fixing level saving 
- files are gain saved onto joining clients machines :/
  
### ctutor - 0.1.1.a
# 10/31/23
- Added level traversal logic for online play
- Removed saving master client level sve files on non master client machines. Level save data is now kept in a variable during play if not master client
- Added portal prefab. Add the destination scene name in the PortalInteraction script component.

### ctutor - 0.1.0.a
# 10/31/23 
- Removed procedural level generation
- Updated menu flow/verbiage to match our new direction with settlement based play
- Hub world is now active and available for level design (with our 4 base assets and the building objects) - Just make sure objects in the level are children of the Terrain/Floor plane
- Objects removed are saved and persist on the next play (i.e. trees that are cut down do not appear in future play throughs)
- Modified level saves are transferring over to other online players when they join your 

### ctutor - 0.0.10.a
# 8/15/23
- beast camp mode by hitting the beast with a beast stick
- beast has cargo racks
- Specific Items can be packed up
- Packed items can be picked up and moved via `f`
- [x] Use `tab` to unpack item in build mode if it is a packable
- [x] Cargo Gear Inventory UI was added
- [x] Can add, move and remove items from beast cargo
- [ ] Can pack and unpack items on the beast cargo
- [ ] Beast cargo is persistent with the level
- [ ] Beast cargo syncs over the network

### ctutor - 0.0.9.a
# 8/15/23
- Added respawning players.
- Players can stoke the fire at a fire pit to revive fallen players
- If all players die, they are respawned at the last stoked fire pit
- Added dev key `F5` for killing all players on local client

### ctutor - 0.0.8.d
# 8/13/23
- Fixed time saving and loading
- Fixed issue with apples not spending correctly
- Fixed issue with double hitting objects when online
- Fixed fire pit bugs
- Adding Readme
- Adding dev key for max-health/max-hunger
  
### ctutor - 0.0.8.c
# 8/8/23
- synked up fire pits when stoked
- synked time of day and set that to save - buggy
- Spawning players at saved position not world origin
- spawn position is updated if players move too far from current spawn point
- new players drop in in current player location

### ctutor - 0.0.8.b
# 8/7/23
- Unified all of the objects rigidbody and collider configuration to ensure no variations during spawning in position or rotation. All spawned objects are now triggers. 
- Fixed issue with picking up logs and leaving axe in the air
- Added some error catching in the audio manager.
- Added spawn points for players were they left off
- Added time saving

### ctutor - 0.0.8.a
# 8/7/23
- Added dev keys for resetting save data
- Added temp crafting recipe for beast stick - stick + stick

### ctutor - 0.0.7.a
# 8/7/23
- Beast follows beast stick
- Beast auto spawns with players and is a photon object
- Removed rigidbody movement from AiMover.cs - This was to prevent the beast from needing a rigidbody which provided unwanted behavior

### ctutor - 0.0.6.a
# 8/1/23
- Added online multiplayer and start screen

### ctutor - 0.0.5b
# 6/23/23
- Adjusted wood cut sound
- changed sounds to regular hit when not holding a weapon
- fixed bug where hands are also hitting when holding a weapon


### ctutor - 0.0.5
# 6/23/23
- Fixed issues with fire
- added sound effects for cutting trees and bolder

### ctutor - 0.0.4b
# 6/20/23
- added development build to the play test build

### ctutor - 0.0.4a
# 6/18/23
- Stopped logs from being picked up
- Added a quit functionality
- Ensured build pieces appear above the players feet
- 
### ctutor - 0.0.4
# 6/18/23
- Added Controls UI
- Added toggle for controls ui and single player game pad
- Added text to status bars for players
- Added list of crafting recipes to pause menu
- fixed bugs with fire pit
- fixed rocks exploding when breaking boulders
- removed water plane
- fixing issues with enemy spawning

### ctutor - 0.0.3
# 6/15/23
- Added 3 stone building pieces
- Adjusted biome spawning to ensure that the player starts in the Evergreen Vale (working title)

### ctutor -
#  1/18/23
- Fixing some item issues and bugs
- Adjusted enemy AI to trigger attack from enemeyManager rather than the tool
- Moved equipped item back to gameObject

### ctutor -
#  1/18/23
- Reimported animations to restore their animation events
- Added ItemMaster to keep a single list of the games items in one place
- Moved GenerateLevel script to GameMaster obj and removed Level Master obj