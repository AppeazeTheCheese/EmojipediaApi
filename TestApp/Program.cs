using System;
using EmojipediaApi;

namespace TestApp
{
    internal class Program
    {
        private static void Main()
        {
            while (true)
            {
                var read = Console.ReadLine();
                using (var searcher = new EmojiSearcher())
                {
                    if (read != null && read.ToLower() == "random")
                    {
                        var emoji = searcher.Random();
                        Console.WriteLine(emoji.Emoji);
                    }
                    else
                    {
                        var results = searcher.Search(read);
                        var emoji = results[0];
                        Console.WriteLine(emoji.Emoji);
                    }
                }
            }
        }
    }
}
