using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentManager.Core.Models
{
    /// <summary>
    /// 大会情報
    /// </summary>
    public class TournamentInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 💡 選択されたチームIDのリスト
        public List<string> Teams { get; set; } = new();

        // 💡 追加：枠番号（キー）と チームID（値）の紐づけ
        // 男子用・女子用で分けて管理します
        public Dictionary<int, string> MenSlotAssignments { get; set; } = new();
        public Dictionary<int, string> WomenSlotAssignments { get; set; } = new();

        public List<MatchResult> MenMatchResults { get; set; } = new();
        public List<MatchResult> WomenMatchResults { get; set; } = new();
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
        // 1) 大会基本名称（例：「市民バスケットボール選手権大会」）
        public string BaseTournamentName { get; set; } = "全日本バスケットボール大会";

        public List<AreaMaster> Areas { get; set; } = new();
        public List<TeamMaster> Teams { get; set; } = new();
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

        // 【新規：勝者ルート用】各線のCSSクラスを保持する変数 ("match-line" または "winner-line")
        public string ClassLineA { get; set; } = "match-line";
        public string ClassLineB { get; set; } = "match-line";
        public string ClassVerticalA { get; set; } = "match-line"; // 中央上側の縦線
        public string ClassVerticalB { get; set; } = "match-line"; // 中央下側の縦線
        public string ClassNextLine { get; set; } = "match-line";  // 次へ進む右横線
    }

    public class SvgTextElement
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string CssClass { get; set; } = "";
        public string Content { get; set; } = "";
    }

}
