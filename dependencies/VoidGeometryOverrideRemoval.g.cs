using Elements;
using System.Collections.Generic;
using System;
using System.Linq;

namespace SketchMasterplan
{
	/// <summary>
	/// Override metadata for VoidGeometryOverrideRemoval
	/// </summary>
	public partial class VoidGeometryOverrideRemoval : IOverride
	{
        public static string Name = "Void Geometry Removal";
        public static string Dependency = null;
        public static string Context = "[*discriminator=Elements.FootprintVoid]";
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