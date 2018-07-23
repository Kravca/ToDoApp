$connectionString = "Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=ToDoDB;Integrated Security=SSPI;"

$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
$connection.Open()

$query = "DELETE from ToDoItem where Completed ='true'"
$command = $connection.CreateCommand()
$command.CommandText = $query
$result = $command.ExecuteReader()
