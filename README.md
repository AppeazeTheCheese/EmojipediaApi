# EmojipediaApi
Unofficial Emojipedia API for written in C#.

Currently retrieves:
<ul>
  <li>Emoji (as string)</li>
  <li>Unicode Name</li>
  <li>Apple Name</li>
  <li>"Also Known As" names</li>
  <li>Emoji Description</li>
  <li>URL to the Emojipedia page</li>
</ul>

Examples:

**Retrieve emoji by name:**
```c#
private static void Main()
{
  var searcher = new EmojiSearcher();
  var results = searcher.Search("smile");
  if(results.Length > 0){
    var desiredResult = results[0];
    Console.WriteLine(desiredResult.Emoji);
    var info = desiredResult.getEmojiInfo();
    Console.WriteLine(info.Description);
  }
  else
    Console.WriteLine("No search results returned.");
}
```
**Retrieve random emoji:**
```c#
private static void Main()
{
  var searcher = new EmojiSearcher();
  var emojiInfo = searcher.Random();
  Console.WriteLine(emojiInfo.Emoji);
  Console.WriteLine(emojiInfo.Description);
}
```
