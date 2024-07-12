using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace Bloomfilter
{
    public static class DataBaseControl
    {
        public static Dictionary<string, int[]> string_hash = new Dictionary<string, int[]>();           //文字列とハッシュ値を格納                           

        public static int[] bfindex = new int[define.IndexLength];                                       //変数の名前の通り

        //BFindex登録
        public static void Registerbfindex(String InputText, int[] BFindex)
        {
            try
            {
                string_hash.Add(InputText, BFindex);     //既に登録されている文字列の場合例外が出る
                Makebfindex();                      //BFindex作成(兼更新用)
                return;
            }
            catch
            {
                return;                                    
            }
        }

        //BFindex作成(兼更新)関数
        private static void Makebfindex()
        {
            for (int j = 0; j < define.IndexLength; j++)
            {
                bfindex[j] = 0;     //0で初期化
            }

            //2進数のBFindex作成
            foreach (int[] v in string_hash.Values)
            {
                for (int i = 0; i < define.NumOfHash; i++)
                {
                    bfindex[v[i]] = 1;
                }
            }
        }
    }
}
