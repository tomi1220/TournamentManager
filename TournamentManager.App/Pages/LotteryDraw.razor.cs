using Microsoft.AspNetCore.Components.Forms;
using System.Text.Json;
using TournamentManager.Core.Models;

namespace TournamentManager.App.Pages
{
    public partial class LotteryDraw
    {
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
                    activeEntry = (TournamentEntry)importedData;

                    RefreshBracket();
                    await SaveAssignmentsToStorageAsync();
                    fileMessage = "✅ 「" + file.Name + "」から抽選結果を復元しました！";
                }
            }
            catch (Exception ex)
            {
                fileMessage = "❌ インポート失敗: " + ex.Message;
            }
        }

    }
}
