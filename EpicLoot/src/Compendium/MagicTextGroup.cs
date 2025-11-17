using System.Linq;

namespace EpicLoot.Compendium;

public class MagicTextGroup(MagicTextElement title, params MagicTextElement[] content)
{
    public readonly MagicTextElement Title = title;
    public readonly MagicTextElement[] Content = content;
    public bool IsMatch(string query) => Title.IsMatch(query) || Content.Any(x => x.IsMatch(query));
    public void Enable(bool enable)
    {
        Title.Enable(enable);
        foreach (MagicTextElement element in Content)
        {
            element.Enable(enable);
        }
    }
}