using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using System.Numerics;

namespace Procon27Comp.Components
{
    public class PuzzleReader
    {
        public string Path { get; }

        /// <summary>
        /// 認識結果のファイルパスから<see cref="PuzzleReader"/>の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="path">認識結果のファイルパス</param>
        public PuzzleReader(string path)
        {
            Path = path;
        }

        /// <summary>
        /// 認識結果からパズルデータを作成します。
        /// </summary>
        /// <returns></returns>
        public Puzzle Read()
        {
            using (var reader = new StreamReader(Path))
            {
                int fcount = int.Parse(reader.ReadLine());
                var flist = new List<Frame>(fcount);
                for (int i = 0; i < fcount; i++)
                {
                    int vcount = int.Parse(reader.ReadLine());
                    var vlist = new List<Vector2>(vcount);
                    for (int j = 0; j < vcount; j++)
                    {
                        string[] s = reader.ReadLine().Split(' ');
                        vlist.Add(new Vector2(float.Parse(s[0]), float.Parse(s[1])));
                    }
                    flist.Add(new Frame(vlist));
                }

                int pcount = int.Parse(reader.ReadLine());
                var plist = new List<Piece>(pcount);
                for (int i = 0; i < pcount; i++)
                {
                    int vcount = int.Parse(reader.ReadLine());
                    var vlist = new List<Vector2>(vcount);
                    for (int j = 0; j < vcount; j++)
                    {
                        string[] s = reader.ReadLine().Split(' ');
                        vlist.Add(new Vector2(float.Parse(s[0]), float.Parse(s[1])));
                    }
                    plist.Add(new Piece(vlist.Distinct()));
                }

                return new Puzzle(flist, plist);
            }
        }
    }
}
