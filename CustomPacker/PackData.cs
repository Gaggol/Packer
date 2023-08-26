using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomPacker
{
    internal static class PackData
    {
        //NAME SPACE SIZE SPACE CONTENT END

        readonly static ConcurrentBag<byte[]> fileByteArray = new ConcurrentBag<byte[]>();

        internal static byte[] Pack(string DirPath) {
            if(Directory.Exists(DirPath)) {
                string[] files = Directory.GetFiles(DirPath);

                byte[] toWrite = Array.Empty<byte>();

                using(CancellationTokenSource cts = new CancellationTokenSource()) {

                    CancellationToken token = new CancellationToken();

                    List<Task> tasks = new List<Task>();

                    foreach(string file in files) {
                        tasks.Add(Task.Run(async () => {
                            try {
                                await ReadFile(file);
                            } catch(Exception ex) {
                                Console.WriteLine(ex);
                            } finally {

                            }
                        }, token));
                    }

                    Task.WaitAll(tasks.ToArray());
                    cts.Cancel();
                }


                bool firstFile = true;

                foreach(var byteArray in fileByteArray) {
                    if(firstFile) {
                        toWrite = byteArray;
                        firstFile = false;
                    } else {
                        toWrite = CombineByteArrays(toWrite, byteArray);
                    }
                }
                return toWrite;
            }
            return Array.Empty<byte>();
        }

        static Task ReadFile(string fileName) {
            if(File.Exists(fileName)) {
                byte[] content = File.ReadAllBytes(fileName);
                byte[] name = Encoding.UTF8.GetBytes(Path.GetFileName(fileName));
                byte[] sizeOfContent = BitConverter.GetBytes(content.Length);
                byte[] combined = Array.Empty<byte>();

                combined = CombineByteArrays(combined, name);
                combined = CombineByteArrays(combined, Program.SpaceBytes);
                combined = CombineByteArrays(combined, sizeOfContent);
                combined = CombineByteArrays(combined, Program.SpaceBytes);
                combined = CombineByteArrays(combined, content);
                combined = CombineByteArrays(combined, Program.EndBytes);

                fileByteArray.Add(combined);
            }
            return Task.CompletedTask;
        }

        static byte[] CombineByteArrays(byte[] a, byte[] b) {
            byte[] _ = new byte[a.Length + b.Length];

            Array.Copy(a, 0, _, 0, a.Length);
            Array.Copy(b, 0, _, a.Length, b.Length);
            return _;
        }
    }
}
