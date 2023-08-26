using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace CustomPacker
{
    public class Program
    {
        readonly static byte[] endBytes = new byte[] { 0x47, 0x41, 0x47, 0x47, 0x4F, 0x4C, 0x00 };
        readonly static byte[] spaceBytes = new byte[] { 0x00, 0x47, 0x47, 0x00 };

        public static byte[] SpaceBytes { get { return spaceBytes; } }
        public static byte[] EndBytes { get { return endBytes; } }

        string dataName;

        public Program() {
            this.dataName = "data.pgk";
        }

        public Program(string dataName) {
            if(dataName.Contains('.')) {
                this.dataName = dataName;
            } else {
                this.dataName = dataName + ".pgk";
            }
        }

        public void SetDataName(string dataName) {
            if(this.dataName.Contains('.')) {
                this.dataName = dataName;
            } else {
                this.dataName = dataName + ".pgk";
            }
        }

        public bool Unpack() {
            if(File.Exists(dataName)) {
                var files = UnpackData.Unpack(File.ReadAllBytes(dataName));

                string dirName = Path.GetFileNameWithoutExtension(dataName);
                if(File.Exists(dirName)) {
                    return false;
                }
                Directory.CreateDirectory(dirName);

                foreach(var file in files) {
                    File.WriteAllBytes(dirName + "/" + file.FileName, file.FileContent);
                }
                return true;
            }
            return false;
        }

        public long PackFolder(string folder) {
            if(Directory.Exists(folder)) {
                byte[] data = PackData.Pack(folder);

                File.WriteAllBytes(dataName, data);
                return new FileInfo(dataName).Length;
            }
            return 0;
        }

#if DEBUG
        public void Test() {
            Test("test", "./");
        }
        public void Test(string path) {
            Test("test", path);
        }
        public void Test(string dataName, string path) {
            Program program = new Program(dataName);
            program.PackFolder(path);
            program.Unpack();
        }
#endif
    }
}