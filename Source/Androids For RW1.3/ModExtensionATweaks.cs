using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace ATReforged
{
    /// <summary>
    /// Marks the ThingDef for being tweaked on initialisation.
    /// </summary>
    public class MechCorpseTweaker : DefModExtension
    {
        /// <summary>
        /// Tweaks the corpse by removing its rotting ability.
        /// </summary>
        public bool tweakCorpseRot = true;

        // TODO: Eliminate corpse butchering tweaking because why do we even want that

        /// <summary>
        /// RecipeDef to get butcher products from.
        /// </summary>
        public RecipeDef recipeDef;

        /// <summary>
        /// Alters the butchering products of the corpse by importing the recipe costs from a recipe.
        /// </summary>
        public bool tweakCorpseButcherProducts = true;

        /// <summary>
        /// How much of the recipe cost you get back from butchering.
        /// </summary>
        public float corpseButcherProductsRatio = 0.5f;

        /// <summary>
        /// If true the products will round up so you can net yourself at least one AI Core for example.
        /// </summary>
        public bool corpseButcherRoundUp = false;
    }
}
