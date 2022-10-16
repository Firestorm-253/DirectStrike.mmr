namespace Serialization;

[Serializable]
public enum Region
{
    NA = 1,
    EU = 2,
    Rest = 3,
}

[Serializable]
public record SerializableGame
{
    public string GameMode { get; set; }
    public DateTime DateTime { get; set; }
    public int WinnerTeam { get; set; }
    public bool HasLeavers { get; set; }
    public List<SerializablePlayer> Players { get; set; }

    public SerializablePlayer Uploader => this.Players.Find(_ => _.IsUploader);

    public SerializableGame(string gameMode, DateTime dateTime, int winnerTeam, bool hasLeavers, List<SerializablePlayer> players)
    {
        this.GameMode = gameMode;
        this.DateTime = dateTime;
        this.WinnerTeam = winnerTeam;
        this.HasLeavers = hasLeavers;
        this.Players = players;
    }
}

[Serializable]
public class SerializablePlayer
{
    public (long?, Dictionary<Region, long>) Id { get; set; }
    public string CurrentName { get; set; }
    public string CurrentClan { get; set; }

    public int Team { get; set; }
    public int InGamePosition { get; set; }
    public int KillValue { get; set; }
    public string Commander { get; set; }
    public bool IsWinner { get; set; }
    public bool IsLeaver { get; set; }

    public bool IsUploader => (this.Id.Item1 != null);

    public SerializablePlayer(
        (long?, Dictionary<Region, long>) id,
        string currentName,
        string currentClan,
        int team,
        int inGamePosition,
        int killValue,
        string commander,
        bool isWinner,
        bool isLeaver)
    {
        this.Id = id;
        this.CurrentName = currentName;
        this.CurrentClan = currentClan;
        this.Team = team;
        this.InGamePosition = inGamePosition;
        this.KillValue = killValue;
        this.Commander = commander;
        this.IsWinner = isWinner;
        this.IsLeaver = isLeaver;
    }
}