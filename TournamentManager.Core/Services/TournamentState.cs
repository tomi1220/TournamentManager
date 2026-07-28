using System.Collections.Generic;
using TournamentManager.Core.Models;

namespace TournamentManager.Core.Services
{
    public class TournamentState
    {
        // 男子・女子の確定エントリーデータを安全にメモリ保持する
        public TournamentEntry MenEntry { get; } = new TournamentEntry { Gender = "Men" };
        public TournamentEntry WomenEntry { get; } = new TournamentEntry { Gender = "Women" };

        // 現在選択されている性別 ("Men" または "Women")
        public string CurrentGender { get; set; } = "Men";

        // アクティブなエントリーデータを取得するヘルパー
        public TournamentEntry GetActiveEntry()
        {
            return CurrentGender == "Men" ? MenEntry : WomenEntry;
        }
    }
}
