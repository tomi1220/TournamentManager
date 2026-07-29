using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Text.Json;
using TournamentManager.Core.Models;
//Inject IJSRuntime JS;

namespace TournamentManager.App.Pages
{
    public partial class TeamSelection
    {
        private static System.Collections.ArrayList masterTeams = new System.Collections.ArrayList();
        private System.Collections.Hashtable selectedTeamIdsTable = new System.Collections.Hashtable();
        //private Core.Models.Team[] masterTeamsArray = new Core.Models.Team[0];
        private int selectedTeamIdsCount = 0;
        private string fileMessage = "";

        private TeamMaster[]? masterTeamsArray;

        protected override void OnInitialized()
        {
            // テスト用のダミーマスターデータ（本物の読み込みロジックがあればそちらに差し替えてください）
            masterTeamsArray = new TeamMaster[]
            {
                new TeamMaster { Id = "1", Name = "シャークス", AreaName = "北部" },
                new TeamMaster { Id = "2", Name = "レイカーズ", AreaName = "北部" },
                new TeamMaster { Id = "3", Name = "ブルズ", AreaName = "南部" },
                new TeamMaster { Id = "4", Name = "ウォリアーズ", AreaName = "南部" },
                new TeamMaster { Id = "5", Name = "セルティックス", AreaName = "東部" },
                new TeamMaster { Id = "6", Name = "ネッツ", AreaName = "西部" }
            };
        }

        private void SwitchGender(string gender)
        {
            // 💡 性別の選択状態も State 側に持たせることで画面遷移で消えなくなります
            State.CurrentGender = gender;
            StateHasChanged();
        }

        // 💡 チームが選択されているかを State (大会データ) から判定
        private bool IsTeamSelected(string teamId)
        {
            if (State.CurrentTournament == null) return false;

            // 選択されたIDを文字列として保持していると仮定
            return State.CurrentTournament.Teams.Contains(teamId);
        }

        // 💡 チェックボックスを操作した時、Stateのデータを直接更新し、その場でファイル保存する
        private void ToggleTeamSelection(string teamId, object? isChecked)
        {
            if (State.CurrentTournament == null) return;

            var idStr = teamId;
            if (isChecked is true)
            {
                if (!State.CurrentTournament.Teams.Contains(idStr))
                {
                    State.CurrentTournament.Teams.Add(idStr);
                }
            }
            else
            {
                State.CurrentTournament.Teams.Remove(idStr);
            }

            // 💡 変更を即座に「tournament_storage.json」に上書き保存！
            State.SaveToFile();
        }

        // 💡 現在選択されているチーム数をカウント
        private int GetSelectedCount()
        {
            return State.CurrentTournament?.Teams?.Count ?? 0;
        }

        // 💡 トーナメントの枠数計算
        private int CalculateTotalSlots(int teamCount)
        {
            if (teamCount <= 0) return 0;
            int slots = 2;
            while (slots < teamCount) { slots *= 2; }
            return slots;
        }

        private void ConfirmEntries()
        {
            // 💡 次の画面へ遷移（データはStateに保存され、ファイルにも書き込み済みなので安全です）
            Navigation.NavigateTo("/lottery-draw");
        }

    }
}
