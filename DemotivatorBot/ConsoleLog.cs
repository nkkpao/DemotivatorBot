using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DemotivatorBot
{
    internal interface IConsoleLog
    {
        void Logging(Message message);
    }

    internal class TextLog : IConsoleLog
    {
        public void Logging(Message message)
        {
            Console.WriteLine($"{message.Chat.Id}: {message.Chat.FirstName}   |Text: {message.Text}");
        }
    }
    internal class PhotoLog : IConsoleLog
    {
        public void Logging(Message message)
        {
            Console.WriteLine($"{message.Chat.Id}: {message.Chat.FirstName}   |Photo"); ;
        }
    }
    internal class DocCaptionLog : IConsoleLog
    {
        public void Logging(Message message)
        {
            Console.WriteLine($"{message.Chat.Id}: {message.Chat.FirstName}   |Document with caption: {message.Caption}");
        }
    }

    internal class DocNoCaptionLog : IConsoleLog
    {
        public void Logging(Message message)
        {
            Console.WriteLine($"{message.Chat.Id}: {message.Chat.FirstName}   |No caption document");
        }
    }
}
