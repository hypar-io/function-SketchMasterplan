// This code was generated by Hypar.
// Edits to this code will be overwritten the next time you run 'hypar init'.
// DO NOT EDIT THIS FILE.

using Elements;
using Elements.GeoJSON;
using Elements.Geometry;
using Hypar.Functions;
using Hypar.Functions.Execution;
using Hypar.Functions.Execution.AWS;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace SketchMasterplan
{
    public class SketchMasterplanOutputs: ResultsBase
    {
		/// <summary>
		/// The total project area
		/// </summary>
		[JsonProperty("Total Project Area")]
		public double TotalProjectArea {get; set;}



        /// <summary>
        /// Construct a SketchMasterplanOutputs with default inputs.
        /// This should be used for testing only.
        /// </summary>
        public SketchMasterplanOutputs() : base()
        {

        }


        /// <summary>
        /// Construct a SketchMasterplanOutputs specifying all inputs.
        /// </summary>
        /// <returns></returns>
        [JsonConstructor]
        public SketchMasterplanOutputs(double totalProjectArea): base()
        {
			this.TotalProjectArea = totalProjectArea;

		}

		public override string ToString()
		{
			var json = JsonConvert.SerializeObject(this);
			return json;
		}
	}
}