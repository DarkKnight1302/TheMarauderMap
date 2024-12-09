using System.Text;

namespace TheMarauderMap.Utilities
{
    public static class NumberUtility
    {
        public static int CleanNumberString(string numberWithComma)
        {
            if (float.TryParse(numberWithComma, out float res))
            {
                return (int)res;
            }
            StringBuilder sb = new StringBuilder();
            for(int i=0; i<numberWithComma.Length; i++)
            {
                if (char.IsDigit(numberWithComma[i]))
                {
                    sb.Append(numberWithComma[i]);
                }
            }
            return int.Parse(sb.ToString());
        }
    }
}
