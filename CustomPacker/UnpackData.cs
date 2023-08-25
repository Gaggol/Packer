﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CustomPacker
{
    public struct PackedData {
        public string FileName;
        public byte[] FileContent;

        public override string ToString() {
            return FileName;
        }
    }

    public static class UnpackData {
        private enum DataPart {
            Name,
            SizeOfContent,
            Content
        }

        public static Task SaveFile(PackedData data) {
            Console.WriteLine(data.FileName);
            Directory.CreateDirectory("./progTest/");
            using(FileStream fs = File.Create("./progTest/" + data.FileName)) {
                fs.Write(data.FileContent);
            }
            return Task.CompletedTask;
        }

        public static PackedData[] Unpack(byte[] byteArray) {
            DataPart currentPart = DataPart.Name;
            DataPart prevPart = DataPart.Name;
            PackedData[] packedDatas = Array.Empty<PackedData>();
            PackedData currentPackedData = new PackedData();
            byte[] data = Array.Empty<byte>();

            try {
                int contentSize = 0;

                for(int i = 0; i < byteArray.Length; i++) {
                    if(byteArray[i] == 0x00 && byteArray[i + 1] == 0x47 && byteArray[i+2] == 0x47 && byteArray[i+3] == 0x00) {
                        switch(currentPart) {
                            case DataPart.Name:
                                currentPart = DataPart.SizeOfContent;
                                i = i + 3;
                                continue;
                            case DataPart.SizeOfContent:
                                currentPart = DataPart.Content;
                                i = i + 3;
                                continue;
                        }
                    }
                    if(byteArray[i] == 0x47 && byteArray[i + 1] == 0x41 && byteArray[i + 2] == 0x47 && byteArray[i + 3] == 0x47 && byteArray[i + 4] == 0x4F && byteArray[i + 5] == 0x4C && byteArray[i + 6] == 0x00) {
                        currentPart = DataPart.Name;

                        PackedData[] _pd = new PackedData[packedDatas.Length + 1];
                        Array.Copy(packedDatas, _pd, packedDatas.Length);
                        _pd[_pd.Length - 1] = currentPackedData;
                        packedDatas = _pd;

                        currentPackedData = new PackedData();

                        i = i + 6;
                        continue;
                    }

                    if(currentPart != prevPart) {
                        switch(prevPart) {
                            case DataPart.Name:
                                currentPackedData.FileName = Encoding.UTF8.GetString(data);
                                break;
                            case DataPart.SizeOfContent:
                                contentSize = BitConverter.ToInt32(data);
                                data = new byte[contentSize];
                                Array.Copy(byteArray, i, data, 0, contentSize);
                                i = i + contentSize;
                                currentPackedData.FileContent = data;

                                data = Array.Empty<byte>();
                                prevPart = currentPart;
                                continue;
                        }
                        data = Array.Empty<byte>();
                    }

                    byte[] _ = new byte[data.Length+1];
                    Array.Copy(data, _, data.Length);
                    _[_.Length - 1] = byteArray[i];
                    data = _;
                    
                    prevPart = currentPart;
                }
            } catch(IndexOutOfRangeException) {
                Console.WriteLine("Reached End - (Corrupted Packed File)");
                return packedDatas;
            }
            Console.WriteLine("Reached End");
            return packedDatas;
        }
    }
}