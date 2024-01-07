using Load;

var duration = (int)TimeSpan.FromMinutes(20).TotalMilliseconds;
CancellationTokenSource tokenSource = new (duration);

await Task.WhenAll(Enumerable.Range(1, 2).Select(async x => 
{
    LoadProvider loadProvider = new(x, "http://localhost:8000", tokenSource.Token);
    await loadProvider.GenerateLoad();
}));

Console.WriteLine("All proccesses are done.");
Console.ReadLine();


