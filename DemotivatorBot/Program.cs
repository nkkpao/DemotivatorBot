using System;
using System.IO;
using System.IO.Enumeration;
using DemotivatorBot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

//-------------------------
// TODO
// Logging
// Color and font choice
// iPhone HEIF format support
// Random hash codes - done
//-------------------------  

var botClient = new TelegramBotClient("5897472120:AAGGvBDZki8avHeY9Nr1NiVsTzzrHkB_ENs");

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
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Message is not { } message) { return; }
    var chatId = message.Chat.Id;

    //Text
    if (message.Text != null)
    {
        Console.WriteLine($"{chatId}: {message.Chat.FirstName}   |Text: {message.Text}");
        await botClient.SendTextMessageAsync(
            chatId,
            "Please submit a JPG document with a caption (this will be the caption on your image).");
        return;
    }

    //Document and Photo
    if (message.Document != null || message.Photo != null)
    {
        if (message.Caption == null) 
        {
            Console.WriteLine($"{chatId}: {message.Chat.FirstName}   |No caption document");
            await botClient.SendTextMessageAsync(
            chatId,
            "Please submit a JPG document or Photo with a caption (this will be the caption on your image).");
            return;
        }

        string fileId;
        string caption = message.Caption;

        if (message.Photo != null)
        {
            Console.WriteLine($"{chatId}: {message.Chat.FirstName}   |Photo with caption: {message.Caption}");
            fileId = message.Photo.Last().FileId;
        }
        else
        {
            fileId = message.Document.FileId;
            Console.WriteLine($"{chatId}: {message.Chat.FirstName}   |Document with caption: {message.Caption}");
        }

        var fileInfo = await botClient.GetFileAsync(fileId);
        var filePath = fileInfo.FilePath;

        if (filePath == null)
        {
            Console.WriteLine("FilePath null");
            return;
        }

        string destinationFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\picsinput\{System.Guid.NewGuid()}.jpg";

        Demotivator d = new Demotivator(caption, destinationFilePath);

        await using Stream fileStream = System.IO.File.OpenWrite(destinationFilePath);
        await botClient.DownloadFileAsync(filePath, fileStream);
        fileStream.Close();

        await Task.Run(() => d.Demotivate());

        await using Stream stream = System.IO.File.OpenRead(d.ResultPath);
        await botClient.SendDocumentAsync(chatId, new InputOnlineFile(stream, $@"output.jpg"));

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
