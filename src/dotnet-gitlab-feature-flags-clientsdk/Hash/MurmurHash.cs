using System.Text;

namespace FeatureFlags.ClientSdk
{
    internal sealed class MurmurHash
    {
        public static uint MurmurHash3(string input, uint seed = 0)
        {
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;

            byte[] data = Encoding.UTF8.GetBytes(input);
            int len = data.Length;
            int blockCount = len / 4;

            uint h1 = seed;

            for (int i = 0; i < blockCount; i++)
            {
                uint k1 = BitConverter.ToUInt32(data, i * 4);
                k1 *= c1;
                k1 = (k1 << 15) | (k1 >> 17);
                k1 *= c2;

                h1 ^= k1;
                h1 = (h1 << 13) | (h1 >> 19);
                h1 = h1 * 5 + 0xe6546b64;
            }

            uint k2 = 0;
            int tail = blockCount * 4;

            switch (len & 3)
            {
                case 3: k2 ^= (uint)data[tail + 2] << 16; goto case 2;
                case 2: k2 ^= (uint)data[tail + 1] << 8; goto case 1;
                case 1:
                    k2 ^= data[tail];
                    k2 *= c1;
                    k2 = (k2 << 15) | (k2 >> 17);
                    k2 *= c2;
                    h1 ^= k2;
                    break;
            }

            h1 ^= (uint)len;
            h1 ^= h1 >> 16;
            h1 *= 0x85ebca6b;
            h1 ^= h1 >> 13;
            h1 *= 0xc2b2ae35;
            h1 ^= h1 >> 16;

            return h1;
        }
    }
}
