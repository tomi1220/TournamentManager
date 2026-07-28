using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TournamentManager.Core.Models;

namespace TournamentManager.Core.Services
{
    public class TournamentState
    {
        // 💾 保存先ファイルのパス（アプリの実行フォルダ直下に保存されます）
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tournament_storage.json");

        // 現在操作中（アクティブ）の大会情報
        public TournamentInfo? CurrentTournament { get; set; }

        // 大会一覧リスト
        public List<TournamentInfo> SavedTournaments { get; set; } = new();

        // 現在選択されている性別タブの状態管理（画面遷移で消えないようにここに保持）
        public string CurrentGender { get; set; } = "Men";

        public TournamentState()
        {
            // アプリ起動時に自動でファイルを読み込む
            LoadFromFile();
        }

        // 📂 ファイルからデータを読み込む
        public void LoadFromFile()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    SavedTournaments = JsonSerializer.Deserialize<List<TournamentInfo>>(json) ?? new();
                }
            }
            catch (Exception ex)
            {
                // エラー時はログ代わりにコンソールへ出力し、空のリストで初期化
                Console.WriteLine($"ファイルの読み込みに失敗しました: {ex.Message}");
                SavedTournaments = new();
            }
        }

        // 💾 ファイルへデータを上書き保存する
        public void SaveToFile()
        {
            try
            {
                // インデントを揃えて見やすくJSON化
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(SavedTournaments, options);

                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ファイルの保存に失敗しました: {ex.Message}");
            }
        }

        // 🆕 新しい大会を作成して保存する
        public void CreateNewTournament(string name)
        {
            var newTournament = new TournamentInfo { Name = name };
            SavedTournaments.Add(newTournament);
            CurrentTournament = newTournament;

            SaveToFile(); // 追加したら即ファイルに保存
        }

        public Dictionary<int, string> GetCurrentSlotAssignments()
        {
            if (CurrentTournament == null) return new();
            return CurrentGender == "Men" ? CurrentTournament.MenSlotAssignments : CurrentTournament.WomenSlotAssignments;
        }

        public List<MatchResult> GetCurrentMatchResults()
        {
            if (CurrentTournament == null) return new();
            return CurrentGender == "Men" ? CurrentTournament.MenMatchResults : CurrentTournament.WomenMatchResults;
        }
    }
}
