using System;
using System.Collections.Generic;
using System.IO;

namespace System.IO
{
    public static class PathExtensions
    {
        public static List<string> GetParentPaths(this string path)
        {
            var parentPaths = new List<string>();
            string currentPath = path;

            while (!string.IsNullOrEmpty(currentPath))
            {
                currentPath = Path.GetDirectoryName(currentPath);
                if (!string.IsNullOrEmpty(currentPath))
                {
                    parentPaths.Add(currentPath);
                }
            }

            return parentPaths;
        }
    }
}
