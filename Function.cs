using LambdaFunction.Storage;

namespace CardGenerator;

public class Function
{
    private readonly IBucketStorage bucketStorage;
    
    // <summary>
    /// Constructs an instance with a preconfigured Bucket Storage. This can be used for testing outside of the Lambda environment.
    /// </summary>
    /// <param name="bucketStorage">The bucket storage manager</param>
    public Function(IBucketStorage bucketStorage)
    {
        this.bucketStorage = bucketStorage;
    }

    public async Task FunctionHandler(int cardsToGenerate)
    {
      
        try
        {
            var outputPath = $"tmp";
            var tasks = new List<Task>();
            int cpus = 8;

            for (int i = 0; i < cpus; i++)
            {
                Guid uuid = Guid.NewGuid();
                var jsonTempFile = $"{outputPath}/{uuid}.txt";
            
                tasks.Add(Task.Run(async () =>
                {
                    using (var fileStream = new FileStream($"{jsonTempFile}", FileMode.Create, FileAccess.Write))
                    using (var writer = new StreamWriter(fileStream))
                    {
                        await foreach (var cardNumber in GenerateCardNumbers(cardsToGenerate / cpus))
                        {
                            await writer.WriteLineAsync(cardNumber);
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);
            
            var jsonFiles = Directory.EnumerateFiles(outputPath, "*.txt").ToList();
            var outputFilePath = $"output_{Guid.NewGuid()}.txt";
            using (var outputStream = new FileStream($"{outputFilePath}", FileMode.Create, FileAccess.Write))
            {
                foreach (var jsonFile in jsonFiles)
                {
                    using (var inputStream = new FileStream(jsonFile, FileMode.Open, FileAccess.Read))
                    {
                        await inputStream.CopyToAsync(outputStream);
                    }
                }
            }
            
            // Clean up temporary files
            Parallel.ForEach(jsonFiles, jsonFile =>
            {
                File.Delete(jsonFile);
            });
        }
        catch (System.Exception ex)
        {
            // Log the error
            System.Console.WriteLine($"Error generating cards, exception: {ex.Message}");
        }
    }


    private async IAsyncEnumerable<string> GenerateCardNumbers(int cardsToGenerate)
    {
        int failCount = 0;
        int maxFailCount = 10000;
        do
        {        
            string cardNumber = string.Empty;

            try
            {
                // Generate a card number
                cardNumber = Guid.NewGuid().ToString();

                // Simulate a delay
                await Task.Delay(2);

                // Persist the card number
            }
            catch (System.Exception ex)
            {
                // Log the error
                System.Console.WriteLine($"Error generating card, exception: {ex.Message}");

                if (++failCount > maxFailCount)
                {
                    // Log the error
                    System.Console.WriteLine($"Error generating card, exceeded max fail count: {maxFailCount}");
                    break;
                }
            }

            if (cardNumber != null)
            {
                --cardsToGenerate;
                yield return cardNumber;
            }
        } while (cardsToGenerate > 0);        
    }
}