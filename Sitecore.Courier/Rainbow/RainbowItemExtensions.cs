using System.IO;

static internal class RainbowItemExtensions
{
    public static string GetItemType(string file)
    {
        var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
        using (var sr = new StreamReader(fs))
        {
            sr.ReadLine();
            return sr.ReadLine();
        }
    }
}