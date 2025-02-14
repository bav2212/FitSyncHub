using Dynastream.Fit;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Common.Fit;

public class FitFileDecoder
{
    private readonly ILogger<FitFileDecoder> _logger;

    public FitFileDecoder(ILogger<FitFileDecoder> logger)
    {
        _logger = logger;
    }

    public FitMessages Decode(Stream stream)
    {
        try
        {
            var decoder = new Decode();

            // Use a FitListener to capture all decoded messages in a FitMessages object
            var fitListener = new FitListener();
            decoder.MesgEvent += fitListener.OnMesg;

            _logger.LogInformation("Decoding...");
            decoder.Read(stream);

            var fitMessages = fitListener.FitMessages;
            return fitMessages;
        }
        catch (FitException ex)
        {
            _logger.LogError("A FitException occurred when trying to decode the FIT file. Message: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception occurred when trying to decode the FIT file. Message: {Message}", ex.Message);
            throw;
        }
    }

    public FitMessages Decode(string fileName)
    {
        // Attempt to open .FIT file
        using var fitSource = new FileStream(fileName, FileMode.Open);
        _logger.LogInformation("Opening {FileName}", fileName);

        return Decode(fitSource);
    }





    //public FitMessages Decode(string fileName)
    //{
    //    try
    //    {
    //        // Attempt to open .FIT file
    //        using var fitSource = new FileStream(fileName, FileMode.Open);
    //        _logger.LogInformation("Opening {FileName}", fileName);

    //        var decoder = new Decode();

    //        // Use a FitListener to capture all decoded messages in a FitMessages object
    //        var fitListener = new FitListener();
    //        decoder.MesgEvent += fitListener.OnMesg;

    //        _logger.LogInformation("Decoding...");
    //        decoder.Read(fitSource);

    //        var fitMessages = fitListener.FitMessages;
    //        return fitMessages;
    //    }
    //    catch (FitException ex)
    //    {
    //        Console.WriteLine("A FitException occurred when trying to decode the FIT file. Message: " + ex.Message);
    //        throw;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine("Exception occurred when trying to decode the FIT file. Message: " + ex.Message);
    //        throw;
    //    }
    //}
}
