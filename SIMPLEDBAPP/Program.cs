using SimpleDataBase;

class Program
{
    static void Main()
    {
        var db = new SimpleDatabase("test.db");

        db.Put("name", "Alice");
        Console.WriteLine(db.Get("name"));

        db.Delete("name");
        Console.WriteLine(db.Get("name")); // null
    }
}
