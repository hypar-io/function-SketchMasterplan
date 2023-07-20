using System;
using System.Collections.Generic;
using System.Linq;
using Elements;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements
{
    public class FootprintVoid : GeometricElement
    {
        public Polygon Boundary { get; set; }

        [JsonIgnore]
        public Polygon BoundaryProjected => Boundary.TransformedPolygon(new Transform().Scaled((1, 1, 0)));
        public double Height { get; set; }

        public double Area { get; set; }

        [JsonProperty("Starting Level")]

        public int StartingLevel { get; set; } = 1;

        [JsonProperty("Last Level")]
        public int LastLevel { get; set; }

        [JsonProperty("Insertion Point")]
        public Vector3 InsertionPoint { get; set; }
        public FootprintVoid(Vector3 insertionPoint, Polygon profile, List<double> levels, int startingLevel, int lastLevel, Material material = null) : base()
        {
            if (profile == null)
            {
                var rect = Polygon.Rectangle(2, 2);
                profile = rect.TransformedPolygon(new Transform(insertionPoint));
            }
            this.InsertionPoint = insertionPoint;
            var baseHeight = levels.LevelElevationAtLevelNumber(startingLevel);
            // lastLevel + 1 because we want the "bottom" of the next level, +/- 0.01 because we
            // want the CSG not to fail.
            var topHeight = levels.LevelElevationAtLevelNumber(lastLevel + 1);
            this.StartingLevel = startingLevel;
            this.LastLevel = lastLevel;
            var height = topHeight - baseHeight;
            this.Boundary = profile.TransformedPolygon(new Transform(0, 0, baseHeight));
            this.Height = height;
            this.Area = profile.Area();
            if (material == null)
            {
                material = BuiltInMaterials.Void;
            }
            this.Material = material;
            this.Representation = new Extrude(this.Boundary, height, Vector3.ZAxis, false);
        }

        public Extrude GetRepresentationAsVoid()
        {
            var extrude = this.Representation.SolidOperations.OfType<Extrude>().First();
            return new Extrude(extrude.Profile, extrude.Height, extrude.Direction, true);
        }
    }
}
