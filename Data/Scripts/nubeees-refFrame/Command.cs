using System.Collections.Generic;

namespace nubeees_refFrame
{
    /// <summary>
    /// Credit to Stollie's daily needs mod, which I referenced
    /// for this code.
    /// </summary>
    public class Command
    {
        public ulong sender;
        public string content;
        public List<string> contentArr;

        public Command()
        {
        }
        public Command(ulong sender, string content)
        {
            this.sender = sender;
            this.content = content;
        }
    }
}
