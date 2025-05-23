using Dynastream.Fit;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Common.Fit;

public class FitFileEncoder
{
    private readonly ILogger<FitFileEncoder> _logger;

    public FitFileEncoder(ILogger<FitFileEncoder> logger)
    {
        _logger = logger;
    }

    public void Encode(Stream stream, FitMessages fitMessages)
    {
        // Create file encode object
        var encodeDemo = new Encode(ProtocolVersion.V20);

        // Write our header
        encodeDemo.Open(stream);

        // Encode each message, a definition message is automatically generated and output if necessary
        encodeDemo.Write(fitMessages.FileIdMesgs);
        encodeDemo.Write(fitMessages.DeveloperDataIdMesgs);
        encodeDemo.Write(fitMessages.FieldDescriptionMesgs);
        encodeDemo.Write(fitMessages.EventMesgs);
        encodeDemo.Write(fitMessages.DeviceInfoMesgs);
        encodeDemo.Write(fitMessages.SportMesgs);
        encodeDemo.Write(fitMessages.WorkoutMesgs);
        encodeDemo.Write(fitMessages.RecordMesgs);
        encodeDemo.Write(fitMessages.LapMesgs);
        encodeDemo.Write(fitMessages.SessionMesgs);
        encodeDemo.Write(fitMessages.ActivityMesgs);

        // Update header datasize and file CRC
        encodeDemo.Close();

        _logger.LogInformation("Encoded FIT file");
    }
}
