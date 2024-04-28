using System.Diagnostics;

namespace AspNetCore_OpenTelemetry_Studies;

public static class Telemetry
{
    public static readonly string ServiceName = "AspNetCore_OpenTelemetry_Studies";

    // ActivitySource = which will be how you trace operations with Activity elements
    // ActivitySource is instantiated once per app/service that is being instrumented
    // it’s a good idea to instantiate it once in a shared location.
    // It is also typically named the same as the Service Name.
    public static readonly ActivitySource ActivitySource = new(ServiceName);
}