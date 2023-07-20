using Elements;
using Elements.Geometry;
using Elements.Geometry.Solids;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchMasterplan
{
    public static class SketchMasterplan
    {
        /// <summary>
        /// The SketchMasterplan function.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A SketchMasterplanOutputs instance containing computed results and the model with any new elements.</returns>
        public static SketchMasterplanOutputs Execute(Dictionary<string, Model> inputModels, SketchMasterplanInputs input)
        {
            /// Your code here.
            var model = new Model();
            var levels = input.TypicalLevels.Select(l => l.Height).ToList();

            if (levels.Count() == 0)
            {
                var errOutput = new SketchMasterplanOutputs();
                errOutput.Warnings.Add("You must specify at least one level.");
            }

            List<Footprint> footprints = new List<Footprint>();

            // DEPRECATED PATHWAY:
            if (input.Footprints != null)
            {
                foreach (var footprint in input.Footprints)
                {
                    var fp = new Footprint(footprint, levels);
                    fp.SetLevels(1, 1);
                    footprints.Add(fp);
                }
            }

            // NEW PATHWAY:
            if (input.Overrides?.Additions?.Masses != null)
            {
                foreach (var newMass in input.Overrides.Additions.Masses)
                {
                    var fp = new Footprint(newMass.Value.FootprintShape, levels);
                    fp.SetLevels(1, 1);
                    footprints.Add(fp);
                    Identity.AddOverrideIdentity(fp, newMass);
                }
            }

            if (input.Overrides?.Masses != null)
            {
                foreach (var massShapeOverride in input.Overrides.Masses)
                {
                    var matchingFootprint = footprints
                         .Select(f => ((Footprint footprint, double similarity)?)(f, PolygonSimilarity(f.Boundary, massShapeOverride.Identity.Boundary)))
                         .Where(f => f.Value.similarity < 5)
                         .OrderBy(f => f.Value.similarity)
                         .FirstOrDefault()?.footprint;
                    if (matchingFootprint != null)
                    {
                        matchingFootprint.FootprintShape = massShapeOverride.Value.FootprintShape;
                        Identity.AddOverrideIdentity(matchingFootprint, massShapeOverride);
                    }
                }
            }
            if (input.Overrides?.Massing != null)
            {

                foreach (var massingOverride in input.Overrides.Massing)
                {
                    var matchingFootprint = footprints
                        .Select(f => ((Footprint footprint, double similarity)?)(f, PolygonSimilarity(f.Boundary, massingOverride.Identity.Boundary)))
                        .Where(f => f.Value.similarity < 5)
                        .OrderBy(f => f.Value.similarity)
                        .FirstOrDefault()?.footprint;
                    if (matchingFootprint != null)
                    {
                        matchingFootprint.MassName = massingOverride.Value.MassName ?? matchingFootprint.MassName;
                        matchingFootprint.BuildingName = massingOverride.Value.BuildingName ?? matchingFootprint.BuildingName;
                        if (massingOverride.Value?.BuildingLevels != null && massingOverride.Value.BuildingLevels.Count() > 0)
                        {
                            matchingFootprint.BuildingLevels = massingOverride.Value?.BuildingLevels?.ToList();
                        }
                        if (massingOverride.Value.StartingLevel != 0)
                        {
                            matchingFootprint.StartingLevelWasSet = true;
                        }
                        matchingFootprint.SetLevels(
                            massingOverride.Value.StartingLevel == 0 ? matchingFootprint.StartingLevel : massingOverride.Value.StartingLevel,
                            massingOverride.Value.NumberOfLevels == 0 ? matchingFootprint.NumberOfLevels : massingOverride.Value.NumberOfLevels);
                        Identity.AddOverrideIdentity(matchingFootprint, "Massing", massingOverride.Id, massingOverride.Identity);
                        // This should really be using nullable ints, but these break the UI atm.
                    }
                }
            }
            var allFootprints = Profile.UnionAll(footprints.Select(f => new Profile(f.FootprintShape)));

            // auto-stack behavior
            for (int i = 1; i < footprints.Count; i++)
            {
                var highestLevel = 0;
                for (int j = 0; j < i; j++)
                {
                    if (footprints[j].FootprintShape.Contains(footprints[i].FootprintShape.Centroid()) && footprints[j].LastLevel > highestLevel)
                    {
                        highestLevel = footprints[j].LastLevel;
                    }
                }
                if (highestLevel != 0 && !footprints[i].StartingLevelWasSet)
                {
                    footprints[i].SetLevels(highestLevel + 1, footprints[i].NumberOfLevels);
                }
            }

            var voidLocations = new List<(Vector3 insertionPoint, VoidGeometryOverrideAddition VoidsOverride)>();
            if (input.Overrides?.Additions?.VoidGeometry != null)
            {
                foreach (var voidGeo in input.Overrides.Additions.VoidGeometry)
                {
                    voidLocations.Add((voidGeo.Value.Boundary.Centroid(), voidGeo));
                }
            }
            // deprecated pathway
            if (input.Voids != null)
            {
                foreach (var v in input.Voids)
                {
                    voidLocations.Add((v, null));
                }
            }
            var unionedFootprints = new List<Footprint>();
            foreach (var profile in allFootprints)
            {
                // assign names
                var fpsWithinProfile = footprints.Where(fp => profile.Contains(fp.FootprintShape.PointInternal()));
                var name = fpsWithinProfile.Select(fp => fp.BuildingName).FirstOrDefault(n => n != null);
                var levelHeights = fpsWithinProfile.Select(fp => fp.BuildingLevels).FirstOrDefault(n => n != null && n.Count() != 0);
                if (name != null)
                {
                    fpsWithinProfile.ToList().ForEach((fp) =>
                    {
                        fp.BuildingName = name;
                    });
                }
                // propagate level settings to whole building
                if (levelHeights != null && levelHeights.Count() > 0)
                {
                    fpsWithinProfile.ToList().ForEach((fp) =>
                    {
                        fp.BuildingLevels = levelHeights;
                        fp.UpdateLevelHeights();
                    });
                }

                var startingLevel = fpsWithinProfile.Select(fp => fp.StartingLevel).OrderBy(s => s).First();
                var endingLevel = fpsWithinProfile.Select(fp => fp.LastLevel).OrderBy(s => s).Last();
                // construct voids 
                var voidsInProfile = voidLocations.Where(v => profile.Contains(v.insertionPoint));
                foreach (var v in voidsInProfile)
                {
                    VoidsOverride matchingVoidSettings = null;
                    VoidGeometryOverride matchingVoidGeometry = null;
                    if (input.Overrides != null)
                    {
                        if (input.Overrides.Voids != null && input.Overrides.Voids.Count > 0)
                        {
                            matchingVoidSettings = input.Overrides.Voids.OrderBy(vo => vo.Identity.InsertionPoint.DistanceTo(v.insertionPoint)).FirstOrDefault(vo => vo.Identity.InsertionPoint.DistanceTo(v.insertionPoint) < 2);
                        }

                        if (input.Overrides.VoidGeometry != null && input.Overrides.VoidGeometry.Count > 0)
                        {
                            matchingVoidGeometry = input.Overrides.VoidGeometry.OrderBy(vo => vo.Identity.InsertionPoint.DistanceTo(v.insertionPoint)).FirstOrDefault(vo => vo.Identity.InsertionPoint.DistanceTo(v.insertionPoint) < 2);
                        }
                    }
                    var voidElement = new FootprintVoid(v.insertionPoint, matchingVoidGeometry?.Value.Boundary ?? v.VoidsOverride?.Value?.Boundary, levels, matchingVoidSettings?.Value?.StartingLevel ?? startingLevel, matchingVoidSettings?.Value?.LastLevel ?? endingLevel);
                    if (v.VoidsOverride != null)
                    {
                        Identity.AddOverrideIdentity(voidElement, v.VoidsOverride);
                    }
                    model.AddElement(voidElement);
                    // var unionedFps = UnionOverlappingFootprints(fpsWithinProfile);
                    foreach (var fp in fpsWithinProfile)
                    {
                        fp.AddVoid(voidElement);
                        // fp.Representation.SolidOperations.Add(voidElement.GetRepresentationAsVoid());
                    }
                    // foreach (var fp in unionedFps)
                    // {
                    //     fp.AddVoid(voidElement);
                    // }
                    // unionedFootprints.AddRange(unionedFps);
                }
            }
            foreach (var fp in footprints)
            {
                fp.RefreshRepresentationAndGenerateLevelElements();
                model.AddElements(fp.LevelElements);
                // fp.LevelElements.Clear();
                model.AddElement(fp);
            }
            // foreach (var fp in unionedFootprints)
            // {
            //     fp.RefreshRepresentationAndGenerateLevelElements();
            //     model.AddElements(fp.LevelElements);
            // }

            var output = new SketchMasterplanOutputs(footprints.Sum(f => f.Area * f.NumberOfLevels));
            // ugh, this is janky:
            foreach (var element in model.Elements)
            {
                switch (element.Value)
                {
                    case LevelVolume levelVolume:
                        output.Model.AddElement(new DeferredElement(levelVolume));
                        break;
                    case LevelPerimeter levelPerimeter:
                        output.Model.AddElement(new DeferredElement(levelPerimeter));
                        break;
                    case Level level:
                        output.Model.AddElement(new DeferredElement(level));
                        break;
                    case Envelope envelope:
                        output.Model.AddElement(new DeferredElement(envelope));
                        break;
                    case Footprint footprint:
                        footprint.LevelElements.Clear();
                        footprint.Envelope = null;
                        output.Model.AddElement(footprint);
                        break;
                    default:
                        output.Model.AddElement(element.Value);
                        break;
                }
            }
            return output;
        }

        private static List<Footprint> UnionOverlappingFootprints(IEnumerable<Footprint> fpsWithinProfile)
        {
            var unionedFps = new List<Footprint>();
            var minLevel = fpsWithinProfile.Min(fp => fp.StartingLevel);
            var maxLevel = fpsWithinProfile.Max(fp => fp.LastLevel);
            for (int i = minLevel; i <= maxLevel; i++)
            {
                var fpsAtLevel = fpsWithinProfile.Where(fp => fp.StartingLevel <= i && fp.LastLevel >= i);
                var profiles = fpsAtLevel.Select(fp => new Profile(fp.FootprintShape));
                var union = Profile.UnionAll(profiles);
                foreach (var p in union)
                {
                    var newFp = new Footprint(p.Perimeter, unionedFps.FirstOrDefault()?.BuildingLevels ?? fpsWithinProfile.FirstOrDefault()?.BuildingLevels)
                    {
                        StartingLevel = i,
                        LastLevel = i
                    };
                    unionedFps.Add(newFp);
                }
            }
            return unionedFps;
        }

        private static double PolygonSimilarity(Polygon a, Polygon b)
        {
            return a.Centroid().DistanceTo(b.Centroid());
        }
    }
}