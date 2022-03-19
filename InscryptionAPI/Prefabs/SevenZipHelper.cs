using System;
using System.IO;
using SevenZip.Compression.LZMA;


namespace InscryptionAPI.Prefabs
{
    internal static class SevenZipHelper
    {
        public static MemoryStream StreamDecompress(MemoryStream inStream)
        {
            var decoder = new Decoder();

            inStream.Seek(0, SeekOrigin.Begin);
            var newOutStream = new MemoryStream();

            var properties = new byte[5];
            if (inStream.Read(properties, 0, 5) != 5)
                throw new Exception("input .lzma is too short");
            long outSize = 0;
            for (var i = 0; i < 8; i++)
            {
                var v = inStream.ReadByte();
                if (v < 0)
                    throw new Exception("Can't Read 1");
                outSize |= ((long)(byte)v) << (8 * i);
            }
            decoder.SetDecoderProperties(properties);

            var compressedSize = inStream.Length - inStream.Position;
            decoder.Code(inStream, newOutStream, compressedSize, outSize, null);

            newOutStream.Position = 0;
            return newOutStream;
        }


        static int dictionary = 1 << 23;

        // static Int32 posStateBits = 2;
        // static  Int32 litContextBits = 3; // for normal files
        // UInt32 litContextBits = 0; // for 32-bit data
        // static  Int32 litPosBits = 0;
        // UInt32 litPosBits = 2; // for 32-bit data
        // static   Int32 algorithm = 2;
        // static    Int32 numFastBytes = 128;

        static bool eos = false;





        static SevenZip.CoderPropID[] propIDs =
                   {
                    SevenZip.CoderPropID.DictionarySize,
                    SevenZip.CoderPropID.PosStateBits,
                    SevenZip.CoderPropID.LitContextBits,
                    SevenZip.CoderPropID.LitPosBits,
                    SevenZip.CoderPropID.Algorithm,
                    SevenZip.CoderPropID.NumFastBytes,
                    SevenZip.CoderPropID.MatchFinder,
                    SevenZip.CoderPropID.EndMarker
                };

        // these are the default properties, keeping it simple for now:
        static object[] properties =
                   {
                    (Int32)(dictionary),
                    (Int32)(2),
                    (Int32)(3),
                    (Int32)(0),
                    (Int32)(2),
                    (Int32)(128),
                    "bt4",
                    eos
                };

        public static byte[] Compress(byte[] inputBytes)
        {
            MemoryStream inStream = new MemoryStream(inputBytes);
            MemoryStream outStream = new MemoryStream();
            Encoder encoder = new Encoder();
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(outStream);
            encoder.Code(inStream, outStream, -1, -1, null);
            return outStream.ToArray();
        }

        public static void StreamDecompress(Stream inStream, Stream outStream, long inSize, long outSize)
        {
            var decoder = new Decoder();
            var properties = new byte[5];
            int ddd = inStream.Read(properties, 0, 5);
            if (ddd != 5)
                throw new Exception("input .lzma is too short");
            decoder.SetDecoderProperties(properties);
            inSize -= 5L;
            decoder.Code(inStream, outStream, inSize, outSize, null);

        }
    }
}
