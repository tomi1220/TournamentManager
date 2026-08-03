using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TournamentManager.Core.Models;

namespace TournamentManager.Core.Services
{
    public class BracketData
    {
        #region フィールド
        private TournamentData _tournamentData;
        //private PartInfo? _allDataInfo;
        //private Dictionary<int, PartInfo> _partDic = new Dictionary<int, PartInfo>();
        /// <summary>
        /// 不要な枠を除いたシード番号の配列
        /// </summary>
        private int[]? _pureSeedArray;
        #endregion

        #region コンストラクタ
        public BracketData()
        {
            _tournamentData = new TournamentData()
            {
                District = true,
                FinalLeague = false,
                OpenDisplayFrame = true
            };
        }
        #endregion

        #region プロパティ
        /// <summary>
        /// チーム数
        /// </summary>
        public int NumOfTeams { get; set; }
        #endregion

        #region メソッド
        public void GenerateBracketbuildingData()
        {
            if (NumOfTeams > 0)
            {
                generate(NumOfTeams);
            }
        }
        #endregion

        #region ローカル・メソッド
        private void generate(int NumberOfTeams)
        {
            // 全体を一つのパートに見立てて作業用の枠を作りシード番号を埋める
            PartInfo partInfo = FillSeedNumber(NumberOfTeams);
            if (partInfo.FirstRoundData == null)
            {
                return;
            }

            // 不要な枠を除いたシード番号の配列を作る
            _pureSeedArray = new int[NumberOfTeams];
            int tIdx = 0;
            for (int i = 0; i < partInfo.FullFrames / 2; i++)
            {
                if (partInfo.FirstRoundData[i, 0] != 0)
                {
                    _pureSeedArray[tIdx++] = partInfo.FirstRoundData[i, 0];
                }
                if (partInfo.FirstRoundData[i, 1] != 0)
                {
                    _pureSeedArray[tIdx++] = partInfo.FirstRoundData[i, 1];
                }
            }

            _tournamentData._partDic.Add(1, partInfo);
        }
        /// <summary>
        /// 作業用の枠を確保し、シード番号を埋める
        /// </summary>
        /// <param name="partInfo">パート毎の情報</param>
        private PartInfo FillSeedNumber(int numOfTeams)
        {
            PartInfo partInfo = new PartInfo();
            int numOfElement;
            partInfo.Round = Round(numOfTeams, out numOfElement);
            partInfo.NumberOfElement = numOfElement;

            int round = 0;
            while (partInfo.FullFrames < numOfTeams)
            {
                partInfo.FullFrames = (int)Math.Pow(2, round++);
            }

            partInfo.Node = GetElementDetail(numOfTeams, partInfo.NumberOfElement, false);

            int[] tSeedTemp = new int[partInfo.FullFrames];
            tSeedTemp[0] = 1;

            // １回戦対戦数分の配列を作り、シード番号を割り当てる
            int[,] tTemp = new int[partInfo.FullFrames / 2, 2];
            tTemp[0, 0] = 1;
            int seedNum = 2;
            int pos = 0;
            while ((pos = NextSeedPosition(tTemp, pos, partInfo.FullFrames)) != -1)
            {
                tTemp[pos / 2, pos % 2] = seedNum;
                tSeedTemp[pos] = seedNum;
                seedNum++;
                if (seedNum > partInfo.FullFrames)
                {
                    break;
                }
            }
            // 不要な枠を示す値（0）を設定する
            partInfo.FirstRoundData = new int[partInfo.FullFrames / 2, 2];
            for (int i = 0; i < partInfo.FullFrames / 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (tTemp[i, j] <= numOfTeams)
                    {
                        partInfo.FirstRoundData[i, j] = tTemp[i, j];
                    }
                    else
                    {
                        partInfo.FirstRoundData[i, j] = 0;
                    }
                }
            }

            return partInfo;
        }

        private int Round(int teams, out int element)
        {
            int teamsNo = teams;
            int half = teams;

            double twoPow = teams;
            int count = 0;

            while (half != 1)
            {
                half = half / 2;
                twoPow = twoPow / 2;
                count = count + 1;
            }

            if (twoPow == 1)
            {
                count = count - 1;
            }

            element = (int)Math.Pow(2, count) - 1;

            return count;
        }

        private int[] GetElementDetail(int numberOfTeams, int numberOfElement, bool reverse)
        {
            int[] detail = new int[4];
            int[] rest = new int[4];

            int[] element = new int[numberOfElement];

            element[0] = numberOfTeams;

            for (int A = 1; A <= numberOfElement / 2; A++)
            {
                detail[1] = element[A - 1];

                for (int i = 1; i <= 2; i++)
                {
                    detail[i + 1] = detail[i] / 2;
                    rest[i] = detail[i] % 2;
                }

                int index = 2 * A - 1;
                if (A == 1)
                {
                    if (rest[1] == 0)
                    {
                        element[index] = detail[2];
                        element[index + 1] = detail[2];
                    }
                    else if (rest[1] == 1 && rest[2] == 0)
                    {
                        if (!reverse)
                        {
                            element[index] = detail[2] + 1;
                            element[index + 1] = detail[2];
                        }
                        else
                        {
                            element[index] = detail[2];
                            element[index + 1] = detail[2] + 1;
                        }
                    }
                    else if (rest[1] == 1 && rest[2] == 1)
                    {
                        if (!reverse)
                        {
                            element[index] = detail[2];
                            element[index + 1] = detail[2] + 1;
                        }
                        else
                        {
                            element[index] = detail[2] + 1;
                            element[index + 1] = detail[2];
                        }
                    }
                }
                else
                {
                    int oddNumber = A % 2;

                    switch (oddNumber)
                    {
                        case 1:
                            if (rest[1] == 0)
                            {
                                element[index] = detail[2];
                                element[index + 1] = detail[2];
                            }
                            else if (rest[1] == 1 && rest[2] == 0)
                            {
                                if (!reverse)
                                {
                                    element[index] = detail[2];
                                    element[index + 1] = detail[2] + 1;
                                }
                                else
                                {
                                    element[index] = detail[2] + 1;
                                    element[index + 1] = detail[2];
                                }
                            }
                            else if (rest[1] == 1 && rest[2] == 1)
                            {
                                if (!reverse)
                                {
                                    element[index] = detail[2] + 1;
                                    element[index + 1] = detail[2];
                                }
                                else
                                {
                                    element[index] = detail[2];
                                    element[index + 1] = detail[2] + 1;
                                }
                            }
                            break;
                        case 0:
                            if (rest[1] == 0)
                            {
                                element[index] = detail[2];
                                element[index + 1] = detail[2];
                            }
                            else if (rest[1] == 1 && rest[2] == 0)
                            {
                                if (!reverse)
                                {
                                    element[index] = detail[2] + 1;
                                    element[index + 1] = detail[2];
                                }
                                else
                                {
                                    element[index] = detail[2];
                                    element[index + 1] = detail[2] + 1;
                                }
                            }
                            else if (rest[1] == 1 && rest[2] == 1)
                            {
                                if (!reverse)
                                {
                                    element[index] = detail[2];
                                    element[index + 1] = detail[2] + 1;
                                }
                                else
                                {
                                    element[index] = detail[2] + 1;
                                    element[index + 1] = detail[2];
                                }
                            }
                            break;
                    }
                }
            }
            return element;
        }

        /// <summary>
        /// 次のシード番号の位置を返す
        /// </summary>
        /// <param name="tTemp">1回戦の対戦カード（シード番号）</param>
        /// <param name="pos">ひとつ前のシード位置</param>
        /// <param name="workFrames">作業用の枠数（出場チーム数以上の最小べき数）</param>
        /// <returns></returns>
        private int NextSeedPosition(int[,] tTemp, int pos, int workFrames)
        {
            int nextPos = 0;
            int counter = 0;
            int nextLength = workFrames;
            int start = 0;
            while (nextLength > 1)
            {
                nextPos = nextLength - (pos - start) - 1 + start;
                if (tTemp[nextPos / 2, nextPos % 2] == 0)
                {
                    break;
                }
                counter++;
                nextLength /= 2;
                if (pos > nextLength)
                {
                    start = nextLength;
                }
                else
                {
                    start = 0;
                }
            }
            return nextPos;
        }

        #endregion
    }
}