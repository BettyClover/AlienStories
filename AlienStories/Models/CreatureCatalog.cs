using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienStories.Models;

public class CreatureCatalog
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Planet { get; set; } = string.Empty;
    public int Rarity { get; set; } // 1=Common, 2=Uncommon, 3=Rare, 4=Legendary
    public string ColorHex { get; set; } = "#FFA500";
    public string Emoji { get; set; } = "🐱";
    public string Description { get; set; } = string.Empty;
    public string Story { get; set; } = string.Empty; // История для кнопки "Расскажи"
}
