using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using TournamentManager.Core.Models;

namespace TournamentManager.Core.Services
{
    public class TournamentState
    {
        public string DummyTournamentName = "U12バスケットボール大会";

        public readonly string DbKeyAllTournamentData = "AllTournamentData";
        public readonly string DbKeyAllTournamentMaster = "AllTournamentMaster";
        public readonly string DbKeyCurrentTournamentData = "CurrentTournamentData";

        // 男子・女子の確定エントリーデータを安全にメモリ保持する
        public TournamentEntry MenEntry { get; } = new TournamentEntry { Gender = "Men" };
        public TournamentEntry WomenEntry { get; } = new TournamentEntry { Gender = "Women" };

        // 現在操作中（アクティブ）の大会情報
        public TournamentInfo? CurrentTournament { get; set; }

        // 大会一覧リスト
        public List<TournamentInfo> SavedTournaments { get; set; } = new();

        // 現在選択されている性別タブの状態管理（画面遷移で消えないようにここに保持）
        public string CurrentGender { get; set; } = "Men";

        // 追加：マスター設定データ
        //public List<TournamentNameMaster>? MasterTournamentNames { get; set; } = new();
        //public List<AreaMaster>? MasterAreas { get; set; } = new();
        //public List<TeamMaster>? MasterTeams { get; set; } = new();
        public AppMasterSettings MasterSettings { get; set; } = new();

        // インポート時に「作業中だった大会」を自動で復元するための手がかり
        public Guid? LastActiveTournamentId { get; set; }

        public TournamentState()
        {
        }

        public bool InitializeMaster()
        {
            // 初回起動時、マスタが空なら初期データを入れておく
            bool addMasterItem = false;
            if (!MasterSettings.TournamentNames.Any())
            {
                MasterSettings.TournamentNames.Add(new TournamentNameMaster { Name = "VAYoreLA cup 鹿児島県U12バスケットボール大会" });
                MasterSettings.TournamentNames.Add(new TournamentNameMaster { Name = "プラッセ＆だいわカップＭＢＣ小学生バスケットボール大会" });
                MasterSettings.TournamentNames.Add(new TournamentNameMaster { Name = "Ｕ１２県下バスケットボール選手権大会" });
                addMasterItem = true;
            }
            if (!MasterSettings.Teams.Any())
            {
                MasterSettings.Areas.Add(new AreaMaster { Name = "鹿児島北" });
                MasterSettings.Areas.Add(new AreaMaster { Name = "鹿児島西" });
                MasterSettings.Areas.Add(new AreaMaster { Name = "鹿児島南" });
                MasterSettings.Areas.Add(new AreaMaster { Name = "鹿児島中央" });
                MasterSettings.Areas.Add(new AreaMaster { Name = "北薩川内" });
                MasterSettings.Areas.Add(new AreaMaster { Name = "北薩出水" });
                MasterSettings.Areas.Add(new AreaMaster { Name = "姶良" });
                MasterSettings.Areas.Add(new AreaMaster { Name = "肝属" });
                MasterSettings.Areas.Add(new AreaMaster { Name = "日置・南薩" });
                MasterSettings.Areas.Add(new AreaMaster { Name = "大島" });
                addMasterItem = true;
            }
            if (!MasterSettings.Teams.Any())
            {
                MasterSettings.Teams.Add(new TeamMaster { Id = "1", Name = "伊敷ミニバスケットボールスポーツ少年団", ShortName = "伊敷", AreaName = "鹿児島北", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "2", Name = "川上ミニバスケットボールスポーツ少年団", ShortName = "川上", AreaName = "鹿児島北", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "3", Name = "ＣＲＥＳＴ", ShortName = "ＣＲＥＳＴ", AreaName = "鹿児島北", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "4", Name = "花野バスケットボールスポーツ少年団", ShortName = "花野", AreaName = "鹿児島北", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "5", Name = "西伊敷ミニバスケットボールスポーツ少年団", ShortName = "西伊敷", AreaName = "鹿児島北", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "6", Name = "吉野東ミニバスケットボール少年団", ShortName = "吉野東", AreaName = "鹿児島北", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "7", Name = "吉野ミニバスケットボールスポーツ少年団", ShortName = "吉野", AreaName = "鹿児島北", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "8", Name = "SEED", ShortName = "ＳＥＥＤ", AreaName = "鹿児島北", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "9", Name = "BlackSailsJr.", ShortName = "ＢＳＪ", AreaName = "鹿児島西", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "10", Name = "西陵ミニバスケットボールクラブ", ShortName = "西陵", AreaName = "鹿児島西", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "11", Name = "武岡ＪＢＳ", ShortName = "武岡", AreaName = "鹿児島西", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "12", Name = "紫原ミニバスケットボールクラブ", ShortName = "紫原", AreaName = "鹿児島西", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "13", Name = "松元ミニバスケットボール少年団", ShortName = "松元", AreaName = "鹿児島西", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "14", Name = "アローズ", ShortName = "アローズ", AreaName = "鹿児島西", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "15", Name = "中山GOLDEN　STARS Jr.", ShortName = "中山", AreaName = "鹿児島西", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "16", Name = "LANI BASKETBALL CLUB", ShortName = "ＬＡＮＩ", AreaName = "鹿児島西", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "17", Name = "桜丘ドリームゲッターズ", ShortName = "桜丘", AreaName = "鹿児島南", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "18", Name = "谷山RED BURNINGS 男子", ShortName = "谷山", AreaName = "鹿児島南", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "19", Name = "東谷山ミニバスケット", ShortName = "東谷山", AreaName = "鹿児島南", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "20", Name = "ＭＥＥＫ", ShortName = "ＭＥＥＫ", AreaName = "鹿児島南", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "21", Name = "福平ミニバスケットボールスポーツ少年団", ShortName = "福平", AreaName = "鹿児島南", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "22", Name = "広木ﾐﾆﾊﾞｽｹｯﾄﾎﾞｰﾙｸﾗﾌﾞ", ShortName = "広木", AreaName = "鹿児島南", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "23", Name = "Courage", ShortName = "Ｃｏｕｒａｇｅ", AreaName = "鹿児島南", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "24", Name = "和田AIRKIDS", ShortName = "和田", AreaName = "鹿児島南", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "25", Name = "宇宿レッドファルコンズ", ShortName = "宇宿", AreaName = "鹿児島中央", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "26", Name = "鴨池ミニバスケットボールスポーツ少年団", ShortName = "鴨池", AreaName = "鹿児島中央", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "27", Name = "清水ミニバスケットボールスポーツ少年団", ShortName = "清水", AreaName = "鹿児島中央", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "28", Name = "大龍ＲＩＳＩＮＧＳＵＮ", ShortName = "大龍", AreaName = "鹿児島中央", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "29", Name = "武バスケットボールスポーツ少年団", ShortName = "武", AreaName = "鹿児島中央", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "30", Name = "ラピスグリフィンズ", ShortName = "ラピスグリフィンズ", AreaName = "鹿児島中央", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "31", Name = "西田 WILD GIRAFFES", ShortName = "西田", AreaName = "鹿児島中央", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "32", Name = "八幡ミニバスケットボールスポーツ少年団", ShortName = "八幡", AreaName = "鹿児島中央", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "33", Name = "平佐西ミニバスケットボールクラブ", ShortName = "平佐西", AreaName = "北薩川内", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "34", Name = "盈進ミニバスケットボールスポーツ少年団", ShortName = "盈進", AreaName = "北薩川内", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "35", Name = "可愛ミニバスケットボールクラブ", ShortName = "可愛", AreaName = "北薩川内", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "36", Name = "亀山ミニバスケットボールクラブ", ShortName = "亀山", AreaName = "北薩川内", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "37", Name = "隈之城BRAVE BOYS", ShortName = "隈之城", AreaName = "北薩川内", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "38", Name = "永利BTS", ShortName = "永利", AreaName = "北薩川内", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "39", Name = "川内GREENBACKS", ShortName = "川内", AreaName = "北薩川内", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "40", Name = "FLYERS", ShortName = "ＦＬＹＥＲＳ", AreaName = "北薩出水", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "41", Name = "脇本UNITY", ShortName = "脇本", AreaName = "北薩出水", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "42", Name = "ノヴァアウルズ", ShortName = "ノヴァアウルズ", AreaName = "北薩出水", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "43", Name = "阿久根バスケットボール少年団", ShortName = "阿久根", AreaName = "北薩出水", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "44", Name = "姶良ＷＩＮＧＳ", ShortName = "姶良", AreaName = "姶良", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "45", Name = "加治木・柁城ミニバスケットボールスポーツ少年団", ShortName = "加治木柁城", AreaName = "姶良", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "46", Name = "蒲生カンファーズJr", ShortName = "蒲生", AreaName = "姶良", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "47", Name = "国分シューティングスターズ", ShortName = "国分", AreaName = "姶良", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "48", Name = "隼人ファルコンズ", ShortName = "隼人", AreaName = "姶良", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "49", Name = "きりしま舞鶴スポーツクラブ", ShortName = "きりしま舞鶴", AreaName = "姶良", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "50", Name = "霧島UNITE", ShortName = "霧島ＵＮＩＴＥ", AreaName = "姶良", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "51", Name = "Laugh", ShortName = "Ｌａｕｇｈ", AreaName = "姶良", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "52", Name = "B.B.B Jr", ShortName = "Ｂ．Ｂ．Ｂ Ｊｒ", AreaName = "姶良", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "53", Name = "鹿屋ﾐﾆﾊﾞｽｹｯﾄﾎﾞｰﾙ", ShortName = "鹿屋", AreaName = "肝属", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "54", Name = "高山ミニバスケットボールスポーツ少年団", ShortName = "高山", AreaName = "肝属", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "55", Name = "寿北バスケットボールクラブ", ShortName = "寿北", AreaName = "肝属", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "56", Name = "ネクサス鹿屋", ShortName = "ネクサス鹿屋", AreaName = "肝属", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "57", Name = "東串良ＢＢＣ", ShortName = "東串良ＢＢＣ", AreaName = "肝属", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "58", Name = "BLAX", ShortName = "ＢＬＡＸ", AreaName = "肝属", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "59", Name = "TROWRE", ShortName = "ＴＲＯＷＲＥ", AreaName = "日置・南薩", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "60", Name = "AXLS_12", ShortName = "AXLS_12", AreaName = "日置・南薩", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "61", Name = "川辺ミニバスケットボールスポーツ少年団", ShortName = "川辺", AreaName = "日置・南薩", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "62", Name = "LAULE'A　TANBAバスケットボールクラブ", ShortName = "ＬＡＵＬＥ’Ａ", AreaName = "日置・南薩", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "63", Name = "いぶすきBC", ShortName = "いぶすき", AreaName = "日置・南薩", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "64", Name = "日置REX", ShortName = "日置ＲＥＸ", AreaName = "日置・南薩", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "65", Name = "妙円寺ミニバスケットボールスポーツ少年団", ShortName = "妙円寺", AreaName = "日置・南薩", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "66", Name = "LEPUS SAKURAYAMA", ShortName = "ＬＥＰＵＳ ＳＡＫＵＲＡＹＡＭＡ", AreaName = "日置・南薩", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "67", Name = "枕崎バスケットボールクラブ", ShortName = "枕崎", AreaName = "日置・南薩", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "68", Name = "知覧小ﾐﾆﾊﾞｽｹｯﾄﾎﾞｰﾙ", ShortName = "知覧", AreaName = "日置・南薩", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "69", Name = "PRIDE", ShortName = "ＰＲＩＤＥ", AreaName = "日置・南薩", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "70", Name = "朝日ミニバスケットボールスポーツ少年団", ShortName = "朝日", AreaName = "大島", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "71", Name = "小宿ミニバスケットボールスポーツ少年団", ShortName = "小宿", AreaName = "大島", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "72", Name = "名瀬ミニバスケットボールスポーツ少年団", ShortName = "名瀬", AreaName = "大島", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "73", Name = "奄美ブラックラビッツ", ShortName = "奄美", AreaName = "大島", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "74", Name = "古仁屋RED ＧＲＯＷＳ", ShortName = "古仁屋", AreaName = "大島", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "75", Name = "伊津部", ShortName = "伊津部", AreaName = "大島", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "76", Name = "沖永良部Stor Whale's", ShortName = "沖永良部", AreaName = "大島", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "77", Name = "徳之島町ﾐﾆﾊﾞｽｹｯﾄﾎﾞｰﾙｽﾎﾟｰﾂ少年団", ShortName = "徳之島", AreaName = "大島", Gender = "Men" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "101", Name = "伊敷ミニバスケットボールスポーツ少年団", ShortName = "伊敷", AreaName = "鹿児島北", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "102", Name = "川上ミニバスケットボールスポーツ少年団", ShortName = "川上", AreaName = "鹿児島北", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "103", Name = "ＣＲＥＳＴ", ShortName = "ＣＲＥＳＴ", AreaName = "鹿児島北", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "104", Name = "花野バスケットボールスポーツ少年団", ShortName = "花野", AreaName = "鹿児島北", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "105", Name = "西伊敷ミニバスケットボールスポーツ少年団", ShortName = "西伊敷", AreaName = "鹿児島北", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "106", Name = "吉野東ミニバスケットボール少年団", ShortName = "吉野東", AreaName = "鹿児島北", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "107", Name = "吉野ミニバスケットボールスポーツ少年団", ShortName = "吉野", AreaName = "鹿児島北", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "108", Name = "明和（休止）", ShortName = "明和", AreaName = "鹿児島西", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "109", Name = "西陵ミニバスケットボールクラブ", ShortName = "西陵", AreaName = "鹿児島西", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "110", Name = "武岡ＪＢＳ", ShortName = "武岡", AreaName = "鹿児島西", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "111", Name = "紫原ミニバスケットボールクラブ", ShortName = "紫原", AreaName = "鹿児島西", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "112", Name = "松元ミニバスケットボール少年団", ShortName = "松元", AreaName = "鹿児島西", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "113", Name = "アローズ", ShortName = "アローズ", AreaName = "鹿児島西", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "114", Name = "中山GOLDEN　STARS Jr.", ShortName = "中山", AreaName = "鹿児島西", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "115", Name = "LANI BASKETBALL CLUB", ShortName = "ＬＡＮＩ", AreaName = "鹿児島西", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "116", Name = "桜丘ドリームゲッターズ", ShortName = "桜丘", AreaName = "鹿児島南", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "117", Name = "谷山RED BURNINGS 女子", ShortName = "谷山", AreaName = "鹿児島南", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "118", Name = "東谷山ミニバスケット", ShortName = "東谷山", AreaName = "鹿児島南", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "119", Name = "ＭＥＥＫ", ShortName = "ＭＥＥＫ", AreaName = "鹿児島南", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "120", Name = "福平ミニバスケットボールスポーツ少年団", ShortName = "福平", AreaName = "鹿児島南", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "121", Name = "広木ﾐﾆﾊﾞｽｹｯﾄﾎﾞｰﾙｸﾗﾌﾞ", ShortName = "広木", AreaName = "鹿児島南", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "122", Name = "Courage", ShortName = "Ｃｏｕｒａｇｅ", AreaName = "鹿児島南", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "123", Name = "和田AIRKIDS", ShortName = "和田", AreaName = "鹿児島南", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "124", Name = "宇宿レッドファルコンズ", ShortName = "宇宿", AreaName = "鹿児島中央", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "125", Name = "鴨池", ShortName = "鴨池", AreaName = "鹿児島中央", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "126", Name = "清水ミニバスケットボールスポーツ少年団", ShortName = "清水", AreaName = "鹿児島中央", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "127", Name = "武バスケットボールスポーツ少年団", ShortName = "武", AreaName = "鹿児島中央", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "128", Name = "ラピスグリフィンズ", ShortName = "ラピスグリフィンズ", AreaName = "鹿児島中央", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "129", Name = "西田 WILD GIRAFFES", ShortName = "西田", AreaName = "鹿児島中央", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "130", Name = "八幡ミニバスケットボールスポーツ少年団", ShortName = "八幡", AreaName = "鹿児島中央", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "131", Name = "平佐西ミニバスケットボールクラブ", ShortName = "平佐西", AreaName = "北薩川内", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "132", Name = "育英ＢＢＣ", ShortName = "育英", AreaName = "北薩川内", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "133", Name = "盈進女子ミニバスケットボールスポーツ少年団", ShortName = "盈進", AreaName = "北薩川内", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "134", Name = "可愛Shining　Star", ShortName = "可愛", AreaName = "北薩川内", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "135", Name = "亀山ミニバスケットボールクラブ", ShortName = "亀山", AreaName = "北薩川内", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "136", Name = "隈之城 White Bears", ShortName = "隈之城", AreaName = "北薩川内", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "137", Name = "proud wings", ShortName = "ｐｒｏｕｄ ｗｉｎｇｓ", AreaName = "北薩川内", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "138", Name = "川内バスケットボールクラブ", ShortName = "川内", AreaName = "北薩川内", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "139", Name = "FLYERS", ShortName = "ＦＬＹＥＲＳ", AreaName = "北薩出水", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "140", Name = "脇本UNITY", ShortName = "脇本", AreaName = "北薩出水", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "141", Name = "米ノ津東スマイルファイターズ", ShortName = "米ノ津東", AreaName = "北薩出水", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "142", Name = "阿久根バスケットボール少年団", ShortName = "阿久根", AreaName = "北薩出水", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "143", Name = "姶良ＷＩＮＧＳ", ShortName = "姶良", AreaName = "姶良", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "144", Name = "加治木・柁城ミニバスケットボールスポーツ少年団", ShortName = "加治木柁城", AreaName = "姶良", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "145", Name = "蒲生カンファーズJr", ShortName = "蒲生", AreaName = "姶良", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "146", Name = "国分シューティングスターズ", ShortName = "国分", AreaName = "姶良", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "147", Name = "隼人ファルコンズ", ShortName = "隼人", AreaName = "姶良", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "148", Name = "きりしま舞鶴スポーツクラブ", ShortName = "きりしま舞鶴", AreaName = "姶良", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "149", Name = "霧島UNITE", ShortName = "霧島ＵＮＩＴＥ", AreaName = "姶良", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "150", Name = "Laugh", ShortName = "Ｌａｕｇｈ", AreaName = "姶良", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "151", Name = "B.B.B Jr", ShortName = "Ｂ．Ｂ．Ｂ Ｊｒ", AreaName = "姶良", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "152", Name = "鹿屋ﾐﾆﾊﾞｽｹｯﾄﾎﾞｰﾙ", ShortName = "鹿屋", AreaName = "肝属", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "153", Name = "高山ミニバスケットボールスポーツ少年団", ShortName = "高山", AreaName = "肝属", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "154", Name = "寿北バスケットボールクラブ", ShortName = "寿北", AreaName = "肝属", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "155", Name = "ネクサス鹿屋", ShortName = "ネクサス鹿屋", AreaName = "肝属", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "156", Name = "東串良ＢＢＣ", ShortName = "東串良ＢＢＣ", AreaName = "肝属", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "157", Name = "BLAX", ShortName = "ＢＬＡＸ", AreaName = "肝属", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "158", Name = "SKY　BASKETBALL　CLUB", ShortName = "ＳＫＹ", AreaName = "肝属", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "159", Name = "TROWRE", ShortName = "ＴＲＯＷＲＥ", AreaName = "日置・南薩", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "160", Name = "加世田女子ミニバスケットボール　コミュニティークラブ", ShortName = "加世田", AreaName = "日置・南薩", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "161", Name = "川辺ミニバスケットボールスポーツ少年団", ShortName = "川辺", AreaName = "日置・南薩", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "162", Name = "LAULE'A　TANBAバスケットボールクラブ", ShortName = "ＬＡＵＬＥ’Ａ", AreaName = "日置・南薩", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "163", Name = "いぶすきBC", ShortName = "いぶすき", AreaName = "日置・南薩", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "164", Name = "日置REX", ShortName = "日置ＲＥＸ", AreaName = "日置・南薩", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "165", Name = "妙円寺ミニバスケットボールスポーツ少年団", ShortName = "妙円寺", AreaName = "日置・南薩", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "166", Name = "LEPUS SAKURAYAMA", ShortName = "ＬＥＰＵＳ ＳＡＫＵＲＡＹＡＭＡ", AreaName = "日置・南薩", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "167", Name = "枕崎バスケットボールクラブ", ShortName = "枕崎", AreaName = "日置・南薩", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "168", Name = "知覧小ﾐﾆﾊﾞｽｹｯﾄﾎﾞｰﾙ", ShortName = "知覧", AreaName = "日置・南薩", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "169", Name = "PRIDE", ShortName = "ＰＲＩＤＥ", AreaName = "日置・南薩", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "170", Name = "朝日ミニバスケットボールスポーツ少年団", ShortName = "朝日", AreaName = "大島", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "171", Name = "小宿ミニバスケットボールスポーツ少年団", ShortName = "小宿", AreaName = "大島", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "172", Name = "名瀬ミニバスケットボールスポーツ少年団", ShortName = "名瀬", AreaName = "大島", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "173", Name = "奄美ブラックラビッツ", ShortName = "奄美", AreaName = "大島", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "174", Name = "古仁屋RED ＧＲＯＷＳ", ShortName = "古仁屋", AreaName = "大島", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "175", Name = "伊津部", ShortName = "伊津部", AreaName = "大島", Gender = "Women" });
                MasterSettings.Teams.Add(new TeamMaster { Id = "176", Name = "知名Star whales B.B.C", ShortName = "知名", AreaName = "大島", Gender = "Women" });
                addMasterItem = true;
            }
            return addMasterItem;
        }

        public string SerializeToJSON(object obj, Type type)
        {
            try
            {
                // インデントを揃えて見やすく
                // 日本語など全Unicodeをエスケープしない
                JsonSerializerOptions jsonSerializerOptions =
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    };
                string json = JsonSerializer.Serialize(obj, type, jsonSerializerOptions);

                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSONファイルへのShiriaraizuに失敗しました: {ex.Message}");

                return string.Empty;
            }
        }

        /// <summary>
        /// JSON値を、objectTypeで指定されたインスタンスに変換します。
        /// </summary>
        /// <param name="jsonText"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public object? DeserializeFromJSON(string jsonText, Type objectType)
        {
            JsonSerializerOptions jsonSerializerOptions =
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) // 日本語など全Unicodeをエスケープしない
                };
            var obj =
                JsonSerializer.Deserialize(jsonText, objectType, jsonSerializerOptions);

            return obj;
        }

        // 新しい大会を作成して保存する
        public string CreateNewTournament(string name)
        {
            var newTournament = new TournamentInfo { Name = name };
            SavedTournaments.Add(newTournament);
            CurrentTournament = newTournament;

            return SerializeToJSON(SavedTournaments, typeof(List<TournamentInfo>));
        }

        // アクティブなエントリーデータを取得するヘルパー
        public TournamentEntry GetActiveEntry()
        {
            return CurrentGender == "Men" ? MenEntry : WomenEntry;
        }

        public Dictionary<int, string> GetCurrentSlotAssignments()
        {
            if (CurrentTournament == null)
            {
                return new();
            }
            return CurrentGender == "Men" ? CurrentTournament.MenSlotAssignments : CurrentTournament.WomenSlotAssignments;
        }

        public List<MatchResult> GetCurrentMatchResults()
        {
            if (CurrentTournament == null)
            {
                return new();
            }
            return CurrentGender == "Men" ? CurrentTournament.MenMatchResults : CurrentTournament.WomenMatchResults;
        }
    }
}
