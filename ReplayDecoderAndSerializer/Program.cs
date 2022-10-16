using pax.dsstats.parser;
using pax.dsstats.shared;
using Serialization;
using System.Reflection;

await Decoder.SerializeReplays(@"Replays\New");
//await DeserializeReplays("Results\\Basilic.replays");

public static class Decoder
{
    //# Uploader-Version
    //static string sc2_path = string.Format("{0}\\StarCraft II\\Accounts", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
    //static string[] accounts_paths => Directory.GetDirectories(sc2_path);
    //static string[] subAccounts_paths => accounts_paths.SelectMany(Directory.GetDirectories).ToArray();
    //static int[] subAccounts => subAccounts_paths.Select(_ => _.Split('\\').Last()).Where(_ => _ != "Hotkeys").Select(_ => int.Parse(_.Split('-').Last())).ToArray();

    public static List<SerializableGame> DeserializeReplays(string file)
    {
        byte[] bytes = File.ReadAllBytes(file);
        PacketReader pr = new PacketReader(bytes);

        List<SerializableGame> serializableGames = new List<SerializableGame>();
        while (pr.BaseStream.CanRead && pr.BaseStream.Position < pr.BaseStream.Length) {
            SerializableGame serializableGame = pr.ReadObject<SerializableGame>();
            serializableGames.Add(serializableGame);
        }

        return serializableGames;
    }

    const string replaysToArchivate_path = "Results\\replaysToArchivate.txt";

    static object lock_replaysToArchivate = new object();
    private static List<string> _replaysToArchivate = new List<string>();
    static List<string> ReplaysToArchivate {
        get {
            lock (lock_replaysToArchivate) {
                return _replaysToArchivate;
            }
        }
        set {
            lock (lock_replaysToArchivate) {
                _replaysToArchivate = value;
            }
        }
    }

    private static void ArchivateReplays(string[] replaysToArchivate)
    {
        if (replaysToArchivate.Length != 0) {
            foreach (string file in replaysToArchivate) {
                File.Move(file, file.Replace("New", "Archive"), true);
            }
            File.WriteAllText(replaysToArchivate_path, string.Empty);
        }
    }

    public static async Task SerializeReplays(string folder)
    {
        Parse.Initialize(new s2protocol.NET.ReplayDecoderOptions() {
            Initdata = true,
            Details = true,
            Metadata = true,
            MessageEvents = false,
            TrackerEvents = true,
            GameEvents = false,
            AttributeEvents = false
        });

        string[] replaysToArchivate = await File.ReadAllLinesAsync(replaysToArchivate_path);
        ArchivateReplays(replaysToArchivate);

        string[] uploaderPaths = Directory.GetDirectories(folder);
        int totalAmount = Directory.GetFiles(folder, "*.SC2Replay", SearchOption.AllDirectories).Length;

        foreach (string uploaderPath in uploaderPaths) {
            string uploaderName = uploaderPath.Split('\\').Last();
            int uploaderAmount = Directory.GetFiles(uploaderPath, "*.SC2Replay", SearchOption.AllDirectories).Length;

            string[] accountPaths = Directory.GetDirectories(uploaderPath);

            foreach (string accountPath in accountPaths) {
                string accountName = accountPath.Split('\\').Last();
                string[] subAccounts = Directory.GetDirectories(accountPath);

                long uploaderBNid = long.Parse(accountName);
                long[] uploaderAllRegionIds = subAccounts.Select(_ => long.Parse(_.Split('\\').Last().Split('-').Last())).ToArray();
                Region[] uploaderAllRegions = subAccounts.Select(_ => (Region)int.Parse(_.Split('\\').Last().Split('-').First())).ToArray();
                Dictionary<Region, long> uploaderAllRegionIdsPair = new Dictionary<Region, long>();

                for (int s = 0; s < subAccounts.Length; s++) {
                    uploaderAllRegionIdsPair.Add(uploaderAllRegions[s], uploaderAllRegionIds[s]);
                }

                foreach (string subAccount in subAccounts) {
                    await ImportReplays(subAccount, uploaderBNid, uploaderAllRegionIdsPair, uploaderName);
                }
            }
        }

        Parse.Dispose();
    }

    private static async Task ImportReplays(string folder, long uploaderBNid, Dictionary<Region, long> uploaderAllRegionIds, string uploaderName)
    {
        string[] files = Directory.GetFiles(folder).ToArray();
        long replaySizeSum = new DirectoryInfo(folder).EnumerateFiles("*.SC2Replay", SearchOption.AllDirectories).Sum(_ => _.Length);
        long doneReplaysSizesSum = 0;

        PacketWriter pw = new PacketWriter();
        DateTime uploaderStartTime = DateTime.Now;

        for (int i = 0; i < files.Length; i++) {
            DateTime timeBefore = DateTime.Now;

            //# Load Replay
            string file = files[i];
            s2protocol.NET.Sc2Replay sc2_replay;
            DsReplay ds_replay;
            try {
                sc2_replay = await Parse.GetSc2Replay(file);
                ds_replay = Parse.GetDsReplay(sc2_replay);
            } catch {
                ReplaysToArchivate.Add(file);
                continue;
            }

            if (ds_replay == null) {
                ReplaysToArchivate.Add(file);
                continue;
            }

            //var dto_replay = pax.dsstats.parser.Parse.GetReplayDto(ds_replay);

            long replaySize = new FileInfo(file).Length;
            doneReplaysSizesSum += replaySize;



            //# Serialize Game
            bool hasLeavers = false;
            bool isUploaderLeaver = false;
            int realDurration = ds_replay.Players.Max(x => x.Duration);

            var sc2_uploader = sc2_replay.Details?.Players.ToList().Find(_ => uploaderAllRegionIds.Any(x =>
                x.Equals(new KeyValuePair<Region, long>((Region)_.Toon.Region, _.Toon.Id))));
            (long?, Dictionary<Region, long>)? uploaderId = ((sc2_uploader != null) ? (uploaderBNid, uploaderAllRegionIds) : null);
            
            DsPlayer[] uploaderEqualityAmountCheck = ds_replay.Players.FindAll(_ => _.Name.Equals(sc2_uploader?.Name)).ToArray();
            if (uploaderEqualityAmountCheck.Length > 1) {
                ReplaysToArchivate.Add(file);
                continue;
            }
            DsPlayer ds_uploader = uploaderEqualityAmountCheck.FirstOrDefault();
            
            if (ds_replay.Duration < 89) {
                ReplaysToArchivate.Add(file);
                continue;
            }

            if (ds_replay.WinnerTeam == 0) {
                hasLeavers = true;
                isUploaderLeaver = true;

                if (ds_uploader?.Duration < 89) {
                    ReplaysToArchivate.Add(file);
                    continue;
                }

                realDurration = ds_replay.Players.Max(_ => !_.Equals(ds_uploader) ? _.Duration : 0);
            }

            DsPlayer[][] team_leavers = new DsPlayer[][] {
                ds_replay.Players.Where(_ => _.Team.Equals(1) && ((_.Duration < realDurration - 89) || (isUploaderLeaver && _.Equals(ds_uploader)))).OrderBy(_ => _.Duration).ToArray(),
                ds_replay.Players.Where(_ => _.Team.Equals(2) && ((_.Duration < realDurration - 89) || (isUploaderLeaver && _.Equals(ds_uploader)))).OrderBy(_ => _.Duration).ToArray()
            };

            List<SerializablePlayer> serializablePlayers = new List<SerializablePlayer>();

            for (int k = 0; k < ds_replay.Players.Count; k++) {
                var ds_player = ds_replay.Players[k];
                var sc2_player = sc2_replay.Details?.Players.Where(x => x.Name.Equals(ds_player.Name)).FirstOrDefault();

                (long?, Dictionary<Region, long>) playerId = ((ds_player.Equals(ds_uploader) && uploaderId.HasValue) ? uploaderId.Value :
                    (null, new Dictionary<Region, long>(new KeyValuePair<Region, long>[] {
                        new KeyValuePair<Region, long>((Region)sc2_player.Toon.Region, sc2_player.Toon.Id)
                    })));

                int enemyTeam = 2 - (ds_player.Team - 1 % 2);

                bool isLeaver = false;
                bool isWinner = false;

                if (team_leavers.SelectMany(_ => _).Any()) {
                    if (team_leavers.SelectMany(_ => _).Contains(ds_player)) {
                        isLeaver = true;

                        if ((team_leavers[ds_player.Team - 1].Length.Equals(ds_replay.Players.Count / 2)) && ds_player.Equals(team_leavers[ds_player.Team - 1].Last())) {
                            isLeaver = false;
                        }
                    }
                }

                if (!isLeaver) {
                    if (ds_player.Team.Equals(ds_replay.WinnerTeam)) {
                        isWinner = true;
                    } else if (team_leavers[enemyTeam - 1].Length.Equals(ds_replay.Players.Count / 2)) {
                        isWinner = true;
                    }
                }

                SerializablePlayer serializablePlayer = new SerializablePlayer(
                    playerId, ds_player.Name, ds_player.Clan, ds_player.Team, ds_player.GamePos, ds_player.Kills, ds_player.Race, isWinner, isLeaver);
                serializablePlayers.Add(serializablePlayer);
            }

            SerializableGame serializableGame = new SerializableGame(ds_replay.GameMode, ds_replay.GameTime, ds_replay.WinnerTeam, hasLeavers, serializablePlayers);
            pw.WriteT(serializableGame);
            byte[] bytes = pw.GetBytes(true, false).ToArray();


            //# Write To File
            Console.Clear();
            Console.WriteLine("Don't cancel!!!");
            using (FileStream fs = new FileStream(string.Format("Results\\{0}.ds-replays", uploaderName), FileMode.Append, FileAccess.Write)) {
                await fs.WriteAsync(bytes, 0, bytes.Length);
                fs.Close();
            }

            Console.Clear();
            Console.WriteLine("Current Replay\n{1} ({0} ms)", (int)DateTime.Now.Subtract(timeBefore).TotalMilliseconds, GetFormatedNumber(replaySize));
            Console.WriteLine("\n{0}", uploaderName);
            Console.WriteLine("{0} / {1}", (i + 1), files.Length);
            Console.WriteLine("{0}% ({1} / {2})", ((int)(10000.0 * doneReplaysSizesSum / (double)replaySizeSum)) / 100.0, GetFormatedNumber(doneReplaysSizesSum), GetFormatedNumber(replaySizeSum));
            Console.WriteLine("{0} min", (int)(DateTime.Now.Subtract(uploaderStartTime).TotalMinutes));
            Console.WriteLine("replaysToArchivate: {0}", ReplaysToArchivate.Count);
            GC.Collect();

            //# Archivating Replay-/s
            ReplaysToArchivate.Add(file);
            List<string> copy = new List<string>(ReplaysToArchivate.ToArray());

            try {
                foreach (string _file in ReplaysToArchivate) {
                    if (File.Exists(_file)) {
                        File.Move(_file, _file.Replace("New", "Archive"), true);
                    }
                    copy.Remove(_file);
                }
            } catch {
            }

            ReplaysToArchivate = copy;
            await File.WriteAllLinesAsync(replaysToArchivate_path, ReplaysToArchivate);
        }
    }

    private static string GetFormatedNumber(double number)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        while (number >= 1024 && order < sizes.Length - 1) {
            order++;
            number = number / 1024;
        }

        return string.Format("{0:0.##} {1}", number, sizes[order]);
    }
}