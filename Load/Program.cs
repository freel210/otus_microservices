using Load;

var duration = (int)TimeSpan.FromSeconds(45).TotalMilliseconds;
CancellationTokenSource tokenSource = new (duration);

await Task.WhenAll(Enumerable.Range(1, 1).Select(async x => 
{
    LoadProvider loadProvider = new(x, "http://localhost:8000", tokenSource.Token);
    await loadProvider.GenerateLoad();
}));

Console.WriteLine("All proccesses are done.");
Console.ReadLine();


