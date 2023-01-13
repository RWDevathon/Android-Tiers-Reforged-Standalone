#	ATR 1.0 Changelog

## Medical
- Mechanical implants, prosthetics, and Hediffs have been completely reworked across the board.
- Mechanical units (using ATR's base features) now have separate overheating, freezing and bleeding hediffs.
- Mechanical units no longer randomly suffer part breakdowns (see Maintenance) like organics suffer age-related diseases.
- Mechanical units are, for now, completely unaffected by solar flares.
- Mechanical units now have Repair Stims as their Medicine equivalent, with all related mechanics tied to crafting skill and the Mechanic Work Type instead of Doctoring and Medicine skill.
- Replacement parts (besides makeshift arms/legs) have been made obsolete via a generic part pack that may repair and replace any damaged or destroyed part (and child parts) for mechanical units.
- Frameworks have received a major rework and are now more specialized, balanced (to each other) and consistent. Only one framework may be applied to a mechanical unit at a time - and are not recovered if removed.
- Brain Implants for mechanical units have been altered to be called Core Assistants, and are now much more fleshed out and unique. Only one Core Assistant may be installed in a unit at a time.
- An Organic Charger bionic (replaces the stomach) allows organics to benefit from charge-capability.

## Charging
- Mechanical units (besides noted cases in Animals) may charge to replenish energy or may eat food.
- Charge-capable pawns will always attempt to charge before eating food.
- Charging stations and charging beds now have very small power draws, but increase power draw by any occupants' body size when in use.
- Any bed (possibly modded ones automatically) can be made charge-capable by using a bedside charger placed facing the head of the bed.
- Charge-capable pawns will generally try to charge when seeking to fulfill rest or health needs.
    - Forcing a charge-capable pawn to rest for health reasons via command will NOT set them to charge.

## Drugs:
- Mechanical units may at the current time use any organic drugs.
- Android drugs' descriptions detail use cases for androids and effects for organics (usually negative).
- Stats, effects, durations, and various other features of drugs have been altered.
- A new stasis pill drug has been introduced that reduces hunger rate by 75% at the cost of a 25% global work speed penalty with a ~3 day duration designed for caravanning or non-worker use cases.
- Fractal pill effects have been completely reworked:
    - Fractal effect in mechanical pawns reduced to 4 linear stages, with greatly reduced overall effect.
    - Fractal effect in organics is significantly changed:
        - Upon contraction of the effect (via ingesting... or other means), effect begins in stage 4.
        - Roughly every 3 days, the effect warps, with an 80% to increase instead of decreasing.
        - Each stage has different effects, with stages < 4 toward mental and > 4 toward physical.
        - Stage 7 is lethal... depending on your point of view.
        - Stage 1 will be lethal... also depending on your point of view... but is for now a stage at which warping will freeze and remain permanent at that level.

## Drones:
- Drones (By default, T1's and the M7) have set skill levels and can not learn or lose skills or passions. Set skill level depends on the pawn's race, but defaults to 8.
- Drones can not use the SkyMind, and do not possess Cores like androids do - they can not be blanks, and start pre-initialized.
- Drones are completely thoughtless, and have no related needs, and no ideology.
- Drones can neither start nor receive social interactions, making them ineligible for various relationship statuses.
    - Drones can not be recruited, converted, or otherwise interacted with as prisoners. The tab is blank and will not allow access. 
    - Use the drone reprogramming surgery (unlocked by T1 android research) to "recruit" them, or handle various other details.
- Drones will not incur usual penalties for death or loss.

## Animals:
- The MUFF unit has been replaced by the Treaded Object for Retrieval and Transport (TORT). It serves the same role, but has had its energy consumption greatly reduced to increase usabililty. It is charge-capable.
- The Mineral Unit (Sheep-analogue) no longer sheds steel wool (which has been removed) and now sheds steel. Its hunger rate has been increased, and can not charge.
- The Nutrition Unit (Cow-analogue) has had output quantity and hunger rate greatly reduced. The Nutrition unit can not charge, nor consume processed foods (to avoid nutrition exploits).
- The Chemical Production Unit (Chicken-analogue) has had output quantity and hunger rate greatly reduced. Its output product is still convertible into Chemfuel or Neutroamine. It can not charge.
- The Watchdog Unit has had minor changes to its utility and stats to increase utility. It is charge-capable.
- All mechanical animals have the maintenance need, but with very high base efficiencies.

## Network:
- VX chips have been removed and replaced by a different system:
    - Androids have an Autonomous Core that allows SkyMind network access while acting as the consciousness of the pawn. 
    - Organics can have a Transceiver Implant that allows SkyMind network access.
- SkyMind Network access can now be blocked by certain effects like Dementia or Corrupted Memory.
- Duplication of a pawn's consciousness can now only be achieved via the use of a SkyMind Core.
- Transfers between pawns can, depending on the operation type, result in the "death" of the body left behind.
- Androids with newly installed autonomous cores may have a SkyMind connected pawn be installed rather than initializing a new pawn.
    - This is the only method for transfering (minus permutation) for transfering consciousness into an android.
- Organics may have consciousnesses downloaded into them if a surrogate (see Surrogates).

## Surrogates:
- Surrogates are now tethered much more closely to their controller, sharing skills, traits, and ideology. XP gain or changes to one will affect the controller and all other surrogates.
- Controllers are no longer downed when controlling surrogates.
- Controllers may now control as many surrogates as conceivable within a game of RimWorld.
    - Controllers not in the SkyMind-Core will suffer an increasing penalty to consciousness and learning to themself and all surrogates if exceeding the limit (default 1) in the mod settings to represent the increasingly taxing effort required to manage so many bodies.
- Surrogates of foreign factions will now disconnect and die (dead-man's-hand) if downed or incapacitated.
- Disconnected surrogates are factionless blanks and use considerably less power than normal.

## Servers:
- There are now only 3 server buildings (1 for point storage, 1 for generation, 1 high tier one for both) that can be toggled to switch between skill, security, and hacking roles.
- Servers now need to be connected to the SkyMind network in order to function, and will otherwise provide no utility.
- Server power consumption, cost, and heat generation has been altered.
- Powered research benches can connect to the SkyMind Network to manually produce skill, security or hacking points based on a pawn's intellectual skill. This task always takes lower priority than doing research.
- Skill: The point allocation interface has been overhauled to add XP or passions to skills rather than giving levels at a variable cost depending on a pawn's learning speed in that particular skill. Pawns must be connected to the Network to use the points.
- Security: Security is no longer an arbitrary server-to-consumer threshold, and enemy hacks are now fully fledged incidents seeking to harm you and your network in various ways ranging from DDOS and Troll attacks to counter-hacks, raid makers, and diplomatic sabotages.
- Hacking: Hacking is now an investment game rather than a completely RNG-based system. Launchable via any server toggled to hack, the hack interface allows choosing a target hack type and target while allowing you to dedicate a certain amount of points that will affect the likelihood of success. Repeatedly hacking increases costs temporarily, to discourage continuous, unending operations.

## Maintenance:
- All mechanical units now have a maintenance need.
- The need is displayed as a small interface visible on individual pawns.
- Most mechanics are visible via tooltip on the interface and act reasonably similarly to Royalty's Psyfocus.
- Mechanical units will receive small penalties for poor maintenance and a small benefit for especially high maintenance.
- Poor maintenance for an extended length of time (usually 3+ days) will result in various part breakdowns occurring.
    - Minor issues like temporary part blackouts, rust development, and part decay occur frequently after this time.
    - Critical issues like corrupt memory, coolant pump failures, and rogue mechanites can occur if in critical maintenance.
- Maintenance related issues are normally only fixable via the use of a part pack (see Medical).
- Units will attempt to do urgent maintenance up to the target level prior to work (unless in the work time assignment) and prior to most other needs if in the meditation time assignment (Royalty required).
- Units will always attempt to do idle maintenance if they have no other tasks to attend to (would wander).
- Emergency maintenance with part packs can be used for disabled or injured units (up to 40%, 10% per pack) via surgery.

## Balance:
- Drones (by default, T1's) can not use the SkyMind Network, learn, have traits, or socially interact at all. They have no mood whatsoever, and are only recruitable via reprogramming surgery (unlocked via T1 android research).
- Androids now have standard humanlike needs (configurable via settings) and personalities.
- Mechanical animals are now much more closely aligned in their output to their vanilla counterparts while still maintaining unique roles.
- Most stats have been tweaked to more closely align with vanilla values.

## T5's:
- Still exist.
- Are now acquirable via a ridiculously rare quest rather than a ridiculously rare incident.
- Have no needs (besides possibly mod added ones).
- Can not use the SkyMind network.
- Still can not wear clothes.
- No longer have the intolerance trait, but probably still looks down upon lesser beings.
- May eventually be de-implemented during the course of making an Archotech sub-expansion.

## Miscellaneous
- Mechanical units are now paintable via surgery (and it has no gameplay impact) in a wider variety of colors.
- Performance and compatibility has been improved across the board, with a number of severe memory and performance flaws resolved.
- Mod settings have been reworked to allow easier locating of individual settings and editing of various variables and mechanics.
    - Gender of androids is now set at their spawn time via mod settings, with default being None.
- Androids now generally care about the same things humans do, and are treated with equal respect by default.
- Fractal abominations have been reworked, with several new mechanics and surprises.
- (Biotech) Genes and Xenotypes have been prevented from appearing on mechanical units.
- (Biotech) Various children and pregnancy mechanics are prevented from interacting with mechanical units.

## Removed features:
- Androids can no longer charge via LWPN from Power++. The integration was very poorly implemented and was removed to decrease complexity and lag.
- Drones and T5's can no longer make use of the SkyMind Network, meaning exploits around transferring minds for trait and bodies have been closed.
- Simple Minded and Mech traits have been removed.
- There is now only the basic Crashlanded-like android scenario
- The M7 is no longer craftable and is obtainable only via the orbital target. M8's have been removed entirely.
- Rerolling android stats and traits, as well as reprogramming androids, has been removed.
- Duplication of minds now requires the SkyCloud Core building. Mind transfer and surrogate operations are unaffected.
- Removed the anti-material rifle.
- Removed steel wool - mineral units now shed steel itself.

## Integrated submods:
- Gynoids
- Android resurrection kit
- Skymind Retexture
- Swarm IED
- AT enchantment
- SM7 overhaul
	- The Mech Blade has been replaced with a pole-axe
- Shiny Androids 

- _The TX (Terminator Expansion) submod is currently incompatible. It's on their owner to make them compatible again._

## Known Issues:
- Mechlinks and Neuroformers are not usable on androids due to the fact that they are hardcoded to check against the Brain bodypart and also are tied to psychic sensitivity. This is not a bug. If you read the item descriptions, it is pretty clear they are designed explicitly and specifically for human physiology.
- Mechanical units will not respond to overheating or hypothermia correctly at the moment as RimWorld is hardcoded to check against the hediffs that androids no longer have.
- "Smart Medicine" and "Common Sense" need to be loaded at the end of your modlist or it will cause issues. That's a general rule, but ATR really depends on you fixing your modlists.
- Activating Self-Tend on a mechanical pawn will throw a message at you complaining that the Doctor work type is not assigned even if the pawn has the Mechanic work type assigned (It's harmless, mechanical pawns will self-tend based on Mechanic, not Doctor).
- Blood Rot and Paralytic Abasia do not function on androids at the moment. Work is planned to add mechanical variants for them.