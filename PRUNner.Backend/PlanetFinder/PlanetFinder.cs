using System.Collections.Generic;
using System.Linq;
using PRUNner.Backend.Data;

namespace PRUNner.Backend.PlanetFinder
{
    public static class PlanetFinder
    {
        public static IEnumerable<PlanetData> Find(FilterCriteria filterCriteria, params string[] resourceFilter)
        {
            return PlanetData.AllItems.Values
                .Where(filterCriteria.DoesPlanetFitCriteria)
                .Where(x => DoesPlanetHaveAllResources(x, resourceFilter));            
        }
        
        public static IEnumerable<PlanetData> Find(FilterCriteria filterCriteria, params MaterialData[] resourceFilter)
        {
            return PlanetData.AllItems.Values
                .Where(filterCriteria.DoesPlanetFitCriteria)
                .Where(x => DoesPlanetHaveAllResources(x, resourceFilter));
        }

        private static bool DoesPlanetHaveAllResources(PlanetData planet, IEnumerable<MaterialData> resources)
        {
            return resources.All(planet.HasResource);
        }        
        
        private static bool DoesPlanetHaveAllResources(PlanetData planet, IEnumerable<string> resources)
        {
            return resources.All(planet.HasResource);
        }
    }
}