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

            // 大会オブジェクト内の Teams リストにこのマスタIDが含まれているかで判定
            return State.CurrentTournament.Teams.Contains(teamId);
        }

        // 💡 チェックボックスを操作した時、Stateのデータを直接更新し、その場でファイル保存する
        private void ToggleTeamSelection(string teamId, object? isChecked)
        {
            if (State.CurrentTournament == null) return;

            if (isChecked is true)
            {
                if (!State.CurrentTournament.Teams.Contains(teamId))
                {
                    State.CurrentTournament.Teams.Add(teamId);
                }
            }
            else
            {
                State.CurrentTournament.Teams.Remove(teamId);
            }

            // 💡 変更を即座に「tournament_storage.json」に自動セーブ
            State.SaveToFile();
        }

        // 💡 現在選択されているチーム数をカウント
        private int GetSelectedCount()
        {
            if (State.CurrentTournament == null) return 0;

            // 大会に登録された全IDのうち、現在選択されている性別に一致するマスタチームのみをカウント
            return State.CurrentTournament.Teams
                .Count(id => State.MasterSettings.Teams.Any(t => t.Id == id && t.Gender == State.CurrentGender));
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
            // 💡 確定して次の抽選画面へ（データは永続化されているので安全に遷移可能）
            Navigation.NavigateTo("/lottery-draw");
        }

    }
}
