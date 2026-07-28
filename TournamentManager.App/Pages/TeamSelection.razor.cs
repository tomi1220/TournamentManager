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
        private Team[] masterTeamsArray = new Team[0];
        private int selectedTeamIdsCount = 0;
        private string fileMessage = "";

        protected override void OnInitialized()
        {
            if (masterTeams.Count == 0)
            {
                LoadMasterTeamsMock();
            }

            // ArrayListをLINQ用にキャストして配列化（HTML側でのGroupBy用）
            masterTeamsArray = masterTeams.Cast<Team>().ToArray();
            LoadCurrentState();
        }

        private void SwitchGender(string gender)
        {
            SaveCurrentState();
            State.CurrentGender = gender;
            LoadCurrentState();
        }

        private bool IsTeamSelected(string teamId)
        {
            //return selectedTeamIdsTable.ContainsKey(teamId);

            if (State.CurrentTournament == null)
            {
                return false;
            }

            // 大会情報モデル(TournamentInfo)の中に選択されたIDリスト(例: SelectedTeamIds)があると仮定
            // もしTournamentStateの直下にList<int>がある場合は `State.SelectedTeamIds.Contains(teamId)` に書き換えてください
            return State.CurrentTournament.Teams.Contains(teamId.ToString());
        }

        private void ToggleTeamSelection(string teamId, object? isChecked)
        {
            //if (isChecked is bool checkedValue && checkedValue)
            //{
            //    if (!selectedTeamIdsTable.ContainsKey(teamId))
            //    {
            //        selectedTeamIdsTable.Add(teamId, true);
            //    }
            //}
            //else
            //{
            //    selectedTeamIdsTable.Remove(teamId);
            //}
            //selectedTeamIdsCount = selectedTeamIdsTable.Count;
            if (State.CurrentTournament == null) return;

            var idStr = teamId.ToString();
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
        }

        // 💡 現在の選択数を取得する便利メソッド
        private int GetSelectedCount()
        {
            return State.CurrentTournament?.Teams?.Count ?? 0;
        }

        private int CalculateTotalSlots(int teamCount)
        {
            int slots = 1;
            while (slots < teamCount) slots *= 2;
            return slots;
        }

        private TournamentEntry SaveCurrentState()
        {
            var activeEntry = State.GetActiveEntry();
            activeEntry.ConfirmedTeams.Clear();

            foreach (Team team in masterTeams)
            {
                if (selectedTeamIdsTable.ContainsKey(team.Id))
                {
                    activeEntry.ConfirmedTeams.Add(team);
                }
            }

            return activeEntry;
        }

        private void LoadCurrentState(TournamentEntry? activeEntry = null)
        {
            selectedTeamIdsTable.Clear();
            if (activeEntry == null)
            {
                activeEntry = State.GetActiveEntry();
            }
            foreach (Team team in activeEntry.ConfirmedTeams)
            {
                if (!selectedTeamIdsTable.ContainsKey(team.Id))
                {
                    selectedTeamIdsTable.Add(team.Id, true);
                }
            }
            selectedTeamIdsCount = selectedTeamIdsTable.Count;
        }

        private void ConfirmEntries()
        {
            SaveCurrentState();
            Navigation.NavigateTo("/lottery-draw");
        }

        private void LoadMasterTeamsMock()
        {
            string[] areas = { "城東地区", "城西地区", "城南地区", "城北地区" };
            int idCounter = 1;

            foreach (var area in areas)
            {
                for (int i = 1; i <= 6; i++)
                {
                    masterTeams.Add(new Team
                    {
                        Id = $"MASTER_T_{idCounter++}",
                        Name = $"{area}第{i}代表クラブ",
                        Area = area
                    });
                }
            }
        }

        private async Task ImportFromJsonAsync(InputFileChangeEventArgs e)
        {
            try
            {
                fileMessage = "";
                var file = e.File;
                if (file == null) return;

                using var stream = file.OpenReadStream(maxAllowedSize: 1024 * 1024);
                using var reader = new System.IO.StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                //var importedData = JsonSerializer.Deserialize<Dictionary<int, string>>(json);
                var importedData = JsonSerializer.Deserialize<TournamentEntry>(json);
                if (importedData != null)
                {
                    // activeEntry.SlotAssignments.Clear();
                    // foreach (var kvp in importedData)
                    // {
                    //     activeEntry.SlotAssignments[kvp.Key] = kvp.Value;
                    // }
                    TournamentEntry activeEntry = activeEntry = (TournamentEntry)importedData;
                    State.CurrentTournament.Teams
                    LoadCurrentState(activeEntry);

                    //await SaveAssignmentsToStorageAsync();
                    fileMessage = "✅ 「" + file.Name + "」から出場チームを復元しました！";
                }
            }
            catch (Exception ex)
            {
                fileMessage = "❌ インポート失敗: " + ex.Message;
            }
        }

        private async Task ExportToJsonAsync()
        {
            try
            {
                TournamentEntry activeEntry = SaveCurrentState();

                fileMessage = "";
                //var json = JsonSerializer.Serialize(activeEntry.SlotAssignments, new JsonSerializerOptions { WriteIndented = true });
                var json = JsonSerializer.Serialize(activeEntry, new JsonSerializerOptions { WriteIndented = true });
                var fileName = $"tournament_slots_{State.CurrentGender}_{DateTime.Now:yyyyMMdd}.json";
                var jsCommand =
                    $@"(function(filename, text) {{" + Environment.NewLine +
                    $@" var element = document.createElement('a');" + Environment.NewLine +
                    $@" element.setAttribute('href', 'data:application/json;charset=utf-8,' + encodeURIComponent(text));" + Environment.NewLine +
                    $@" element.setAttribute('download', filename);" + Environment.NewLine +
                    $@" element.style.display = 'none';" + Environment.NewLine +
                    $@" document.body.appendChild(element);" + Environment.NewLine +
                    $@" element.click();" + Environment.NewLine +
                    $@" document.body.removeChild(element);" + Environment.NewLine +
                    $@"}})('{fileName}', `{json.Replace("`", "\\`").Replace("$", "\\$")}`)";
                await JS.InvokeVoidAsync("eval", jsCommand);
                fileMessage = $"✅ {fileName} をダウンロードしました。";
            }
            catch (Exception ex)
            {
                fileMessage = $"❌ エクスポート失敗: {ex.Message}";
            }
        }

    }
}
