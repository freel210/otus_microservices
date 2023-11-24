using OpenTelemetry.Metrics;

namespace Demo.Metrics;

public static class MeterProviderBuilderExtensions
{
    public static MeterProviderBuilder AddUpTimeInstrumentation(this MeterProviderBuilder builder)
    {
        builder.AddMeter(UpTimeMetrics.InstrumentationName);
        builder.AddInstrumentation(new UpTimeMetrics());

        return builder;
    }
}
