using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace CustomPacker
{
    internal class Program
    {

        const string TEST_FOLDER = "./../../../testfiles";
        const string OUTPUT_FOLDER = "./unpacked";

        private static void Main(string[] args) {

            string DirPath = TEST_FOLDER;

            if(!Directory.Exists(DirPath)) {
                Directory.CreateDirectory(DirPath);
                Console.WriteLine("No test files found. Place files in testfiles folder to continue.");
                return;
            }

            long totalSize = 0;
            foreach(var file in Directory.GetFiles(DirPath)) {
                totalSize += new FileInfo(file).Length;
            }

            Console.WriteLine("Files: " + Directory.GetFiles(DirPath).Length + " | Total Size: " + totalSize/1000/1000 + " mb");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            PackData.Pack(DirPath);

            Console.WriteLine("Finished packing data: " + sw.Elapsed.ToString("mm\\:ss"));
            sw.Restart();

            PackedData[] _data = UnpackData.Unpack(File.ReadAllBytes("data"));

            Console.WriteLine("Reading packed data completed: " + sw.Elapsed.ToString("mm\\:ss"));
            sw.Restart();

            Directory.CreateDirectory(OUTPUT_FOLDER);
            foreach(var packedData in _data) {
                using(FileStream fs = File.Create(OUTPUT_FOLDER + "/" + packedData.FileName)) {
                    fs.Write(packedData.FileContent);
                }
            }
            Console.WriteLine("Written all files: " + sw.Elapsed.ToString("mm\\:ss"));

            totalSize = 0;

            foreach(var file in Directory.GetFiles(OUTPUT_FOLDER)) {
                totalSize += new FileInfo(file).Length;
            }

            Console.WriteLine("Files written: " + Directory.GetFiles(OUTPUT_FOLDER).Length + " | Size: " + totalSize/1000/1000 + " mb");

            sw.Stop();
        }
    }
}