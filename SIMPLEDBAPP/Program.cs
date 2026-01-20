using Simpledb;

class Program
{
    static void Main()
    {
        var db = new SimpleDatabase("test.db");

        db.Put("name", "Alice");
        Console.WriteLine(db.Get("name"));
        Console.WriteLine($"1: {db.Get("name") ?? "<null>"}");
        db.Delete("name");
        Console.WriteLine($"2: {db.Get("name") ?? "<null>"}");
        db.Compact();
        Console.WriteLine($"3: {db.Get("name") ?? "<null>"}");
        db.Put("name", "Bob");   
        Console.WriteLine($"3: {db.Get("name") ?? "<null>"}");

    }
}
