using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using ExpenseTracker.Data;
using ExpenseTracker.Records;

namespace ExpenseTracker;

class Program
{
    static async Task Main()
    {
        // We have -1 as positions, because a position of a header
        // won't ever be -1. It indicates that it couldn't find the
        // position instead, which will help us big time.
        Dictionary<string, int> headers = new()
        {
            {HeaderNames.idHeader, -1},
            {HeaderNames.dateHeader, -1},
            {HeaderNames.descriptionHeader, -1},
            {HeaderNames.amountHeader, -1},
        };

        using FileStream destination = new("./test.txt", FileMode.Open, FileAccess.ReadWrite);
        using StreamReader reader = new(destination);
        
        using MemoryStream memoryStream = new();
        using StreamWriter writer = new(memoryStream);
        
        string? line = null;
        List<string> parsedLines;
        int headerEndPosition = 0; // We need this to know at which position do headers dissapear.
        
        System.Threading.Lock lockObject = new();

        line = await reader.ReadLineAsync();
        if (line == null) return;

        parsedLines = [.. line.Split(',')]; // I don't need this after this step.

        Parallel.For(0, parsedLines.Capacity, index =>
        {
            lock(lockObject)
            {
                string parsedLine = parsedLines[index];
                
                headers[parsedLine.ToLower()] = index;
            }
        });

        headerEndPosition = line.Length+1; // +1 to accomodate for a newline.

        while ((line = await reader.ReadLineAsync()) != null)
        {
            parsedLines = [.. line.Split(',')];

            ExpenseData expenseObj = new();

            try
            {
                expenseObj.SetData(
                    int.Parse(parsedLines[headers[HeaderNames.idHeader]]),
                    DateOnly.Parse(parsedLines[headers[HeaderNames.dateHeader]]),
                    parsedLines[headers[HeaderNames.descriptionHeader]],
                    int.Parse(
                        parsedLines[headers[HeaderNames.amountHeader]].Replace(expenseObj.Currency, "")
                    )
                );

                Console.WriteLine(expenseObj.Amount);
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine($"ERROR: Couldn't find the headers. {e}\nExiting...");
                return;
            }
            
            if (expenseObj.Id == 2)
            {
                expenseObj.Description = "UPDATED!";
                await writer.WriteLineAsync(expenseObj.ToString());
                continue;
            }
            
            await writer.WriteLineAsync(line);
        }

        writer.Flush();

        memoryStream.Position = 0;
        destination.Position = headerEndPosition;
        await memoryStream.CopyToAsync(destination);
    }
}
