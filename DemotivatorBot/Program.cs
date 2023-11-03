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
// Color and font choice

string botkey = "";

try
{
    // Open the text file using a stream reader.
    using (var sr = new StreamReader(@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\botkey.txt"))
    {
        // Read the stream as a string, and write the string to the console.
        botkey = sr.ReadToEnd();
    }
}
catch (IOException e)
{
    Console.WriteLine("The file could not be read:");
    Console.WriteLine(e.Message);
    Console.ReadKey();
}

var botClient = new TelegramBotClient(botkey);

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
    Logger logger = new Logger();
    string sendMePicMessage = "Please submit a JPG document with a caption (this will be the caption on your image).";

    //Text

    if (message.Text != null)
    {
        try
        {
            logger.Log(new TextLog(), message);
            await botClient.SendTextMessageAsync(chatId, sendMePicMessage);
            return;

        }
        catch
        {
            Console.WriteLine($"{chatId}: {message.Chat.FirstName}   |TEXT ERROR");
            return;
        }
    }

    //Document and Photo
    if (message.Document != null || message.Photo != null)
    {
        if (message.Caption == null) 
        {
            try
            {
                logger.Log(new DocNoCaptionLog(), message);
                await botClient.SendTextMessageAsync(chatId, sendMePicMessage);
                return;
            }
            catch
            {
                return;
            }
            
        }

        string fileId;
        string caption = message.Caption;

        if (message.Photo != null)
        {
            logger.Log(new PhotoCaptionLog(), message);
            fileId = message.Photo.Last().FileId;
        }
        else
        {
            logger.Log(new DocCaptionLog(), message);
            fileId = message.Document.FileId;
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

        await using Stream fileStream = System.IO.File.OpenWrite(destinationFilePath); //Download file
        await botClient.DownloadFileAsync(filePath, fileStream);
        fileStream.Close();

        await Task.Run(() => d.Demotivate()); //Create demotivator

        await using Stream stream = System.IO.File.OpenRead(d.ResultPath); //Send file
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
