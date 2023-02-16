using System.IO;
using DemotivatorBot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

var botClient = new TelegramBotClient(//key);

using CancellationTokenSource cts = new();

ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>()
};

botClient.StartReceiving(
    HandleUpdateAsync,
    HandlePollingErrorAsync,
    receiverOptions,
    cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

//Send cancellation request to stop
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Message is not { } message)
        return;

    var chatId = message.Chat.Id;

    if (message.Text != null)
    {
        string messageText = message.Text;
        Console.WriteLine($"{chatId}: {message.Chat.FirstName}   |Text: {messageText}");
        return;
    }

    if (message.Photo != null)
    {
        await botClient.SendTextMessageAsync(
            chatId,
            "Пожалуйста, отправьте документом в формате JPG с описанием(оно будет подписью на вашем изображении).");
        Console.WriteLine($"{chatId}: {message.Chat.FirstName}   |Photo");
        return;
    }

    if (message.Document != null)
    {
        if (message.Caption == null) 
        {
            await botClient.SendTextMessageAsync(
            chatId,
            "Пожалуйста, отправьте документом в формате JPG с описанием(оно будет подписью на вашем изображении).");
            return;
        }

        string caption = message.Caption;
        Console.WriteLine($"{chatId}: {message.Chat.FirstName}   |Caption: {message.Caption}");

        var fileId = message.Document.FileId;
        var fileInfo = await botClient.GetFileAsync(fileId);
        var filePath = fileInfo.FilePath;

        if (filePath == null)
        {
            Console.WriteLine("filePath null");
            return;
        }

        string destinationFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\picsinput\{message.Document.FileName}";

        Demotivator d = new Demotivator(caption, destinationFilePath);

        await using Stream fileStream = System.IO.File.OpenWrite(destinationFilePath);
        await botClient.DownloadFileAsync(filePath, fileStream);
        fileStream.Close();

        d.Demotivate();

        await using Stream stream = System.IO.File.OpenRead(d.ResultPath);
        await botClient.SendDocumentAsync(chatId, new InputOnlineFile(stream, "pic.jpg"), "Вот ваша картинка.");

        stream.Close();

        return;
    }
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
