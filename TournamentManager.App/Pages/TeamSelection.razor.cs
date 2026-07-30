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
        private bool isLoaded = false;
        private static System.Collections.ArrayList masterTeams = new System.Collections.ArrayList();
        private System.Collections.Hashtable selectedTeamIdsTable = new System.Collections.Hashtable();
        //private Core.Models.Team[] masterTeamsArray = new Core.Models.Team[0];
        private int selectedTeamIdsCount = 0;
        private string fileMessage = "";

        private TeamMaster[]? masterTeamsArray;

        protected override void OnInitialized()
        {
        }

        // 画面遷移して戻ってきたときに、もし別の画面でIndexedDBが更新されていればここで最新データを再ロードします
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !isLoaded)
            {
                string json = await JS.InvokeAsync<string>("loadFromIndexedDb", "AllTournamentMaster");
                if (!string.IsNullOrEmpty(json))
                {
                    var savedList = JsonSerializer.Deserialize<List<TournamentInfo>>(json);
                    if (savedList != null)
                    {
                        State.SavedTournaments = savedList;

                        // 現在アクティブな大会もリスト内から再捕捉して参照を同期する
                        if (State.CurrentTournament != null)
                        {
                            State.CurrentTournament = State.SavedTournaments.FirstOrDefault(t => t.Id == State.CurrentTournament.Id) ?? State.CurrentTournament;
                        }
                    }
                    isLoaded = true;
                    StateHasChanged();
                }
            }
        }

        private void SwitchGender(string gender)
        {
            State.CurrentGender = gender;
            StateHasChanged();
        }

        private bool IsTeamSelected(string teamId)
        {
            if (State.CurrentTournament == null)
            {
                return false;
            }
            return State.CurrentTournament.Teams.Contains(teamId);
        }

        // チェックボックス操作時に非同期(Async)でメモリ更新とIndexedDBへの即時保存を同時に行う
        private async Task ToggleTeamSelectionAsync(string teamId, object? isChecked)
        {
            if (State.CurrentTournament == null)
            {
                return;
            }

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

            // メモリ上の全大会リスト（最新の選択が反映された状態）をJSONに変換してIndexedDBへ上書き保存！
            string json = JsonSerializer.Serialize(State.SavedTournaments);
            await JS.InvokeVoidAsync("saveToIndexedDb", "AllTournamentMaster", json);
        }

        // 現在選択されているチーム数をカウント
        private int GetSelectedCount()
        {
            if (State.CurrentTournament == null)
            {
                return 0;
            }
            return State.CurrentTournament.Teams
                .Count(id => State.MasterSettings.Teams.Any(t => t.Id == id && t.Gender == State.CurrentGender));
        }

        // トーナメントの枠数計算
        private int CalculateTotalSlots(int teamCount)
        {
            if (teamCount <= 0) return 0;
            int slots = 2;
            while (slots < teamCount) { slots *= 2; }
            return slots;
        }

        private void ConfirmEntries()
        {
            // 次の画面へ遷移（データはStateに保存され、ファイルにも書き込み済みなので安全です）
            Navigation.NavigateTo("/lottery-draw");
        }

    }
}
