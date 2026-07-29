using System;
using System.Collections.Generic;
using System.Linq;
using TournamentManager.Core.Models;

namespace TournamentManager.Core.Services
{
    public class TournamentEngine
    {
        public int TotalSlots { get; private set; }
        public List<MatchNode> Matches { get; } = new List<MatchNode>();
        public List<SvgTextElement> SvgTexts { get; } = new List<SvgTextElement>();

        private const double SvgHeight = 1200;
        private const double RoundWidth = 160;
        private const double LeftMargin = 120;

        public void BuildStructure(List<TeamMaster> activeTeams, Dictionary<int, string> slotAssignments, Dictionary<string, MatchResult> matchResults)
        {
            Matches.Clear();
            SvgTexts.Clear();

            int n = activeTeams.Count;
            if (n < 2) return;

            TotalSlots = 1;
            while (TotalSlots < n) TotalSlots *= 2;
            int seedCount = TotalSlots - n;

            List<int> order = GetTournamentOrder(TotalSlots);
            bool[] isSeedSlotInternal = new bool[TotalSlots];
            for (int i = 0; i < seedCount; i++)
            {
                isSeedSlotInternal[order[TotalSlots - 1 - i]] = true;
            }

            double yGap = SvgHeight / (n + 1);

            double[] internalSlotToYMap = new double[TotalSlots];
            int actualRowCounter = 0;

            for (int i = 0; i < TotalSlots; i++)
            {
                if (isSeedSlotInternal[i])
                {
                    internalSlotToYMap[i] = -1;
                }
                else
                {
                    double y = (actualRowCounter + 1) * yGap;
                    internalSlotToYMap[i] = y;

                    SvgTexts.Add(
                        new SvgTextElement { X = LeftMargin - 20, Y = y + 4,
                            CssClass = "slot-number",
                            Content = (actualRowCounter + 1).ToString() });

                    if (slotAssignments.TryGetValue(actualRowCounter, out var teamId) && !string.IsNullOrEmpty(teamId))
                    {
                        var team = activeTeams.FirstOrDefault(t => t.Id == teamId);
                        if (team != null)
                        {
                            SvgTexts.Add(new SvgTextElement { X = LeftMargin + 5, Y = y - 5, CssClass = "team-name", Content = team.Name });
                            SvgTexts.Add(new SvgTextElement { X = LeftMargin + 5, Y = y + 10, CssClass = "area-name", Content = team.AreaName });
                        }
                    }
                    actualRowCounter++;
                }
            }

            for (int i = 0; i < TotalSlots / 2; i++)
            {
                int idxA = i * 2;
                int idxB = i * 2 + 1;
                if (internalSlotToYMap[idxA] < 0) internalSlotToYMap[idxA] = internalSlotToYMap[idxB];
                if (internalSlotToYMap[idxB] < 0) internalSlotToYMap[idxB] = internalSlotToYMap[idxA];
            }

            string?[] currentSlotTeams = new string?[TotalSlots];
            int teamAssignIndex = 0;
            for (int i = 0; i < TotalSlots; i++)
            {
                if (isSeedSlotInternal[i]) currentSlotTeams[i] = "SEED";
                else slotAssignments.TryGetValue(teamAssignIndex++, out currentSlotTeams[i]);
            }

            int totalRounds = (int)Math.Log2(TotalSlots);
            int currentMatchCount = TotalSlots / 2;
            double currentX = LeftMargin + RoundWidth;

            // 勝ち上がり線を追跡するための前ラウンドの勝者フラグ配列
            // 内部スロット単位で「そのルートが現在勝利で選択されているか」を保持
            bool[] upperLineHighlighted = new bool[TotalSlots];
            for (int i = 0; i < TotalSlots; i++)
            {
                // 最初（1回戦の土台）はすべて通常
                upperLineHighlighted[i] = false;
            }

            for (int r = 0; r < totalRounds; r++)
            {
                double[] nextYPositions = new double[currentMatchCount];
                string?[] nextSlotTeams = new string?[currentMatchCount];
                bool[] nextLineHighlighted = new bool[currentMatchCount];

                for (int i = 0; i < currentMatchCount; i++)
                {
                    int idxA = i * 2;
                    int idxB = i * 2 + 1;

                    double yA = internalSlotToYMap[idxA];
                    double yB = internalSlotToYMap[idxB];
                    double yMid = (yA + yB) / 2;
                    string mId = $"M_{r}_{i}";

                    bool isSeedA = (currentSlotTeams[idxA] == "SEED");
                    bool isSeedB = (currentSlotTeams[idxB] == "SEED");

                    var match = new MatchNode
                    {
                        MatchId = mId,
                        Round = r,
                        Position = i,
                        IsSeedA = isSeedA,
                        IsSeedB = isSeedB,
                        X = currentX,
                        ParentAY = yA,
                        ParentBY = yB,
                        Y = yMid,
                        LineStartXA = currentX - RoundWidth,
                        LineStartXB = currentX - RoundWidth
                    };

                    if (!isSeedA && !string.IsNullOrEmpty(currentSlotTeams[idxA]))
                        match.TeamA = activeTeams.FirstOrDefault(t => t.Id == currentSlotTeams[idxA]);
                    if (!isSeedB && !string.IsNullOrEmpty(currentSlotTeams[idxB]))
                        match.TeamB = activeTeams.FirstOrDefault(t => t.Id == currentSlotTeams[idxB]);

                    if (matchResults.TryGetValue(mId, out var res))
                    {
                        match.ScoreA = res.ScoreA;
                        match.ScoreB = res.ScoreB;
                        match.WinnerTeamId = res.WinnerTeamId;
                    }

                    // 勝者進出ロジック
                    bool hasWinner = false;
                    bool isWinnerA = false;

                    if (isSeedA && match.TeamB != null)
                    {
                        match.WinnerTeamId = match.TeamB.Id;
                        nextSlotTeams[i] = match.TeamB.Id;
                        hasWinner = true; isWinnerA = false; // B(下側)が不戦勝
                    }
                    else if (isSeedB && match.TeamA != null)
                    {
                        match.WinnerTeamId = match.TeamA.Id;
                        nextSlotTeams[i] = match.TeamA.Id;
                        hasWinner = true; isWinnerA = true;  // A(上側)が不戦勝
                    }
                    else if (!string.IsNullOrEmpty(match.WinnerTeamId))
                    {
                        nextSlotTeams[i] = match.WinnerTeamId;
                        hasWinner = true;
                        isWinnerA = (match.WinnerTeamId == match.TeamA?.Id);
                    }
                    else
                    {
                        nextSlotTeams[i] = null;
                    }

                    // --- 【重要】赤線（ハイライト）クラスの割り当てロジック ---
                    // 1. 過去のラウンドから勝ち上がってきた赤線をそのまま引き継いで横線を赤くする
                    if (r > 0)
                    {
                        if (upperLineHighlighted[idxA]) match.ClassLineA = "winner-line";
                        if (upperLineHighlighted[idxB]) match.ClassLineB = "winner-line";
                    }
                    // 1回戦が不戦勝(シード)の場合、最初からその1本線は勝者ルートなので赤くする
                    if (r == 1)
                    {
                        bool p1Seed = isSeedSlotInternal[idxA * 2] || isSeedSlotInternal[idxA * 2 + 1];
                        bool p2Seed = isSeedSlotInternal[idxB * 2] || isSeedSlotInternal[idxB * 2 + 1];
                        if (p1Seed) match.ClassLineA = "winner-line";
                        if (p2Seed) match.ClassLineB = "winner-line";
                    }

                    // 2. この試合で勝者が決まった場合、合流縦線と、次のラウンドへ向かう横線を赤くする
                    if (hasWinner)
                    {
                        if (isWinnerA)
                        {
                            match.ClassLineA = "winner-line"; // 上側チームの横線を赤に
                            match.ClassVerticalA = "winner-line"; // 縦線の上半分を赤に
                        }
                        else
                        {
                            match.ClassLineB = "winner-line"; // 下側チームの横線を赤に
                            match.ClassVerticalB = "winner-line"; // 縦線の下半分を赤に
                        }
                        match.ClassNextLine = "winner-line"; // 次に進む横線を赤に
                        nextLineHighlighted[i] = true; // 次ラウンドへフラグをリレー
                    }
                    else
                    {
                        nextLineHighlighted[i] = false;
                    }
                    if (r == 0)
                    {
                        if (isSeedA || isSeedB)
                        {
                            nextYPositions[i] = yMid;
                            // シードで自動通過した場合はそのルートをあらかじめハイライト化しておく
                            nextLineHighlighted[i] = true;
                            continue;
                        }
                    }
                    else if (r == 1)
                    {
                        bool parent1WasSeed = isSeedSlotInternal[idxA * 2] || isSeedSlotInternal[idxA * 2 + 1];
                        bool parent2WasSeed = isSeedSlotInternal[idxB * 2] || isSeedSlotInternal[idxB * 2 + 1];
                        if (parent1WasSeed) match.LineStartXA = LeftMargin;
                        if (parent2WasSeed) match.LineStartXB = LeftMargin;
                    }
                    if (match.ScoreA.HasValue && match.ScoreB.HasValue)
                    {
                        SvgTexts.Add(new SvgTextElement { X = match.X - 15, Y = match.ParentAY - 6, CssClass = "score-text", Content = match.ScoreA.ToString()! });
                        SvgTexts.Add(new SvgTextElement { X = match.X - 15, Y = match.ParentBY - 6, CssClass = "score-text", Content = match.ScoreB.ToString()! });
                    }
                    Matches.Add(match);
                    nextYPositions[i] = yMid;
                }
                internalSlotToYMap = nextYPositions;
                currentSlotTeams = nextSlotTeams;
                upperLineHighlighted = nextLineHighlighted; // ハイライト状態を次ラウンドへ移行
                currentMatchCount /= 2;
                currentX += RoundWidth;
            }
        }
        private List<int> GetTournamentOrder(int totalSlots)
        {
            List<int> order = new List<int> { 0 };
            while (order.Count < totalSlots)
            {
                int len = order.Count;
                List<int> next = new List<int>();
                foreach (int val in order)
                {
                    next.Add(val);
                    next.Add(len * 2 - 1 - val);
                }
                order = next;
            }
            return order;
        }
    }
}

