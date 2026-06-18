using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienStories.Models;

public class CapturedCreature
{
    public int Id { get; set; }
    public int CatalogId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public DateTime CaptureDate { get; set; } = DateTime.Now;
    public double Size { get; set; } = 1.0;
    public bool IsShiny { get; set; }

    //забота
    public int Hunger { get; set; } = 100;
    public int TimesHugged { get; set; }
    public int TimesFed { get; set; }
    public int TimesHeardStory { get; set; }
    public DateTime? LastFed { get; set; }
    public DateTime? LastHugged { get; set; }

    //навигационное свойство
    public CreatureCatalog? Catalog { get; set; }
}
