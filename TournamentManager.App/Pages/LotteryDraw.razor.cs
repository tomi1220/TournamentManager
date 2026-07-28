using Microsoft.AspNetCore.Components.Forms;
using System.Text.Json;
using TournamentManager.Core.Models;
using TournamentManager.Core.Services;

namespace TournamentManager.App.Pages
{
    public partial class LotteryDraw
    {
        private TournamentEngine engine = new TournamentEngine();
        private List<Team> confirmedTeamsList = new List<Team>();
        private List<Team> unassignedTeamsArray = new List<Team>();
        private int unassignedTeamsCount = 0;
        private string selectedTeamIdForDraw = "";
        private int inputSlotNumber;
        private int totalSlotsCount;
        private string errorMessage = "";

        protected override void OnInitialized()
        {
            // 💡 画面起動時、Stateから同期的にデータを復元する
            InitializeTeams();
            RefreshBracket();
        }

        private void InitializeTeams()
        {
            if (State.CurrentTournament == null) return;

            // ※前画面の masterTeamsArray と同様に、本来はマスターデータ等から
            // 「現在選択されているチームID一覧」に合致するTeamオブジェクトのリストを生成してください。
            // ここでは仮で、Stateに保存されているIDを元にTeamオブジェクトを作成しています。
            confirmedTeamsList = State.CurrentTournament.Teams
                .Select(id => new Team { Id = id, Name = $"チーム {id}", Area = "地区" })
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
            // (MatchResultsは仮で空の辞書を渡しています。必要に応じてモデルに追加してください)
            engine.BuildStructure(confirmedTeamsList, currentSlots, new Dictionary<string, MatchResult>());
        }

        private void AssignTeamToSlot()
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

            // 💡 Stateのオブジェクトを直接更新
            currentSlots[targetSlotIndex] = selectedTeamIdForDraw;
            selectedTeamIdForDraw = "";
            inputSlotNumber = 0;

            // 💡 更新されたデータを即座にJSONファイルへ保存！
            State.SaveToFile();

            RefreshBracket();
        }

        private void ClearAllAssignments()
        {
            if (State.CurrentTournament == null) return;

            // 💡 データをクリアしてファイル保存
            State.GetCurrentSlotAssignments().Clear();
            State.SaveToFile();

            RefreshBracket();
        }
    }
}
