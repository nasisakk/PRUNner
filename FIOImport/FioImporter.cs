﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using FIOImport.Pocos;
using FIOImport.POCOs;
using FIOImport.POCOs.Buildings;
using FIOImport.POCOs.Planets;
using Newtonsoft.Json;
using NLog;

namespace FIOImport
{
    public static class FioImporter
    {
        private static readonly HttpClient Client = new();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string CacheFolder = "FIOCache/";
        private const string PlanetFolder = CacheFolder + "Planets/";

        private const string AllBuildingsPath = CacheFolder + "allBuildings.json";
        private const string AllMaterialsPath = CacheFolder + "allMaterials.json";
        private const string AllSystemsPath = CacheFolder + "systemStars.json";
        private const string AllPlanetIdentifiersPath = CacheFolder + "allPlanetIdentifiers.json";
        private const string RainPricesPath = CacheFolder + "rainPrices.json";

        public static RawData LoadAllFromCache()
        {
            if (Directory.Exists(CacheFolder))
            {
                return new RawData(LoadBuildings(), LoadMaterials(), LoadAllPlanetData(), LoadSystems(), LoadPrices());
            }

            Logger.Info("Unable to find Cache folder, downloading all data from fio instead. This might take a little while.");
            return ImportAll();
        }

        private static RawData ImportAll()
        {
            Directory.CreateDirectory(CacheFolder);
            Directory.CreateDirectory(PlanetFolder);

            return new RawData(DownloadBuildings(), DownloadMaterials(), LoadAllPlanetData(), DownloadSystems(), DownloadPrices());
        }

        private static FioBuilding[] LoadBuildings()
        {
            if (!File.Exists(AllBuildingsPath))
            {
                Logger.Info("Unable to locate cache file for buildings, downloading instead.");
                return DownloadBuildings();
            }

            Logger.Info("Loading Buildings...");
            return LoadFromCache<FioBuilding[]>(AllBuildingsPath)!;
        }

        private static FioBuilding[] DownloadBuildings()
        {
            Logger.Info("Downloading Building data from FIO...");
            return DownloadAndCache<FioBuilding[]>("https://rest.fnar.net/building/allbuildings", AllBuildingsPath)!;
        }

        private static FioMaterial[] LoadMaterials()
        {
            if (!File.Exists(AllMaterialsPath))
            {
                Logger.Info("Unable to locate cache file for materials, downloading instead.");
                return DownloadMaterials();
            }

            Logger.Info("Loading Materials...");
            return LoadFromCache<FioMaterial[]>(AllMaterialsPath)!;
        }

        private static FioMaterial[] DownloadMaterials()
        {
            Logger.Info("Donwloading Material data from FIO...");
            return DownloadAndCache<FioMaterial[]>("https://rest.fnar.net/material/allmaterials", AllMaterialsPath)!;
        }

        private static FioRainPrices[] LoadPrices()
        {
            if (!File.Exists(RainPricesPath))
            {
                Logger.Info("Unable to locate cache file for price data, downloading instead.");
                return DownloadPrices();
            }

            Logger.Info("Loading Price data from Cache...");
            return LoadFromCache<FioRainPrices[]>(RainPricesPath)!;
        }

        public static FioRainPrices[] DownloadPrices()
        {
            Logger.Info("Importing Price data from Fio...");
            return DownloadAndCache<FioRainPrices[]>("https://rest.fnar.net/rain/prices", RainPricesPath)!;
        }

        private static FioPlanetIdentifier[] LoadPlanetIdentifiers()
        {
            if (!File.Exists(AllPlanetIdentifiersPath))
            {
                Logger.Info("Unable to locate planet identifier list, going to download it instead instead.");
                return DownloadPlanetIdentifiers();
            }

            return LoadFromCache<FioPlanetIdentifier[]>(AllPlanetIdentifiersPath)!;
        }

        private static FioPlanetIdentifier[] DownloadPlanetIdentifiers()
        {
            Logger.Info("Downloading Planet list from FIO...");
            return DownloadAndCache<FioPlanetIdentifier[]>("https://rest.fnar.net/planet/allplanets", AllPlanetIdentifiersPath)!;
        }

        private static FioPlanet[] LoadAllPlanetData()
        {
            LoadPlanetIdentifiers();
            return GetAllPlanetData();
        }

        private static FioPlanet[] LoadPlanetData(string fileName)
        {
            var json = File.ReadAllText(fileName);
            var result = JsonConvert.DeserializeObject<FioPlanet[]>(json);

            return result!;
        }

        private static FioPlanet[] GetAllPlanetData()
        {
            return LoadFromCacheOrDownloadPlanetData();
        }

        private static FioPlanet[] LoadFromCacheOrDownloadPlanetData()
        {
            string allPlanetsJsonFile = $"{PlanetFolder}AllPlanets.json";
            if (File.Exists(allPlanetsJsonFile))
            {
                return LoadPlanetData(allPlanetsJsonFile);
            }

            return DownloadAndCache<FioPlanet[]>("https://rest.fnar.net/planet/allplanets/full", allPlanetsJsonFile)!;
        }

        private static FioSystem[] LoadSystems()
        {
            if (!File.Exists(AllSystemsPath))
            {
                Logger.Info("Unable to locate system data cache file, downloading it instead..");
                return DownloadSystems();
            }

            Logger.Info("Loading System Data from Cache...");
            return LoadFromCache<FioSystem[]>(AllSystemsPath)!;
        }

        private static FioSystem[] DownloadSystems()
        {
            Logger.Info("Downloading Systems from FIO...");
            return DownloadAndCache<FioSystem[]>("https://rest.fnar.net/systemstars", AllSystemsPath)!;
        }

        private const int MaximumRetries = 5;
        private static T? DownloadAndCache<T>(string requestUri, string cacheFilePath)
        {
            var tries = 0;
            while (tries < MaximumRetries + 1)
            {
                try
                {
                    var json = Client.GetStringAsync(requestUri).GetAwaiter().GetResult();
                    File.WriteAllText(cacheFilePath, json);
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch (HttpRequestException e)
                {
                    tries++;
                    Logger.Error(e, "Errror whilst downlading data – waiting a bit, then retrying [{Tries}/{MaximumRetries}]", tries, MaximumRetries);
                    Thread.Sleep(2000);
                }
            }

            throw new Exception("Unable to query " + requestUri);
        }

        private static T? LoadFromCache<T>(string path)
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}