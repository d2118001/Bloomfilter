using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Collections;
using System.IO;

namespace Bloomfilter
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>

    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        //登録ボタンクリック
        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            Registration_TextBox();
        }

        //検索ボタンクリック
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            Search_TextBox();
        }

        //登録するテキストボックスでEnterを押すと実行
        private void RegistrationTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Registration_TextBox();
            }
        }

        //検索するテキストボックスでEnterを押すと実行
        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Search_TextBox();
            }
        }

        private void Registration_TextBox()
        {
            String inputText = RegistrationTextBox.Text;

            if (inputText == "")
            {
                MessageBox.Show("なにか文字を入力してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //文字をハッシュ化し、bfindexにする
            int[] inputTextbfindex = Encryption.HashSHA256(inputText);

            //入力した文字のbfindexを登録
            DataBaseControl.Registerbfindex(inputText, inputTextbfindex);

            //画面更新
            DisplayDataBase();

            RegistrationTextBox.Text = "";  //テキストボックスを空白する
        }

        private void Search_TextBox()   //検索欄に入れられた文字列を検索
        {
            String inputText = SearchTextBox.Text;

            if (inputText == "")
            {
                MessageBox.Show("なにか文字を入力してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //文字をbfindexにする
            int[] inputTextbfindex = Encryption.HashSHA256(inputText);

            SearchWord(inputTextbfindex, inputText);            //検索と結果表示

            SearchTextBox.Text = "";
        }

        //辞書データを自動入力
        private void BFA_Click(object sender, RoutedEventArgs e)
        {
            string str = "";
            ArrayList al = new ArrayList();

            try
            {   //password.txtがないと例外を投げる
                using (StreamReader sr = new StreamReader("password.txt", Encoding.GetEncoding("Shift_JIS")))   //password.txt読み込み
                {
                    if (sr != null)
                        while ((str = sr.ReadLine()) != null)
                        {
                            al.Add(str);                   //1行ごとにArrayListに入れる
                        }
                    for (int i = 0; i < al.Count; i++)      //alの数だけ回す
                    {
                        String word = al[i].ToString();
                        if (word.IndexOf("#") == 0 || word.Length == 0)                   //password.txtのコメント行と空白行をスキップ
                            continue;
                        int[] inputTextbfindex = Encryption.HashSHA256(word);
                        DataBaseControl.Registerbfindex(word, inputTextbfindex);       //入力した文字のbfindexを登録
                        DisplayDataBase();
                        if (i > 39) //全部読み込むとものすごい時間がかかるので50行目で切り上げる
                            break; 
                    }
                }
            }

            catch   //password.txtがなかったとき
            {
                MessageBox.Show("password.txtを実行ファイルと同じフォルダ内に配置してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //DataGridに文字列(bfindex、登録文字)を出力
        //ついでにbfindex表示制御
        private void DisplayDataBase()
        {
            //一度画面をリセット
            Displaybfindex.Items.Clear();
            DataBase.Items.Clear();

            int[] bfindex = new int[define.NumOfHash];

            //登録されているキーと値両方を取得
            foreach (KeyValuePair<string, int[]> kvp in DataBaseControl.string_hash)
            {
                bfindex = kvp.Value;
                String bfindexText = "";
                for (int j = 0; j < define.NumOfHash; j++)
                {
                    bfindexText += bfindex[j];
                    //画面表示用に変換
                    if (j != define.NumOfHash - 1)
                    {
                        bfindexText += ",";
                    }
                }

                Dictionary<string, string> dict = new Dictionary<string,string>();   //画面に表示する表を格納
                dict.Add(kvp.Key,bfindexText);                                       //表に行を追加
                DataBase.Items.Add(dict);                                            //表を出力
            }

            //2進数のbfindexを16進数に変換
            String tmp16 = "";  //16進数
            for (int o = 7; o < DataBaseControl.bfindex.Length; o += 8)
            {
                String tmp2 = "";   //2進数
                //8bitずつにわける
                for (int i = 7; i >= 0; i--)
                    tmp2 += DataBaseControl.bfindex[o - i].ToString();

                //2進数から16進数に変換
                int num = Convert.ToInt32(tmp2, 2);
                tmp16 += num.ToString("X2");

                //少しでも見やすく変換
                if (o != DataBaseControl.bfindex.Length - 1)
                {
                    tmp16 += "-";
                }
            }

            //16進数bfindexを追加
            Displaybfindex.Items.Add(tmp16);
        }

        //検索と結果表示
        private void SearchWord(int[] inputbfindex, String inputWord)
        {
            //画面をリセット
            Result.Items.Clear();

            Boolean flg = true;            //検索文字がない場合falseになる
            for (int i = 0; i < define.NumOfHash; i++)
            {
                if (DataBaseControl.bfindex[inputbfindex[i]] == 0)
                {
                    flg = false;
                    break;          //ハッシュが1個でもBFindexになければ存在しない文字列
                }
            }

            //あるかどうか判定
            if (flg)
            {
                Result.Items.Add("あるかも");     //あるかもしれない
            }

            else
            {
                Result.Items.Add("なし");           //ない
            }

            DisplayDetail(inputbfindex, inputWord);          //詳細表示
        }

        //検索したデータの詳細表示
        private void DisplayDetail(int[] inputbfindex, String inputWord)
        {
            String tmpString = "";
            for (int i = 0; i < define.NumOfHash; i++)
            {
                tmpString += inputbfindex[i];
                //画面表示用に変換
                if (i != define.NumOfHash - 1)
                {
                    tmpString += ",";
                }
            }

            //一度画面を削除
            Detail.Items.Clear();
            //詳細ボックスにテキスト表示
            Detail.Items.Add("Word：" + inputWord);
            Detail.Items.Add("Hash：" + tmpString);
        }
    }
}
