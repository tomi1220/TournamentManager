using System.Text.Json;
//using Microsoft.JSInterop;
using TournamentManager.Core.Models;

namespace TournamentManager.Core.Services
{
    public class TournamentState
    {
        //private readonly IJSRuntime _js;

        // 現在アクティブ（選択中・編集中）の大会情報
        //public List<TournamentInfo> SavedTournaments { get; set; } = new();
        public TournamentInfo? CurrentTournament { get; set; }

        // 保存されている過去の大会リスト（簡易的なモックデータ）
        public List<TournamentInfo> SavedTournaments { get; set; } = new()
        {
            new TournamentInfo { Name = "2026年 春季地区大会", Teams = new() { "チームA", "チームB", "チームC" } },
            new TournamentInfo { Name = "第5回 社内バスケ杯", Teams = new() { "シャークス", "レイカーズ" } }
        };

        //// コンストラクタでIJSRuntimeを注入
        //public TournamentState(IJSRuntime js)
        //{
        //    _js = js;
        //}

        // 新規大会を作成してアクティブにするメソッド
        public async Task CreateNewTournamentAsync(string name)
        {
            var newTournament = new TournamentInfo { Name = name };
            SavedTournaments.Add(newTournament);
            CurrentTournament = newTournament;
            await SaveToStorageAsync(); // 追加したら即保存
        }

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

        // 最後に選択されていたチームのリストを保持する
        public List<string> SelectedTeams { get; set; } = new();

        // 例: その他、残したい画面の状態（テキストボックスに入力中だった文字など）
        public string CurrentSearchQuery { get; set; } = "";
    }
}
