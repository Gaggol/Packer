using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CustomPacker
{
    internal struct PackedData {
        public string FileName;
        public byte[] FileContent;

        public override string ToString() {
            return FileName;
        }
    }

    internal static class UnpackData {
        private enum DataPart {
            Name,
            SizeOfContent,
            Content
        }

        internal static PackedData[] Unpack(byte[] byteArray) {
            DataPart currentPart = DataPart.Name;
            DataPart prevPart = DataPart.Name;
            PackedData[] packedDatas = Array.Empty<PackedData>();
            PackedData currentPackedData = new PackedData();
            byte[] data = Array.Empty<byte>();

            int spaceHits = 0;
            int endHits = 0;

            try {
                int contentSize = 0;


                for(int i = 0; i < byteArray.Length; i++) {
                    if(byteArray[i] == 0x00 && byteArray[i + 1] == 0x47 && byteArray[i+2] == 0x47 && byteArray[i+3] == 0x00) {
                        i = i + 3;
                        spaceHits += 1;
                        switch(currentPart) {
                            case DataPart.Name:
                                currentPart = DataPart.SizeOfContent;
                                continue;
                            case DataPart.SizeOfContent:
                                currentPart = DataPart.Content;
                                continue;
                        }
                    }
                    
                    if(byteArray[i] == 0x47 && byteArray[i + 1] == 0x41 && byteArray[i + 2] == 0x47 && byteArray[i + 3] == 0x47 && byteArray[i + 4] == 0x4F && byteArray[i + 5] == 0x4C && byteArray[i + 6] == 0x00) {
                        endHits += 1;
                        currentPart = DataPart.Name;

                        PackedData[] _pd = new PackedData[packedDatas.Length + 1];
                        Array.Copy(packedDatas, _pd, packedDatas.Length);
                        _pd[_pd.Length - 1] = currentPackedData;
                        packedDatas = _pd;

                        currentPackedData = new PackedData();

                        i = i + 7;

                        byte[] removeRead = new byte[byteArray.Length - i];
                        Array.Copy(byteArray, i, removeRead, 0, byteArray.Length - i);
                        byteArray = removeRead;
                        i = -1;

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
                                i = i + contentSize-1;
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
                return packedDatas;
            }
            return packedDatas;
        }
    }
}
