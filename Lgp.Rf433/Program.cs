// See https://aka.ms/new-console-template for more information

using System.Device.Gpio;
using System.Timers;

while (!System.Diagnostics.Debugger.IsAttached)
{
    Thread.Sleep(100);
}
Console.WriteLine("debugger attached");
System.Diagnostics.Debugger.Break();

const int Pin = 2;

using var controller = new GpioController();

using var pin = controller.OpenPin(Pin, PinMode.Input);


List<byte> bytes = [];

int bufferTimeout = 500;

using System.Timers.Timer timer = new(bufferTimeout) { AutoReset = true };
timer.Elapsed += TimerCallback;

void TimerCallback(object? sender, ElapsedEventArgs e)
{
    if (bytes.Count == 0) return;
    Console.WriteLine(Convert.ToBase64String(bytes.ToArray()));
    bytes = [];
}

byte byteCount = 0;

void OnPinEvent(object sender, PinValueChangedEventArgs args)
{
    timer.Stop(); timer.Start();
    switch (args.ChangeType)
    {
        case PinEventTypes.None:
            Console.WriteLine("none");
            break;
        case PinEventTypes.Rising:
            bytes.Add(byteCount);
            byteCount = 0;
            break;
        case PinEventTypes.Falling:
            byteCount++;
            break;
        default:
            Console.WriteLine("default");
            break;
    }
}

controller.RegisterCallbackForPinValueChangedEvent(
    Pin,
    PinEventTypes.Falling | PinEventTypes.Rising,
    OnPinEvent);

await Task.Delay(Timeout.Infinite);
