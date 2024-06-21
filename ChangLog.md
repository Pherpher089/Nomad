### ctutor - 0.2.13b
- Fixing Fire Head Boss

### ctutor - 0.2.13a
- Updating armor to use synty character armor

### ctutor - 0.2.12b
- added correct button prompts to the rune info

### ctutor - 0.2.12a
- adding spread sheet support for info runes

### ctutor - 0.2.11b
- Fixing apples deleting items
- Added drop item sound indication
- Fixed color of damage text when hitting an item
- Stopped items that can not be destroyed from showing damage text
- Stopped players from being stuck in room password screen
- Clear out input fields when player leaves a menu

### ctutor - 0.2.11a
- Added player naming to show who is jointing roms
- Added password protected servers
- Added kick player functionality
- Added Create/Delete Worlds
- Can select which world you would like to play


### ctutor - 0.2.10b
- fixing issues with inventory
- fixing issues weth earth bulwark


### ctutor - 0.2.10a
- Added hot keys
- Upgraded inventory controls
- Fixed network bugs with new magic weapons

### ctutor - 0.2.9e
- added ice mantle
- added earth bulwark
- added fire and frozen status effect

### ctutor - 0.2.9d
- Fixing enemy aggro
- Fixing enemy animations not syncing
- Fixed enemy hit not syncing
- Enemy attacks are harder to interrupt with hits
- Added a little knock back
- Fixed spelling of loading in loading screen
- stopped loot from landing on enemy and getting stuck in the air
- lowered the spawn frequency in battle arena
  
### ctutor - 0.2.9a
- Added Realmwalker Desk
- Added Provision Desk
- Players can not craft RW Desk in the wilds
- Changed Wood Armor over to leather Armor
- Added enemy loot drop ranges
- Added Iron Node
- Added Earth Crystal Node
- Added Prismatic cotton Node
- Added Water Tesseract Node

### ctutor - 0.2.8a
- Added Cook Station
- Added in recipe for large meat, apple pie and glazed feast
- Added hand crafting recipe for spice
- added health value to food items. Previously it would increase both health and hunger from the same food value.


### ctutor - 0.2.7b
- Fix for level loading issue
- Fix for beast chest random item loss issue

### ctutor - 0.2.7a
- Adding camping tent
- Added a few icons
- Added tent bag and tent bag crafting recipe
- Added leather
- Wolf now drops leather
- Attempted another fix for portal issues.
- Added apothecary station
- Added crafting recipe for the health potion
- Added crafting recipe for the apothecary tent
  
### ctutor - 0.2.6a
- Adding new Screen Space info prompt - This has reset all the text in the current info prompts
- Cleaned up Item, SourceObject, and Health Manager scripts in the inspector, hiding unneeded properties
- Fixed the issue with the beast stable when building.
- Left an opening in the wilds to test Gerry's new structures.

### ctutor - 0.2.5c
- Adding title and version number
  
### ctutor - 0.2.5b
- Fixing level changing

### ctutor - 0.2.5a
- Adding version checker
- 
### ctutor - 0.2.4a
- Added wolf enemy

### ctutor - 0.2.3a
- Added stamina circle to beast
- Riding beast now costs stamina
- Feed the beast apples to regain his stamina
- Beast can not ram without the gear
- Added colored circles to the players

### ctutor - 0.2.2c
- Added Lava to boss area
- Added mana geysers to wilds
- Added mana item
- Wands need manna to shoot
- Added one mana to fire wand recipe


### ctutor - 0.2.2c
- Major movement sync improvements
- Fixing Battle arena based AI issue

### ctutor - 0.2.2b
- Added Moose model and animations to the beast
- Changed beast gear to be its own thing rather than a type of Item. This prevents bugs when the player attempts to grab near the beast gear.
- Players can use the ram action wile riding the beast.
- Players can open inventory if riding beast but not driving
- Added press and hold movement to build controls
- Fixed dead zone issue for all inventories
- Beast can not be mounted when laying down
- Beast can not be forced to lay down when he has riders
- Added enemy models in 
- Flipped the old boss level into a battle arena
- Added new models to main menu scene
- Fixed fireballs destroying stuff
  
### ctutor - 0.2.2a
- Added beast riding
- Added animal controller pack
  
### ctutor - 0.2.1f
- Fixing boss not shooting
- Fixing scene loading I hope
- Fixing issue with rotation while attacking
- Fixing input issue in inventory for XBox One controllers

### ctutor - 0.2.1e
- fixing raid issue
  
### ctutor - 0.2.1d
- fixing double equip issue
- fixing portal issue hopefully
- added some more enemies to wilds
- Also sopped boss from preventing enemy spawns
- Adding aim to projectile weapons
- Adding aiming line to projectile weapons
- Fixed bow enemy after bow changes - Enemies and Characters now have two different animation controllers(only bow dude currently)
- Added functionality that changes ai to inactive when far enough away from players
- Moved fire flowers so players have to travel further into the wilds
- Added ogre enemy
- Fixed some character animation issues
- Added an optimization for both transparency and AI only working if within range of a player
  
### ctutor - 0.2.1c
- Fixing portals issue where multiple people portal at the same time
- fixing boss pillars
- Fixing items not syncing between scenes
- Fixed fire ball wand issue
- fixed rock building pieces causing nav mesh slowdown
- Will now drop crafted item if inventory is full
- Lots of fixes in inventory
- adding trail render to arrow

### ctutor - 0.2.1b
- fixing beast stable, beast crafting and beast equipment
- Fixing raid issues.
- Raid now has win and loose state
- All enemies die if the raid is completed successfully
- Added raid countdown timer
- 
### ctutor - 0.2.1a
- Added character for player
- Increased plant fiber size
- Added ProgressionTracker.cs to manage updating game progress data. This is a solution for the issue that was spawning us in the tutorial world even though we passed it.

### ctutor - 0.1.19e
- Shrinking is grounded collider to prevent players from scaling walls
- Transparency will only be calculated for players that belong to a givin client. Not other players on other clients.

### ctutor - 0.1.19d
- Fixing portal issues

### ctutor - 0.1.19c
- Setting all controller ui to active when the scene loads for idealization. Was throwing an error.

### ctutor - 0.1.19b
- Fixing torch collider not to mess with the player
- Fixed issue with info rune 3d mesh: set read/write to true. This prevents errors.
  
### ctutor - 0.1.19a
- Consolidated some of the structures in wilds
- Replaced walls in wilds
- Added new navigation to all three game scenes

### ctutor - 0.1.18i
- Fixed camera issue
- Apples should not fall through the terrain
- Increased the collider size on a few items
- Lowered info rune in tutorial world
- Brightened night time
- Other players should not see an extra build piece when building
- Character does not attack when placing a build piece

### ctutor - 0.1.18h
# 3/01/24
- Fixed quit game button on main menu
- resized some menus to prevent clipping
- Disabled start game button when pressed to prevent bug
- Hit counters now render on top layer when behind other objects
- Fixed issue with not being able to unpause without moving in the pause menu first
- Fixed tutorial water glowing
- Implemented a new type of ground checking which will prevent the double jump
- Fixed crafting bench inventory side not cleaning up after use

### ctutor - 0.1.18g
# 2/29/24
- Fixing bugs related to spawning
- Fixing jump hit bug
- Adding weaker enemy for tutorial with slower frequency

### ctutor - 0.1.18f
# 2/28/24
- Fixing bugs
- adding water back in that I deleted

### ctutor - 0.1.18e
# 2/27/24
- Fixing more progress based spawning issues
- Adding in crafting pages
- Adding in page flipping functionality
- Fixing Compass Icons
  - Using a package from the asset store - Way better 
- Fixed boss health UI not appearing

### ctutor - 0.1.18d
# 2/22/24
- Fixed sprinting out of crouch weather there is head room or not
- Info rune can be closed with (esc)
- Like the new main menu music?

### ctutor - 0.1.18c
# 2/21/24
- Fixing further spawn issues
- Changing up character name to "Realmwalker"
- Fixing Wood armor material and icons to be wood color
- Adding interaction prompts to a few items(chests, work bench, beast chest)

### ctutor - 0.1.18a
# 2/12/24
- Added draft hub world
- Added player and beast spawners
- Added draft tutorial section
- Added internal portals for teleporting to another part of the same level\
- Added Info Prompt
- Added outlines to players and enemies when hidden

### ctutor - 0.1.17c
# 2/12/24
- Added Player Spawn point
- Added ability to set breakables to only be broken by specified tools
- Added bolder barrier that only the beast can break
- Added floating effect to info rune
  
### ctutor - 0.1.17b
# 2/12/24
- Looked at and fixed boss fight... I guess... Its working now
- Added player spawn mechanic where players spawn near the portal that returns them back to the level they came from.
  - example: If you take the portal from the hub world to the wilds, you will spawn in the wilds near the hub world portal. 
  - This helps with testing parts of the level like the boss in the wilds

### ctutor - 0.1.16d
# 2/10/24
- Players can no longer pick up items if they have no room
- Arrows no longer slam players out of this world
- Fixed some issues with main menu scaling
- fixing parts of beast chests not working

### ctutor - 0.1.16c
# 2/07/24
- Set portal so that it can not be damaged by players
- set added second larger box collider to arrow to prevent them from falling through the level
- Arrows and fireballs come from the characters center making them far more accurate

### ctutor - 0.1.16b
# 2/07/24
- Updated Raid mechanics and enemy raid behavior
  - 1/3 enemies target players on raid
  - Raids are triggered when the portal is fully built
  - Raids end after 2 mins, the portal is completely destroyed, or all the players die.
- Added First crafting page to pause menu
- Added loading screen.

### ctutor - 0.1.16a
# 2/03/24
- added night raids by enemies in the hub world
- Enemies now explode after death
- Portal loses pieces when It looses health
- Players should not be able to pick up items if they do not have room

### ctutor - 0.1.15a
# 1/28/24
- added info object
- added terrain and structures to wilds

### ctutor - 0.1.14b
# 1/19/24
- Fixed Boss Fight
- Fixed bug with beast chests
- Fixed issues with menus not working when controls UI was turned off

### ctutor - 0.1.14a
# 1/15/24
- Added beast chests
- Screen compass icons now disappear when in frame
- F6 now revives your player
- F7 now resets the time to morning
- Fixed issues with building pieces attaching to body
- Can now go into stable with beast - enemies can walk right through it though... will fix in future

### ctutor - 0.1.13b
# 1/12/24
- Fixed boss arena to support new navmesh
- Fixed boss not dropping portal piece
- Leaving boss kinematic as some of his ai will not work without it currently. This does need to be fixed but, because he does not walk up raps currently, it will not be an issue. 
- Adjusting pivots on some items
### ghay - 0.1.13a
# 1/14/24
- Updated stone Sword asset
- Updated stone sword blade asset
- updated stone axe asset
- updated stone axe blade asset
- updated stone pickaxe asset
- updated stone pickaxe blade asset
- updated rope asset
- updated stick asset
- updated basic arrow asset
- updated basic bow asset
### ctutor - 0.1.12b
# 1/11/24
- fixing two of the enemies who were not kinematic
- removed build restricted to colliding with a build piece or the world terrain
- Added in controls guide UI for main screen. Swaps between xbox and keyboard keys
- Added two new settings to start menu. Also set settings up to be persistent.
- Fixed issue with respawning team
- Fixed issue with players not being able to get into saddle station

### ctutor - 0.1.12a
# 1/7/24
- adding lanterns to the beast - only on at night

### ctutor - 0.1.11c
# 1/1/24
- Converted crafting benches to scriptable object recipes
- If a players inventory is full and they leave the crafting bench, items that do not fit in the crafting bench are spawned into the world
- Converted hand crafting to scriptable objects
- Removed AStart Pathfinding project and replaced it with unities built in navmesh

### ctutor - 0.1.11b
# 12/31/23
- Shortened day night cycle
- fixed icons
- added fire flowers to wilds

### ctutor - 0.1.11a
# 12/30/23
- Added Main Portal
  - Can add portal fragments to the portal
  - portal lights up when complete - thats all for the moment
- Hub world is the only world that saves. Wilds are reset when leaving.
- Portals now reset in the morning when returning to the hub world
- Players are returned to the hub world when the whole party dies
- added bow enemy to spawner
- adjusted enemy loot drops
  - EnemyManager component has array of Items that the enemy will drop when dead
  - enemies sill drop their weapon
- Added CompassIcon prefab for tracking objects off screen
  
### ctutor - 0.1.10b
# 12/28/23
- Bumped version
- fixed bug with saddle station
- fixed issue picking up arrow and everything breaking
- Added Title and company logo to splash screens
- 
### ctutor - 0.1.10a
# 12/27/23
- Added beast stable
  - Can craft and equipped beast with gear
- added ram gear for the beast

### ctutor - 0.1.9c
# 12/12/23
- Fixed issue with not being able to hit enemies
- fixed some behaviors with fire boss
- Added new burst attack where there will be 5 attacks and then a pause
  
### ctutor - 0.1.9b
# 12/10/23
- Added Fire Head Boss and his level
- Added bow enemy (not in game yet)
- Fixed issue with everyone getting the same equipment on load
- Other minor issues with items
- 
### ghay - 0.1.9a
# 12/03/23

- updated sword mesh
### Ctutor - 0.1.8b

# 12/02/23
- players have different colors
  
### ctutor - 0.1.8a
# 12/02/23
- Added Armor
  - Hemp and Wood armor(place holders)
  - Crafted at crafting bench
  - Expanded inventory ui to have armor slots
- Added hit number popup
- Core stats(Strength, Dexterity, Intelligence, constitution) are now equal to players level and thus increases in health, defense, attack should be visible.
  - This will need more intuitive solution, this is just for now
  - Many other bug fixes in inventory and around items

### ctutor - 0.1.7a
# 11/25/23
- Should have fixed arrows
- Added Fire Flower resource
- Added spell circle and spell circle crafting recipe
  - Created new system for creating crafting recipes with scriptable objects rather than hard coding them.
  - This also prevents an issue were the ItemManager does not need to be rearranged when creating a craftable that is buildable (e.x. fire pit, chest, crafting bench, torch, spell circle)
- Adjusted nighttime to have more player viability
- Added light intensity to torches and campfires

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