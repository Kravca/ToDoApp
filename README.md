# ToDoApp
The goal of this project is to demonstrate how basic on-premise application can be migrated to Microsoft Azure Platform-as-a-Service (PaaS) services. 
## Solution contents
* ContosoToDo - Web UI for task management
* ToDoCleanUp - Console app for cleaning up completed tasks
* ToDoNotifications - Console app for sending emails to users with expired tasks
## Installation instructions
### Prerequisites
* Server 2012 R2 (Domain joined)
### Server configuration
* Install .Net Framework 4.6.2
* Add Web Server (IIS) role
  * "SMTP Server" feature, if doesnt exist already
  * "Windows Authentication" Role Services
* Install SQL Server Express Edition
 * Download from https://www.microsoft.com/en-us/sql-server/sql-server-downloads
 * Make sure "Database Engine Services" is installed
 * Enable Mixed Mode Authentication
 * Use "Default instance"
* Install SQL Server Management Studio
 * Download from https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-2017
### Installing Solution on Server
#### Database
1. Connect to SQL Server with SQL Management Studio (SSMS)
2. Create new blank database called "ToDoDB"
3. Right click on database and under 'Tasks' choose 'Upgrade Data-tier Application...'
4. Select provided 'ToDoDB.dacpac' file and complete the upgrade wizard
5. Using SSMS create new login on the server
5.1 select SQL Server Authentication
5.2 specify login name - todosql
5.2 specify pass - TodoPassword1!
5.3 for this lab purposes remove checkboxes related to password policies
5.4 select default database 'ToDoDB'
5.5 under 'User Mapping' select ToDODB database and select 'db_owner' role for it
  
  
  
  ![Screenshot](subfolder/screenshot.png)
