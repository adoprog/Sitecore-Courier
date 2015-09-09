using System.Collections.Generic;

namespace Sitecore.Courier.Runner
{
    public static class ExclusionProcessor
    {
        public static void RemoveExcludedItems(string directoryPath, IEnumerable<string> excludedFiles)
        {
            foreach (var item in excludedFiles)
            {
                var path = string.Concat(string.Concat(directoryPath, "\\"), item);
                if (System.IO.File.Exists(path))
                {
                    // hard delete of excluded files. rename to .item.excluded? will courier still diff these?
                    System.IO.File.Delete(path);                    
                }
            }
        }
    }
}
