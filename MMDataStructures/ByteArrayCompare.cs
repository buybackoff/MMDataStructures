using System.Runtime.ConstrainedExecution;

namespace MMDataStructures
{
    public static class ByteArrayCompare
    {
        /// <summary>
        /// Compare the values of two byte arrays
        /// </summary>
        /// <param name="arr1"></param>
        /// <param name="arr2"></param>
        /// <returns></returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static unsafe bool UnSafeEquals(byte[] arr1, byte[] arr2)
        {
            int length = arr1.Length;
            if (length != arr2.Length)
            {
                return false;
            }
            fixed (byte* ptrA = arr1)
            {
                byte* bPtr = ptrA;
                fixed (byte* ptrB = arr2)
                {
                    byte* bPtr2 = ptrB;
                    byte* bPtr3 = bPtr;
                    byte* bPtr4 = bPtr2;
                    while (length >= 10)
                    {
                        if ((((*(((int*) bPtr3)) != *(((int*) bPtr4))) ||
                              (*(((int*) (bPtr3 + 2))) != *(((int*) (bPtr4 + 2))))) ||
                             ((*(((int*) (bPtr3 + 4))) != *(((int*) (bPtr4 + 4)))) ||
                              (*(((int*) (bPtr3 + 6))) != *(((int*) (bPtr4 + 6)))))) ||
                            (*(((int*) (bPtr3 + 8))) != *(((int*) (bPtr4 + 8)))))
                        {
                            break;
                        }
                        bPtr3 += 10;
                        bPtr4 += 10;
                        length -= 10;
                    }
                    while (length > 0)
                    {
                        if (*(((int*) bPtr3)) != *(((int*) bPtr4)))
                        {
                            break;
                        }
                        bPtr3 += 2;
                        bPtr4 += 2;
                        length -= 2;
                    }
                    return (length <= 0);
                }
            }
        }

        public static bool Equals(byte[] arr1, byte[] arr2)
        {
            if (arr1.Length != arr2.Length)
            {
                return false;
            }
            for (int i = 0; i < arr1.Length; i++)
            {
                if (!arr1[i].Equals(arr2[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}