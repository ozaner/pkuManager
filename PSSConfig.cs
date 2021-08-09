using Newtonsoft.Json;
using System;
using System.Linq;

public class PSSConfigObject
{
    private string[] boxes;

    [JsonProperty("Boxes")]
    public string[] Boxes { get => boxes; set => boxes = value.Distinct().ToArray(); }
    public bool BoxesSpecified { get; set; }
}