namespace Automa.Entities
{
    internal static class HashUtility
    {
        public static uint Fletcher32(ComponentType[] data, int count)
        {
            unchecked
            {
                uint sum1 = 0xff;
                uint sum2 = 0xff;
                var offset = 0;
                while (count > 0)
                {
                    var batchCount = count < 359 ? count : 359;
                    for (var i = 0; i < batchCount; ++i)
                    {
                        sum1 += data[offset + i].TypeId;
                        sum2 += sum1;
                    }

                    sum1 = (sum1 & 0xffff) + (sum1 >> 16);
                    sum2 = (sum2 & 0xffff) + (sum2 >> 16);
                    count -= batchCount;
                    offset += batchCount;
                }

                sum1 = (sum1 & 0xffff) | (sum1 >> 16);
                sum2 = (sum2 & 0xffff) | (sum2 >> 16);
                return (sum2 << 16) | sum1;
            }
        }
    }
}