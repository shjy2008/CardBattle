using System;
using System.Collections.Generic;
using BigInteger = System.Numerics.BigInteger;

namespace Assets.Scripts.common
{
    public class BigNumber
    {
        public BigNumber()
        {
        }

        public static List<string> suffixList = new List<string> { "", "k", "m", "b", "t", "aa", "bb", "cc", "dd" };

        public static string BigInteger2FormatString(BigInteger big)
        {
            string normalString = big.ToString();

            if (normalString.Length <= 3)
            {
                return normalString;
            }
            else
            {
                // 3个3个拆分，取前2个
                int maxIndex = (normalString.Length - 1) / 3;
                int firstStrLen = normalString.Length % 3;
                if (firstStrLen == 0)
                {
                    firstStrLen = 3;
                }
                string firstStr = normalString.Substring(0, firstStrLen);
                string secondStr = normalString.Substring(firstStrLen, 3);
                int secondNum = int.Parse(secondStr);
                return string.Format("{0}.{1:D2}{2}", firstStr, (secondNum + 5) / 10, suffixList[maxIndex]);
            }
        }
    }

}
