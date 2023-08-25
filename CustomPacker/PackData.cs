using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomPacker
{
    public static class PackData
    {
        //NAME SPACE SIZE SPACE CONTENT SPACE END

        static byte[] endBytes = new byte[] { 0x47, 0x41, 0x47, 0x47, 0x4F, 0x4C, 0x00 };
        static byte[] spaceBytes = new byte[] { 0x00, 0x47, 0x47, 0x00 };
        static ConcurrentBag<byte[]> fileByteArray = new ConcurrentBag<byte[]>();

        public static void Pack(string DirPath) {
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
                File.WriteAllBytes("data", toWrite);
            }
        }

        static Task ReadFile(string fileName) {
            if(File.Exists(fileName)) {
                byte[] content = File.ReadAllBytes(fileName);
                byte[] name = Encoding.UTF8.GetBytes(Path.GetFileName(fileName));
                byte[] sizeOfContent = BitConverter.GetBytes(content.Length);

                byte[] combined = new byte[endBytes.Length + spaceBytes.Length + sizeOfContent.Length + spaceBytes.Length + name.Length + content.Length];

                int length = 0;

                name.CopyTo(combined, length);
                length += name.Length;

                spaceBytes.CopyTo(combined, length);
                length += spaceBytes.Length;

                sizeOfContent.CopyTo(combined, length);
                length += sizeOfContent.Length;

                spaceBytes.CopyTo(combined, length);
                length += spaceBytes.Length;

                content.CopyTo(combined, length);
                length += content.Length;

                endBytes.CopyTo(combined, length);

                fileByteArray.Add(combined);
            }
            return Task.CompletedTask;
        }

        static byte[] CombineByteArrays(byte[] a, byte[] b) {

            byte[] _ = new byte[a.Length + b.Length];
            a.CopyTo(_, 0);
            b.CopyTo(_, a.Length);

            return _;
        }
    }
}
