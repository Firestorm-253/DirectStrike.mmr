using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_1.dotNet
{
    [Serializable]
    public enum Commander
    {
        Abathur,
        Alarak,
        Artanis,
        Dehaka,
        Fenix,
        Horner,
        Karax,
        Kerrigan,
        Mengsk,
        Nova,
        Raynor,
        Stetmann,
        Stukov,
        Swann,
        Tychus,
        Vorazun,
        Zagara,
        Zeratul
    }

    public class Commanders
    {
        public static readonly int Amount = Enum.GetNames(typeof(Commander)).Length;
        public static string[] AllCommanderNames = Enum.GetNames(typeof(Commander));
        
        public static Commander Get(string commanderName)
        {
            return commanderName switch {
                "Abathur" => Commander.Abathur,
                "Alarak" => Commander.Alarak,
                "Artanis" => Commander.Artanis,
                "Dehaka" => Commander.Dehaka,
                "Fenix" => Commander.Fenix,
                "Horner" => Commander.Horner,
                "Karax" => Commander.Karax,
                "Kerrigan" => Commander.Kerrigan,
                "Mengsk" => Commander.Mengsk,
                "Nova" => Commander.Nova,
                "Raynor" => Commander.Raynor,
                "Stetmann" => Commander.Stetmann,
                "Stukov" => Commander.Stukov,
                "Swann" => Commander.Swann,
                "Tychus" => Commander.Tychus,
                "Vorazun" => Commander.Vorazun,
                "Zagara" => Commander.Zagara,
                "Zeratul" => Commander.Zeratul,
                _ => throw new Exception()
            };
        }
    }
}
