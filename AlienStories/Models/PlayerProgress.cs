using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienStories.Models;

public class PlayerProgress
{
    public int TotalFriends { get; set; }
    public int TotalHugs { get; set; }
    public int TotalFed { get; set; }
    public int TotalStoriesHeard { get; set; }
    public string? RarestCaught { get; set; }
}