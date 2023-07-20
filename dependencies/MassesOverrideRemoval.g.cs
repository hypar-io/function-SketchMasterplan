using Elements;
using System.Collections.Generic;
using System;
using System.Linq;

namespace SketchMasterplan
{
	/// <summary>
	/// Override metadata for MassesOverrideRemoval
	/// </summary>
	public partial class MassesOverrideRemoval : IOverride
	{
        public static string Name = "Masses Removal";
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