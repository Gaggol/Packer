namespace CPT
{
#if DEBUG
    internal class Program
    {
        private static void Main(string[] args) {
            var p = new CustomPacker.Program();
            p.Test();
        }
    }
#endif
}