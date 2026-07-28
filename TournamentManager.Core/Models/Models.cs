using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentManager.Core.Models
{
    public class Team
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Area { get; set; } = "";
    }

    public class TournamentEntry
    {
        public string Gender { get; set; } = "Men";
        public List<Team> ConfirmedTeams { get; set; } = new List<Team>();
        public Dictionary<int, string> SlotAssignments { get; set; } = new Dictionary<int, string>();
        public Dictionary<string, MatchResult> MatchResults { get; set; } = new Dictionary<string, MatchResult>();
    }

    public class MatchResult
    {
        public int? ScoreA { get; set; }
        public int? ScoreB { get; set; }
        public string? WinnerTeamId { get; set; }
    }

    public class MatchNode
    {
        public string MatchId { get; set; } = "";
        public int Round { get; set; }
        public int Position { get; set; }
        public Team? TeamA { get; set; }
        public Team? TeamB { get; set; }
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
