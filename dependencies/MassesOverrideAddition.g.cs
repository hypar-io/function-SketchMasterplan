using Elements;
using System.Collections.Generic;
using System;
using System.Linq;

namespace SketchMasterplan
{
	/// <summary>
	/// Override metadata for MassesOverrideAddition
	/// </summary>
	public partial class MassesOverrideAddition : IOverride
	{
        public static string Name = "Masses Addition";
        public static string Dependency = null;
        public static string Context = "[*discriminator=Elements.Footprint]";
		public static string Paradigm = "Edit";

        /// <summary>
        /// Get the override name for this override.
        /// </summary>
        public string GetName() {
			return Name;
		}

		public object GetIdentity() {

			return Identity;
		}

	}

}