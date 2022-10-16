using Microsoft.Data.Sqlite;

string cs = @"data source=dsstats2.db";
string stm = "SELECT SQLITE_VERSION()";

using var con = new SqliteConnection(cs);

//SQLitePCL.Batteries.Init()

con.Open();

using var cmd = new SqliteCommand(stm, con);
var schema = con.GetSchema();


Console.WriteLine("");

//cmd.CommandText = "DROP TABLE IF EXISTS cars";
//cmd.ExecuteNonQuery();

//cmd.CommandText = @"CREATE TABLE cars(id INTEGER PRIMARY KEY,
//            name TEXT, price INT)";
//cmd.ExecuteNonQuery();

//cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Audi',52642)";
//cmd.ExecuteNonQuery();

//cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Mercedes',57127)";
//cmd.ExecuteNonQuery();

//cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Skoda',9000)";
//cmd.ExecuteNonQuery();

//cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Volvo',29000)";
//cmd.ExecuteNonQuery();

//cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Bentley',350000)";
//cmd.ExecuteNonQuery();

//cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Citroen',21000)";
//cmd.ExecuteNonQuery();

//cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Hummer',41400)";
//cmd.ExecuteNonQuery();

//cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Volkswagen',21600)";
//cmd.ExecuteNonQuery();