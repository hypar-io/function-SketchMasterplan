using Elements;
using System.Collections.Generic;
using System;
using System.Linq;

namespace SketchMasterplan
{
	/// <summary>
	/// Override metadata for VoidGeometryOverrideAddition
	/// </summary>
	public partial class VoidGeometryOverrideAddition : IOverride
	{
        public static string Name = "Void Geometry Addition";
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