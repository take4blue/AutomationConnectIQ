using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace AutomationConnectIQ.Lib
{
    /// <summary>
    /// Connect IQのプロジェクトファイルを解析し、いろいろ処理を行えるようにする
    /// </summary>
    /// <remarks>
    /// ・プログラム名、使用しているデバイス名を取り出せるようにする。<br/>
    /// ・シミュレーションを連続実行する
    /// </remarks>
    public class Jungle
    {
        /// <summary>
        /// プロジェクトファイルからJungleを作成するためのファクトリ関数
        /// バージョン違いで解析方法が異なる場合に、ここでそれらを吸収するようにする
        /// </summary>
        /// <param name="projectFile">対象とするプロジェクト名ファイル名(monkey.jungleを指定する)</param>
        static public Jungle Create(string projectFile)
        {
            return new Jungle(projectFile);
        }

        /// <summary>
        /// ターゲットとするデバイス情報
        /// </summary>
        public List<string> Devices { get; private set; } = new List<string>();

        /// <summary>
        /// プロジェクトの中に記載があるエントリー名<br/>
        /// (iq:applicationのentryを出力している)
        /// </summary>
        /// <remarks>
        /// 本当のビルド時の名前は、iq:applicationのnameに入っているのだが、そちらはリソースを参照している。<br/>
        /// このクラス内でリソースまでは見に行ってないので、そちらの名前は参照していない。
        /// </remarks>
        public string EntryName { get; private set; } = "";

        /// <summary>
        /// プロジェクトファイル名
        /// </summary>
        public string JungleFile {
            get => jungleFile_;
        }

        private readonly string manifest_;

        private readonly string jungleFile_;

        /// <summary>
        /// プロジェクト読み込み<br/>
        /// 例外は、StreamReaderと同じものが返ってくる可能性あり
        /// </summary>
        /// <param name="projectFile">プロジェクトファイル</param>
        public Jungle(string projectFile)
        {
            jungleFile_ = projectFile;
            // マニフェストファイルを探す
            string line;
            using (StreamReader sr = new StreamReader(projectFile)) {
                line = sr.ReadLine();
            }
            if (line.Length != 0 && line.IndexOf("project.manifest =") >= 0) {
                var work = line.Split("=");
                manifest_ = Path.GetDirectoryName(projectFile) + @"\" + work[1].Trim();
            }

            // マニフェストファイルを解析して、デバイス名とプログラム名を取り出す
            if (manifest_.Length > 0) {
                using StreamReader sr = new StreamReader(manifest_);
                using XmlReader reader = XmlReader.Create(sr);
                while (reader.Read()) {
                    switch (reader.NodeType) {
                    case XmlNodeType.Element:
                        if (reader.HasAttributes) {
                            switch (reader.LocalName) {
                            case "application":
                                while (reader.MoveToNextAttribute()) {
                                    if (reader.LocalName == "entry") {
                                        EntryName = reader.Value;
                                    }
                                }
                                break;
                            case "product":
                                while (reader.MoveToNextAttribute()) {
                                    if (reader.LocalName == "id") {
                                        Devices.Add(reader.Value);
                                    }
                                }
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 指定デバイス名が登録されているかどうか判断
        /// </summary>
        public bool IsValidDevice(string device)
        {
            return Devices.FindAll((x) => { return device == x; }).Count == 1;
        }

        /// <summary>
        /// デフォルトのプログラム出力ファイル名
        /// </summary>
        /// <remarks>
        /// jungleファイルの場所のbinの下のProgramNameで拡張子がprgとする
        /// </remarks>
        public string DefaultProgramPath {
            get {
                return Path.GetDirectoryName(jungleFile_) + @"\bin\" + MakeProgramPath(EntryName);
            }
        }

        /// <summary>
        /// プログラム名を作る<br/>
        /// 実際には拡張子を追加しているだけだが
        /// </summary>
        public static string MakeProgramPath(string name)
        {
            return name + ".prg";
        }
    }
}
