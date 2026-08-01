using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Text.Json;
using TournamentManager.Core.Models;
using TournamentManager.Core.Services;

namespace TournamentManager.App.Pages
{
    public partial class LotteryDraw
    {
        private TournamentEngine engine = new TournamentEngine();
        private List<TeamMaster> confirmedTeamsList = new List<TeamMaster>();
        private List<TeamMaster> unassignedTeamsArray = new List<TeamMaster>();
        private int unassignedTeamsCount = 0;
        private string selectedTeamIdForDraw = "";
        private int inputSlotNumber;
        private int totalSlotsCount;
        private string errorMessage = "";

        // 🗂️ 現在選択されている性別タブ
        private string activeGender = "Men";

        protected override void OnInitialized()
        {
            // 画面起動時、Stateから同期的にデータを復元する
            activeGender = !string.IsNullOrEmpty(State.CurrentGender) ? State.CurrentGender : "Men";
            State.CurrentGender = activeGender;

            InitializeTeams();
            RefreshBracket();
        }

        // タブがクリックされた時の非同期処理
        private async Task ChangeGenderTabAsync(string gender)
        {
            activeGender = gender;
            State.CurrentGender = gender; // State側の性別を書き換えることで、GetCurrentSlotAssignments()の取得対象も切り替わります

            // フォーム入力値をクリア
            selectedTeamIdForDraw = "";
            inputSlotNumber = 0;
            errorMessage = "";

            InitializeTeams();
            RefreshBracket();

            await Task.CompletedTask;
        }

        private void InitializeTeams()
        {
            if (State.CurrentTournament == null) return;

            // チームマスタから、現在選択されているチーム、かつ【選択中の性別】のデータを引っ張ってくる
            confirmedTeamsList = State.CurrentTournament.Teams
                .Select(id => State.MasterSettings.Teams.FirstOrDefault(t => t.Id == id))
                .Where(t => t != null && t.Gender == activeGender) // 👈 性別フィルターを適用
                .Select(t => new TeamMaster
                {
                    Id = t!.Id,
                    Name = t.ShortName,
                    AreaName = t.AreaName,
                    Gender = t.Gender
                })
                .ToList();
        }

        private void RefreshBracket()
        {
            var currentSlots = State.GetCurrentSlotAssignments();

            totalSlotsCount = confirmedTeamsList.Count;
            unassignedTeamsArray = confirmedTeamsList
                .Where(t => !currentSlots.ContainsValue(t.Id))
                .ToList();
            unassignedTeamsCount = unassignedTeamsArray.Count;

            // トーナメント生成エンジンにStateのデータを渡して再描画
            engine.BuildStructure(confirmedTeamsList, currentSlots, new Dictionary<string, MatchResult>());
        }

        private async Task AssignTeamToSlotAsync()
        {
            errorMessage = "";
            int targetSlotIndex = inputSlotNumber - 1;

            if (State.CurrentTournament == null) return;
            var currentSlots = State.GetCurrentSlotAssignments();

            if (inputSlotNumber < 1 || inputSlotNumber > totalSlotsCount)
            {
                errorMessage = "枠番号の範囲が不正です。";
                return;
            }
            if (currentSlots.ContainsKey(targetSlotIndex))
            {
                errorMessage = "既にチームが配置されています。";
                return;
            }

            // Stateのオブジェクトを直接更新
            currentSlots[targetSlotIndex] = selectedTeamIdForDraw;
            selectedTeamIdForDraw = "";
            inputSlotNumber = 0;

            // 変更を即座にIndexedDBへ非同期保存！
            await SaveStateToIndexedDbAsync();

            RefreshBracket();
        }

        private async Task ClearAllAssignmentsAsync()
        {
            if (State.CurrentTournament == null) return;

            // 現在選択中の性別のデータ領域のみをクリアしてファイル保存
            State.GetCurrentSlotAssignments().Clear();

            // 変更を即座にIndexedDBへ非同期保存！
            await SaveStateToIndexedDbAsync();

            RefreshBracket();
        }

        /// <summary>
        /// 現在のTournamentStateをシリアライズして、index.html内の saveToIndexedDb を呼び出す
        /// </summary>
        private async Task SaveStateToIndexedDbAsync()
        {
            try
            {
                if (State.CurrentTournament == null) return;

                // State全体、もしくはトーナメントデータをJSON文字列に変換
                // 保存用のストレージキーは、運用に合わせて変更してください (例: "current_tournament_data")
                var jsonText = JsonSerializer.Serialize(State.CurrentTournament);

                // index.html内の JavaScriptファンクションを実行
                await JS.InvokeVoidAsync("saveToIndexedDb", "tournament_storage_key", jsonText);
            }
            catch (Exception ex)
            {
                errorMessage = $"データの自動保存に失敗しました: {ex.Message}";
            }
        }
    }
}
