using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TournamentManager.Core.Services;

namespace TournamentManager.Core.Models
{
    public enum Category
    {
        None = 0,
        Men = 1,
        Women = 2,
        Mixed = 3
    }

    public enum ProgressStatus
    {
        None = 0,
        /// <summary>申込み受付中</summary>
        BeingAccepted = 1,
        /// <summary>抽選会</summary>
        Lottery = 2,
        /// <summary>大会期間中</summary>
        DuringTheTournament = 3,
        /// <summary>大会終了</summary>
        TournamentEnded = 4
    }

    /// <summary>
    /// 大会情報
    /// </summary>
    public class TournamentInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>進捗状況</summary>
        public ProgressStatus Status { get; set; } = ProgressStatus.BeingAccepted;

        // 選択されたチームIDのリスト
        public List<string> Teams { get; set; } = new();

        // 追加：枠番号（キー）と チームID（値）の紐づけ
        // 男子用・女子用で分けて管理します
        public Dictionary<int, string> MenSlotAssignments { get; set; } = new();
        public Dictionary<int, string> WomenSlotAssignments { get; set; } = new();

        public List<MatchResult> MenMatchResults { get; set; } = new();
        public List<MatchResult> WomenMatchResults { get; set; } = new();
    }

    // 大会基本名称マスタ
    public class TournamentNameMaster
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// 地区マスタ
    /// </summary>
    public class AreaMaster
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// チームマスタ
    /// </summary>
    public class TeamMaster
    {
        /// <summary>チームID</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        /// <summaryチーム名称</summary>
        public string Name { get; set; } = string.Empty;
        /// <summaryチーム略称</summary>
        public string ShortName { get; set; } = string.Empty;
        /// <summary>所属地区名</summary>
        public string AreaName { get; set; } = string.Empty;
        // 男女区分
        public string Gender { get; set; } = "Men";           // "Men" または "Women"
    }

    // アプリ全体の設定やマスタをまとめるコンテナ
    public class AppMasterSettings
    {
        public List<TournamentNameMaster> TournamentNames { get; set; } = new();
        public List<AreaMaster> Areas { get; set; } = new();
        public List<TeamMaster> Teams { get; set; } = new();
    }

    public class ExportContainer
    {
        public List<TournamentInfo>? SavedTournaments { get; set; }
        public AppMasterSettings? MasterSettings { get; set; }
        public Guid? LastActiveTournamentId { get; set; }
    }

    public class TournamentEntry
    {
        public string Gender { get; set; } = "Men";
        public List<TeamMaster> ConfirmedTeams { get; set; } = new List<TeamMaster>();
        public Dictionary<int, string> SlotAssignments { get; set; } = new Dictionary<int, string>();
        public Dictionary<string, MatchResult> MatchResults { get; set; } = new Dictionary<string, MatchResult>();
    }

    public class MatchResult
    {
        public int MatchId { get; set; }
        public int? ScoreA { get; set; }
        public int? ScoreB { get; set; }
        public string? WinnerTeamId { get; set; }
        public bool IsFinished { get; set; }
    }

    public class MatchNode
    {
        public string MatchId { get; set; } = "";
        public int Round { get; set; }
        public int Position { get; set; }
        public TeamMaster? TeamA { get; set; }
        public TeamMaster? TeamB { get; set; }
        public int? ScoreA { get; set; }
        public int? ScoreB { get; set; }
        public string? WinnerTeamId { get; set; }
        public bool IsSeedA { get; set; }
        public bool IsSeedB { get; set; }

        public double X { get; set; }
        public double Y { get; set; }
        public double ParentAY { get; set; }
        public double ParentBY { get; set; }

        public double LineStartXA { get; set; }
        public double LineStartXB { get; set; }

        // 【勝者ルート用】各線のCSSクラスを保持する変数 ("match-line" または "winner-line")
        public string ClassLineA { get; set; } = "match-line";
        public string ClassLineB { get; set; } = "match-line";
        public string ClassVerticalA { get; set; } = "match-line"; // 中央上側の縦線
        public string ClassVerticalB { get; set; } = "match-line"; // 中央下側の縦線
        public string ClassNextLine { get; set; } = "match-line";  // 次へ進む右横線

        /// <summary>上側(A)から中央へ向かう合流縦線のCSSクラス</summary>
        public string ClassLineVerticalUp { get; set; } = string.Empty;

        /// <summary>下側(B)から中央へ向かう合流縦線のCSSクラス</summary>
        public string ClassLineVerticalDown { get; set; } = string.Empty;
    }

    public class SvgTextElement
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string CssClass { get; set; } = "";
        public string Content { get; set; } = "";
    }

    /// <summary>
    /// 大会情報
    /// </summary>
    public class TournamentData
    {
        public enum UsageCls
        {
            None = 0,
            ForLottery = 1,
            ForTournament = 2
        }

        /// <summary>
        /// 男子チーム数
        /// </summary>
        public int NumOfMensTeams { get; set; }

        /// <summary>
        /// 女子チーム数
        /// </summary>
        public int NumOfwomensTeams { get; set; }

        ///// <summary>
        ///// 用途
        ///// </summary>
        //public UsageCls Usage { get; set; } = UsageCls.None;

        ///// <summary>
        ///// 大会名称
        ///// </summary>
        //public string TournamentName { get; set; } = string.Empty;

        /// <summary>
        /// 地区名称の表示
        /// </summary>
        public bool District { get; set; }
        /// <summary>
        /// BEST4で決勝リーグ
        /// </summary>
        public bool FinalLeague { get; set; }
        /// <summary>
        /// オープン参加表示枠
        /// </summary>
        public bool OpenDisplayFrame { get; set; }
        public List<AreaMaster> VenueDatas { get; set; } = new List<AreaMaster>();

        public Dictionary<Category, BracketData> BrackectDataDic { get; set; } = new Dictionary<Category, BracketData>();

        public PartInfo _allDataInfo { get; set; } = new();
        public Dictionary<int, PartInfo> _partDic { get; set; } = new Dictionary<int, PartInfo>();
    }
    /// <summary>
    /// パート毎の情報
    /// </summary>
    public class PartInfo
    {
        /// <summary>
        /// パート番号
        /// </summary>
        public int PartNumber { get; set; }
        /// <summary>
        /// チーム数
        /// </summary>
        public int NumOfTeams { get; set; } = 0;
        /// <summary>
        /// 回戦数
        /// </summary>
        public int Round { get; set; } = 0;
        /// <summary>
        /// 作業用の枠数（出場チーム数以上の最小べき数）
        /// </summary>
        public int FullFrames { get; set; } = 0;
        /// <summary>
        /// 1回戦のデータ（0は、不要な枠を表します）
        /// </summary>
        public int[,]? FirstRoundData { get; set; }
        public int NumberOfElement { get; set; }
        public int[]? Node { get; set; }
    }
}
