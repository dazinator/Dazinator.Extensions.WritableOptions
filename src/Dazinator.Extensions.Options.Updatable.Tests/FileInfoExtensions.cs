using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Microsoft.Extensions.FileProviders
{
    public static class FileInfoExtensions
    {
        public static string ReadAllContent(this IFileInfo fileInfo)
        {
            using (var reader = new StreamReader(fileInfo.CreateReadStream()))
            {
                return reader.ReadToEnd();
            }
        }

        //public static string ReadAllBytes(this IFileInfo fileInfo)
        //{
        //    using (var reader = new StreamReader(fileInfo.CreateReadStream()))
        //    {
        //        return reader.readal ();
        //    }
        //}
    }
}

