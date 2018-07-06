namespace TombLib.Utils
{
    public static class Swap
    {
        public static void Do<T>(ref T first, ref T second)
        {
            T temp = first;
            first = second;
            second = temp;
        }
    }
}
