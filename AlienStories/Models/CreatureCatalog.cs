using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienStories.Models;

public class CreatureCatalog
{
    public int ShapeType { get; set; }
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Planet { get; set; } = string.Empty;
    public int Rarity { get; set; }
    public string ColorHex { get; set; } = "#FFA500";
    public string Emoji { get; set; } = "🐱";
    public string Description { get; set; } = string.Empty;
    public string Story { get; set; } = string.Empty;

    public string RarityName => Rarity switch
    {
        1 => "Обычный",
        2 => "Необычный",
        3 => "Редкий",
        4 => "Легендарный",
        _ => "Неизвестно"
    };

    public string RarityColor => Rarity switch
    {
        1 => "#4CAF50",
        2 => "#2196F3",
        3 => "#9C27B0",
        4 => "#FFD700",
        _ => "#888"
    };
}

