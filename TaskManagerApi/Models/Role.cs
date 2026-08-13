
using System.Text.Json.Serialization;

namespace TaskManager.Api.Models;

public class Role
{
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    [JsonIgnore]
    public ICollection<User> Users{get; set;} = new List<User>();
}