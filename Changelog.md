#	ATR 1.0 Changelog

##	New features:

###	General:
- ATR will no longer eat your TPS. This was done in a number of ways that can be summarised as: "Code optimisations"
- ATR is no longer guaranteed to break RimThreaded or Multiplayer with desyncs (This has not been tested, so proceed with caution!)
- Drones: (T1, M7 and M8 by default) They replace the "Simple Minded" trait and cannot connect to the SkyMind network as their cores are not powerful enough to contain a full consciousness. They are incapable of learning, very slow in their work, but come with level 8 in every skill from the start, don't care about recreation, mood and all those pesky organic things, but in turn, T1 are incredibly inefficient in their power usage.
- All humanlike races (except T5) can be manually added to either be drones, androids or neither in the settings. Just do yourself a favour and leave humans where they are, or you'll become eligible for a free ride to the title screen.
- The MUFF unit has been replaced with the TORT (Treaded Object for Retrieval and Transport). It is functionally identical but based on a turtle, therefore better.
- Androids can now be painted in whatever colour you like through an operation. (As long as RW supports that colour.)
- Androids can now use the new bedside charger to allow them to charge in normal beds. Charging pods and the wireless charging station still exist to offer a slightly cheaper alternative.
- The new organic charger implant replaces the stomach and does exactly what its name implies. (Allows organics to charge instead of requiring food.)
- Charging now takes priority over food whenever possible. If you don't want a charge-capable pawn to consume spicy lightning for whatever reason, disallow any chargers through their zone restrictions.
- Android-only medicine has been renamed to repair stims.
- Structure Gel has been replaced with the Stasis Pill that decreases power consumption for caravans instead of granting more carrying capacity.
- Fractal pills can now be acquired without the need for traders, by hunting fractal abominations in a new event.
- Fractal pills are also no longer guaranteed to turn your organic pawns into fractal abominations when consumed and might actually be useful... - Use at your own risk!
- Maintenance: All mechanical units added by ATR require maintenance to balance their lack of need for sleep and food. Humanlikes (and drones) can be told to keep their level of maintenance at a manually set stage while units with animal-like intelligence automatically maintain themselves in a working state. Maintenance is automatically done when idle or as a replacement for meditation. The higher the level of maintenance, the faster it falls and once it reaches critical levels, it can make parts fail which requires them to be replaced, so don't neglect setting aside some time for maintenance!

###	Network:
- The VX chip tiers are gone and replaced by the SkyMind Transceiver (VX3) and Receiver (VX0).
- The RX chip and relay antenna have been removed. Surrogates now work without penalty on other maps or in caravans.
- Hosts now share their skills and relationships with their surrogates - and they'll no longer be incapacitated while remotely controlling them!
- By default, you can now connect up to 8 Surrogates to each host without incurring a penalty to learning speed and consciousness. (See pinned guides for more details)

###	Servers:
- There is now only one server for each tier of point generation that can be toggled to fulfil one of these roles instead of needing one for each.
- Servers now need to be connected to the network in order to function, so make sure to build enough antennas!
- High-tech research benches without a research project set, can be used to manually produce skill, security or hacking points while connected to the network.
- Skill: The point allocation interface has been overhauled and now works with any mod that adds new skills or increases the skill cap! You can even grant or increase passions by supercharging a pawn's skill with points. Point conversion now depends on learning speed and aptitude for that particular skill.
- Security: Enemies can now hack you in a number of ways, including but not limited to: Draining your skill points, turning a random appliance on or off, forcing an enemy raid to appear, or damaging your relations with allies and more! (Can be disabled entirely in settings if you don't want to have to deal with it.)
- Hacking: The hacking system has been overhauled. You no longer rely on luck to have your hack succeed and instead need to use your hacking points to overcome your enemies' security points. The same applies to you though, so make sure you have all your security servers running at full capacity!

###	 Balance:
- The mechanical animals have been rebalanced to be less OP. (Boo!)
- Their output largely stayed the same, but their input has been increased to compensate. (Yay!)

##	Removed features:
- Androids can no longer charge via LWPN from Power++. The integration was very poorly implemented and was removed to decrease complexity and lag.
- T5 can no longer interact with the SkyMind network to fix an exploit that made it possible to swap a max skill T1 with a kind personality into a murder machine's body to avoid the T5 going on said murderous rampage.
- T5 are no longer guaranteed to be intolerant (They never were, but now they never are) towards organic life. They still hate you personally though.
- All android scenarios except for the basic one have been removed
- M7 and M8 can no longer be crafted and are obtainable as quest rewards only. They will get their own rework someday, so don't be too sad about that.
- You can no longer re-roll androids' stats through surgery. Remove and replace their core instead and live with the consequences of taking a(n) (artificial) life.
- Duplication of minds now requires the SkyCloud Core building. Swapping and Surrogate control still work as normal.
- Mechanite banks have gone the way of the Dodo. See the section about maintenance.

##	Integrated submods:
- Gynoids
- Android resurrection kit
- Skymind Retexture
- Swarm IED
- AT enchantment
- SM7 overhaul
- Shiny Androids 

- The TX (Terminator Expansion) submod is currently incompatible. It's on their owner to make them compatible again.

##	Known Issues:
- "Smart Medicine" and "Common Sense" need to be loaded at the end of your modlist or it will cause issues. That's a general rule, but ATR really depends on you fixing your modlists.
