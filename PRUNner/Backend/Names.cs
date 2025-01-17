using System.Diagnostics.CodeAnalysis;

namespace PRUNner.Backend
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Names
    {
        public static class Systems
        {
            public const string AntaresI = "Antares I";
            public const string Benten = nameof(Benten);
            public const string Hortus = nameof(Hortus);
            public const string Moria = nameof(Moria);
        }

        public static class Planets
        {
            public const string Katoa = nameof(Katoa);
            public const string Montem = nameof(Montem);
            public const string Pyrgos = nameof(Pyrgos);
            public const string Promitor = nameof(Promitor);
            public const string Umbra = nameof(Umbra);
            public const string Vallis = nameof(Vallis);
        }
        
        public static class Materials
        {
            public const string FEO = nameof(FEO);
            public const string LST = nameof(LST);
            
            public const string MCG = nameof(MCG);
            public const string AEF = nameof(AEF);
            public const string SEA = nameof(SEA);
            public const string HSE = nameof(HSE);
            public const string MGC = nameof(MGC);
            public const string BL = nameof(BL);
            public const string INS = nameof(INS);
            public const string TSH = nameof(TSH);
        }

        public static class Buildings
        {
            public const string CM = nameof(CM);
            public const string HB1 = nameof(HB1);
            public const string HB2 = nameof(HB2);
            public const string HB3 = nameof(HB3);
            public const string HB4 = nameof(HB4);
            public const string HB5 = nameof(HB5);
            public const string HBB = nameof(HBB);
            public const string HBC = nameof(HBC);
            public const string HBM = nameof(HBM);
            public const string HBL = nameof(HBL);
            public const string STO = nameof(STO);
         
            public const string COL = nameof(COL);
            public const string EXT = nameof(EXT);
            public const string RIG = nameof(RIG);
            
            public const string FRM = nameof(FRM);
            
            public const string ORC = nameof(ORC);
            
        }
    }
}