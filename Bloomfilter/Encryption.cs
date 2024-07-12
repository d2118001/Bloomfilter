using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Numerics;
using System.Globalization;

namespace Bloomfilter
{
    //定数の定義
    static class define
    {
        //bfindex数
        public const int IndexLength = 64;
        //ハッシュ関数の個数
        public const int NumOfHash = 8;
    }

    class Encryption
    {
        //Hash生成
        public static int[] HashSHA256(String word)
        {
            String[] StringHash = new String[define.NumOfHash];
            int[] Salt = new int[define.NumOfHash];

            // SHA256のハッシュ値を取得する
            SHA256 crypto = new SHA256CryptoServiceProvider();
            
            //salt生成
            Salt = GenerateSalt(define.NumOfHash);

            for (int p = 0; p < define.NumOfHash; p++)
            {
                //文字をUTF-8エンコード、バイト配列化
                byte[] byteWord = Encoding.UTF8.GetBytes(word + Salt[p]);
                byte[] hashValue = crypto.ComputeHash(byteWord);

                // バイト配列をUTF8エンコードで文字列化
                StringBuilder hashedText = new StringBuilder();
                for (int i = 0; i < hashValue.Length; i++)
                {
                    hashedText.AppendFormat("{0:X2}", hashValue[i]);
                }
                StringHash[p] = hashedText.ToString();
            }

            //文字列16進数のSHA256_Hash値をbfindexにする
            int[] bfindex = SHAToBFindex(StringHash);

            return bfindex;
        }

        //salt生成
        private static int[] GenerateSalt(int size)
        {
            int[] tmpSalt = new int[size];

            //saltを用いてSHA256の結果を変更
            for (int i = 0; i < size; i++)
            {
                //1,2,3,....
                tmpSalt[i] = i + i;
            }

            return tmpSalt;
        }

        //SHAのハッシュ値を64bitのハッシュ値に変換する
        private static int[] SHAToBFindex(String[] HashedWord)
        {
            int[] hash = new int[define.NumOfHash];

            //64bitのハッシュ値の生成
            for (int k = 0; k < define.NumOfHash; k++)
            {
                //整数値のみの64bitハッシュ値を計算
                hash[k] = Math.Abs((int)(BigInteger.Parse(HashedWord[k], NumberStyles.AllowHexSpecifier) % define.IndexLength));
            }
            Array.Sort(hash);    //小さい順にソート
            return hash;
        }
    }
}