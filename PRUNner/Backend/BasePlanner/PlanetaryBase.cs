using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData.Binding;
using PRUNner.Backend.Data;
using PRUNner.Backend.Enums;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace PRUNner.Backend.BasePlanner
{
    public class PlanetaryBase : ReactiveObject
    {
        private bool _loading;
        public readonly Empire Empire;
        public PlanetData Planet { get; }
        public PriceOverrides PriceOverrides { get; } = new();

        public PlanetaryBaseInfrastructure InfrastructureBuildings { get; }
        public ObservableCollection<PlanetBuilding> ProductionBuildings { get; } = new();
        public ShoppingCart.ShoppingCart ShoppingCart { get; }
        
        public ExpertAllocation ExpertAllocation { get; } = new();
        public PlanetWorkforce WorkforceRequired { get; } = new();
        public PlanetWorkforce WorkforceCapacity { get; } = new();
        public PlanetWorkforce WorkforceRemaining { get; } = new();
        public PlanetWorkforceFilledJobRatio FilledJobRatio { get; } = new();
        public PlanetWorkforceUpkeepCosts WorkforceUpkeepCosts { get; }
        
        public WorkforceSatisfaction WorkforceSatisfaction { get; } = new();
        public PlanetProductionTable ProductionTable { get; }
        
        public ProvidedConsumables ProvidedConsumables { get; } = new();
        [Reactive] public CoGCBonusType CoGCBonus { get; set; } = CoGCBonusType.None;
        
        public int AreaTotal { get; } = Constants.BaseArea;
        [Reactive] public int AreaDeveloped { get; private set; }
        [Reactive] public int AreaAvailable { get; private set; } = Constants.BaseArea;
        [Reactive] public double ProfitPerDay { get; private set; }
        
        [Reactive] public bool IncludeCoreModuleInColonyCosts { get; set; }
        [Reactive] public double ColonyCosts { get; private set; }
        [Reactive] public double TotalDailyRepairCosts { get; private set; }
        [Reactive] public double NetProfit { get; private set; }
        [Reactive] public double ReturnOfInvestment { get; private set; }
        
        [Reactive] public double VolumeIn { get; private set; }
        [Reactive] public double VolumeOut { get; private set; }
        [Reactive] public double WeightIn { get; private set; }
        [Reactive] public double WeightOut { get; private set; }

        public IEnumerable<PlanetBuildingProductionRecipe> ActiveRecipes =>
            ProductionBuildings.SelectMany(building => building.Production.Select(prod => prod.ActiveRecipe!));
        
        public PlanetaryBase(Empire empire, PlanetData planet)
        {
            BeginLoading();

            ShoppingCart = new ShoppingCart.ShoppingCart(this);
            PriceOverrides.OnPriceUpdate += OnPriceDataUpdate;
            
            Empire = empire;
            Planet = planet;

            WorkforceUpkeepCosts = new PlanetWorkforceUpkeepCosts(this);
            InfrastructureBuildings = new PlanetaryBaseInfrastructure(this);
            ProductionTable = new PlanetProductionTable(this);
            foreach (var infrastructureBuilding in InfrastructureBuildings.All)
            {
                infrastructureBuilding.Changed.Subscribe(_ => OnBuildingChange());
            }

            ProvidedConsumables.Changed.Subscribe(_ =>
            {
                WorkforceSatisfaction.Recalculate(ProvidedConsumables, FilledJobRatio);
                OnPriceDataUpdate();
            });

            WorkforceSatisfaction.Changed.Subscribe(_ => RecalculateBuildingEfficiencies());
            Empire.Headquarters.Changed.Subscribe(_ => RecalculateBuildingEfficiencies());
            
            // TODO: Figure out why this here does not work, then use Observable.Merge instead of subscribing a dozen times.
            // ExpertAllocation.Changed.Subscribe(_ => RecalculateBuildingEfficiencies());
            ExpertAllocation.Agriculture.Changed.Subscribe(_ => RecalculateBuildingEfficiencies());
            ExpertAllocation.Chemistry.Changed.Subscribe(_ => RecalculateBuildingEfficiencies());
            ExpertAllocation.Construction.Changed.Subscribe(_ => RecalculateBuildingEfficiencies());
            ExpertAllocation.Electronics.Changed.Subscribe(_ => RecalculateBuildingEfficiencies());
            ExpertAllocation.FoodIndustries.Changed.Subscribe(_ => RecalculateBuildingEfficiencies());
            ExpertAllocation.FuelRefining.Changed.Subscribe(_ => RecalculateBuildingEfficiencies());
            ExpertAllocation.Manufacturing.Changed.Subscribe(_ => RecalculateBuildingEfficiencies());
            ExpertAllocation.Metallurgy.Changed.Subscribe(_ => RecalculateBuildingEfficiencies());
            ExpertAllocation.ResourceExtraction.Changed.Subscribe(_ => RecalculateBuildingEfficiencies());

            this.WhenPropertyChanged(x => x.CoGCBonus).Subscribe(_ => RecalculateBuildingEfficiencies());
            this.WhenPropertyChanged(x => x.IncludeCoreModuleInColonyCosts).Subscribe(_ => RecalculateProfits());
         
            FinishLoading();
        }

        private void RecalculateBuildingEfficiencies()
        {
            if (_loading)
            {
                return;
            }
            
            foreach (var building in ProductionBuildings)
            {
                building.UpdateProductionEfficiency(WorkforceSatisfaction, ExpertAllocation, CoGCBonus, Empire.Headquarters);
            }
            
            OnProductionChange();
        }

        public PlanetBuilding AddBuilding(BuildingData building)
        {
            var addedBuilding = ProductionBuildings.SingleOrDefault(x => x.Building == building);
            if (addedBuilding == null)
            {
                addedBuilding = PlanetBuilding.FromProductionBuilding(this, Planet, building);
                addedBuilding.Changed.Subscribe(_ => OnBuildingChange());
                addedBuilding.OnProductionUpdate += OnProductionChange;
                ProductionBuildings.Add(addedBuilding);
            }

            addedBuilding.Amount++;
            return addedBuilding;
        }

        private void OnBuildingChange()
        {
            if (_loading)
            {
                return;
            }
            
            UpdateEverything();
        }

        private void OnProductionChange()
        {
            ProductionTable.Update(ProductionBuildings);

            RecalculateProfits();
            
            VolumeIn = ProductionTable.Inputs.Sum(x => x.Volume);
            WeightIn = ProductionTable.Inputs.Sum(x => x.Weigth);
            
            VolumeOut = ProductionTable.Outputs.Sum(x => x.Volume);
            WeightOut = ProductionTable.Outputs.Sum(x => x.Weigth);
        }

        private void RecalculateWorkforce()
        {
            WorkforceRequired.Reset();
            foreach (var building in ProductionBuildings)
            {
                WorkforceRequired.Add(building.Building.Workforce, building.Amount);
            }

            WorkforceCapacity.Reset();
            foreach (var building in InfrastructureBuildings.All)
            {
                WorkforceCapacity.Add(building.Building.AdditionalWorkforceSpace, building.Amount);
            }

            WorkforceRemaining.SetRemainingWorkforce(WorkforceRequired, WorkforceCapacity);
            FilledJobRatio.Recalculate(WorkforceCapacity, WorkforceRequired);
            WorkforceSatisfaction.Recalculate(ProvidedConsumables, FilledJobRatio);
            WorkforceUpkeepCosts.Recalculate();
        }

        private void RecalculateSpace()
        {
            var usedArea = ProductionBuildings.Sum(x => x.CalculateNeededArea())
                + InfrastructureBuildings.All.Sum(x => x.CalculateNeededArea());
            
            AreaDeveloped = usedArea;
            AreaAvailable = AreaTotal - usedArea;
        }

        public void BeginLoading()
        {
            _loading = true;
        }

        public void FinishLoading()
        {
            _loading = false;
            UpdateEverything();
        }

        public void RemoveProductionBuilding(PlanetBuilding building)
        {
            ProductionBuildings.Remove(building);
            UpdateEverything();
        }

        private void UpdateEverything()
        {
            RecalculateWorkforce();
            RecalculateBuildingEfficiencies();
            RecalculateSpace();
            OnProductionChange();   
            OnPriceDataUpdate();
            ShoppingCart.UpdateBuildings();
            this.RaisePropertyChanged(nameof(ActiveRecipes));
        }

        public void OnPriceDataUpdate()
        {
            WorkforceUpkeepCosts.Recalculate();
            foreach (var building in ProductionBuildings)
            {
                building.OnPriceDataUpdate();
            }

            foreach (var building in InfrastructureBuildings.All)
            {
                building.OnPriceDataUpdate();
            }

            ProductionTable.UpdatePriceData();
            RecalculateProfits();
        }

        public void RecalculateProfits()
        {
            ProfitPerDay = ProductionTable.Rows.Sum(x => x.Value);
            ColonyCosts = ProductionBuildings.Sum(x => x.BuildingCost * x.Amount) + InfrastructureBuildings.All.Sum(x => x.BuildingCost * x.Amount);
            if (!IncludeCoreModuleInColonyCosts)
            {
                ColonyCosts -= InfrastructureBuildings.CM.BuildingCost;
            }
            
            TotalDailyRepairCosts = ProductionBuildings.Sum(x => x.DailyCostForRepairs);
            NetProfit = ProfitPerDay - TotalDailyRepairCosts;

            ReturnOfInvestment = ColonyCosts / NetProfit;
        }
    }
}