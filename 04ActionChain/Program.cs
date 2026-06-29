List<Func<Action, Action>> list = new();
Action app = () => Console.WriteLine("404");
for (int i = 1; i <= 7; i++)
{
    int num = i;
    Func<Action, Action> func = (next) =>
    {
        return () =>
        {
            Console.WriteLine($"{num}被调用");
            next();
        };
    };
    list.Add(func);
}
for (int i = 6; i >= 0; i--)
{
    app = list[i](app);
}
app.Invoke();