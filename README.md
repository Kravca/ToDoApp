# ToDoApp
The goal of this project is to demonstrate how basic on-premise application can be migrated to Microsoft Azure Platform-as-a-Service (PaaS) services. 
## Solution contents
* ContosoToDo - Web UI for task management
* ToDoCleanUp - Console app for cleaning up completed tasks
* ToDoNotifications - Console app for sending emails to users with expired tasks
## Installation instructions (on-premise)
### Prerequisites
* Windows Server 2012 R2 (Domain joined), for lab demonstration purposes, nothing should be installed it
### Server configuration
* Install .Net Framework 4.6.2
* Add Web Server (IIS) role
  * "SMTP Server" feature, if doesnt exist already
  * ".NET Framework 4.5 Features/ASP.NET 4.5" feature
  * "Windows Authentication" Role Services
  * "Application Development/ASP.NET 4.5" Role Services
* Install SQL Server Express Edition
 * Download from https://www.microsoft.com/en-us/sql-server/sql-server-downloads
 * Make sure "Database Engine Services" is installed
 * Enable Mixed Mode Authentication
 * Use "Default instance"
* Install SQL Server Management Studio
 * Download from https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-2017
### Installing solution components
#### Database
1. Connect to SQL Server with SQL Management Studio (SSMS)
2. Create new blank database called "ToDoDB"
3. Right click on database and under 'Tasks' choose 'Upgrade Data-tier Application...'
4. Select provided 'ToDoDB.dacpac' file and complete the upgrade wizard
5. Using SSMS create new login on the server
  * select SQL Server Authentication
  * specify login name - todosql
  * specify pass - TodoPassword1!
  * for this lab purposes remove checkboxes related to password policies
  * select default database 'ToDoDB'
  * under 'User Mapping' select ToDODB database and select 'db_owner' role for it
#### Web Application
1. Open C:\inetpub\wwwroot folder and delete files there
2. Copy over all files of the website to C:\inetpub\wwwroot
3. Open 'IIS Manager' console and in 'Default Web Site' 'Authentication' configuration disable 'Anonymous Authentication' and enable 'Windows Authentication'
4. Open C:\inetpub\wwwroot\web.config file and under <configuration><connectionStrings> modify 'ToDoItemContext' entry by changing its connection string to "Data Source=localhost;Initial Catalog=ToDoDB;User Id=todosql;Password=TodoPassword1!;", replace values to yours if you didn't follow this guide
5. Open Internet Explorer or any other browser and navigate to http://localhost/, try checking 'My Tasks' and 'My Profile' areas to verify that there are no issues with SQL database connection
![Screenshot](pics/startpage.PNG)
#### SMTP server configuration (optional)
1. Open 'IIS 6.0 Manager' console, not the 'IIS Manager'
2. Right click on SMTP server and start it, if its not started
3. Under SMTP server properties, on 'General' tab, choose assigned IP. On 'Access' tab, click 'Relay' button and add two IP addresses, one 127.0.0.1 and the one you selected on 'General' tab. Close SMTP server properties.
#### Notification service
1. Create folder on a server f.e. c:\todo\notifications
2. Copy over published files of ToDoNotifications console app
3. Open "C:\todo\notifications\ToDoNotifications.exe.config" file and edit its connection string to the same as used in Web Application, update SMTP server setting, with your server name.
4. Create a scheduled task that runs 'ToDoNotifications.exe' executable on regular basis.
5. To test service, open ToDo Web Application, update your profile with proper email and enable email notifications. Create a task that is not completed and that has an expired deadline. Run "ToDoNotifications.exe" (without scheduled task), make sure it says 'Completed' without any errors. Check your email if you have received an email (might be in Spam folder)
#### CleanUp service
1. Create folder on a server f.e. c:\todo\cleanup
2. Copy over published powershell file
3. Update $connectionString variable in the PowerShell script file with connection string that was used with the Web Application
3. Create a scheduled task that runs 'ToDoCleanUp.ps1' script on regular basis.
4. To test the script, create new or update any task by setting 'Completed' flag on it. When PowerShell script is executed it deletes all tasks that are completed.
## Migrating components to Azure
The following steps describe how to transform and migrate this simple ToDo application to Microsoft Azure. Migration paths:
 * Web Application -> WebApp in Azure App Service
 * Windows Authentication -> App Service Authentication
 * Database -> Azure SQL Database
 * ToDoNotifications -> WebJob in Azure App Service
 * ToDoCleanUp -> Azure Function
 * SMTP Server -> SendGrid for Microsoft Azure (3rd party)
### Database
One of the easiest and quickest way to migrate on-premise SQL database to Azure is by exporting Data-tier Application '*.bacpac' file and importing it into Azure SQL server. This can also be done by using simplified 'Deploy Database to Microsoft Azure SQL Database' wizard in SQL Management Studio. Both of these approaches use '*.bacpac' file, and it is strongly reccomended to store this file for later use, as it can be reused during auto-provisioning of the solution in Azure Resource Manager (ARM) templates. '*.bacpac' file contains both, database schema and data.
1. Connect to SQL server containing source database
2. Before exporting database, make sure to remove any unnecesary data from it, as this database will be used as a template database for cloud solution. Do not cleanup 'dbo._MigrationHistory' table.
3. Right click on the database and choose 'Tasks -> Export Data-tier Application'
4. Specify location for '*.bacpac' file and complete the wizard
#### Setup Azure SQL Database
1. Open https://portal.azure.com
2. Create a new Resource Group for your cloud solution
3. Create in the resource group new 'SQL server (logical server)'
 * specify Azure SQL server name
 * specify admin username and password
 * pay attention to resource location, all solution resources should be in the same location
4. Connect with SSMS to Azure SQL server, with previously specified credentials
5. By default, Azure SQL firewall will block the connection and SSMS will ask to add your current IP address to allowed IP rules, sign in with this wizard to add your IP
6. Right click 'Databases' foled and select 'Import Data-tier Application'
7. Specify previously exported '*.bacpac' file, choose the right size for database (basic is enough for this lab) and complete import wizard
8. To test the database update connection string in all on-premise components to point them to new Azure SQL database. Connection string should be in a format 'Server=tcp:{your_server},1433;Initial Catalog=ToDoDB;Persist Security Info=False;User ID={your_username};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
> Keep in mind Azure SQL server firewall rules, if your on-prem web application or other components are reaching server from different IP addresses, those should be added to firewall as well
### Web Application
The easiest way to migrate on-premise web application to Azure App Service is by simply copying files over to Azure Web App though FTP. But it is reccomended to create a WebDeploy package, as it can be reused during auto-provisioning of the solution in Azure Resource Manager (ARM) templates.
#### Setup Azure App Service Web App
1. Open https://portal.azure.com
2. In the same Resource Group create 'App Service Plan', make sure location is the same as for database. For this lab B1 (basic) tier is enough.
3. In the same Resource Group create 'Web App', specify to host it in the previously created 'App Service Plan'.
4. Navigate to newly created Web App and click on 'Deployment Credentials' menu. Set your desired username and password for FTP access.
> These credentials will work for all Web Apps, where your azure account has access to, not just this single lab web app.
5. Navigate to 'Overview' menu of the Web App, and copy out 'FTP hostname' address, also pay attention to your 'FTP/deployment username' value as it will be slightly different.
6. Open File Explorer and past 'FTP hostname' in the address bar
7. Type in 'FTP/deployment username' and password
8. Browse to 'site/wwwroot', delete any files tehre, and copy over files from your on-premise server c:\inetpub\wwwroot folder.
9. If you try to open Web App URL address, you should see 'You do not have permission to view this directory or page.', that is because we have not authenticated, but anonymous access is not allowed. Lets set up authentication in next section.
#### Setup Azure App Service Authentication
1. Open https://portal.azure.com
2. Navigate to Web App object and click on menu 'Authentication / Authorization'
> For this step your Azure account must have enough permissions to create Azure AD application
3. Click 'ON' under 'App Service Authentication, select 'Log in with Azure Active Directory' under 'Action to take when request is not authenticated', click on 'Azure Active Directory' provider, choose 'express' setting and confirm your choices by clicking 'OK' and 'Save'
> This step creates new Azure Active Directory application, access to Web Application can be controlled through this application definition, by default, it allows all users from your Azure Active Directory to access this application
4. To test application authentication, open again Web App URL, you should see that you are asked to authenticate with your Azure AD account. You should also see consent request from Azure AD application, click 'Accept' and you should be able to access your cloud application. Check application functionality, pay attention to your cloud username.
### ToDoCleanUp
Although it is possible to use multiple approaches to do migration of the 'ToDoCleanUp' component, such as Azure Functions, App Service WebJob or Azure Logic App, for this lab we will be demonstarting Azure Function approach.
1. Open https://portal.azure.com
2. In the same Resource Group, where other components already reside, create new component 'Function App'. Select 'App Service Plan' as a hosting plan and choose the same plan that was used for Web App.
3. Navigate to newly create Function App, click on 'Functions' menu and select 'New function'
4. Turn On 'Experimental Language Support' and select 'PowerShell' from 'Language' drop-down. Choose 'Timer Trigger'. Leave default 'every 5 minutes' trigger and create a Function.
5. In the online editor, leave first command line intact and copy over all comands from 'ToDoCleanUp.ps1'
6. Make sure you update your Connection string with the one for Azure SQL Database and save the script
> Best practice would be to read the connection string from fuction app application settings, so that it is not stored directly in the script
7. To test the function, create a new task in ToDo application, make sure it is completed. Either 'Run' you Azure Function manually or wait max 5 minutes for it to be triggered automatically. Confirm that completed tasks are deleted.
### ToDoNotifications
For ToDoNotifications service to send emails, we will need SMTP server. Although usually you should be able to reuse same on-premise service, for this Lab we will setup cloud SMTP account from 3rd party provider 'SendGrid'.
#### Setup SendGrid SMTP
1. Open https://portal.azure.com
2. In the same Resource Group, add new components called 'SendGrid Email Delivery'. Specify the name for the component and password, this password will be stored later on in configuration file for ToDoNotifications service. Choose free tier, that has 25000 emails per month, this should be enough for our Lab.
3. Navigate to newly created SendGrid account and under 'All Settings -> Configurations' make anote of username and SMTP server address.
4. In the files of ToDoNotifications service, find file 'ToDoNotifications.exe.config' and update it with Azure SQL Database connection string and SMTP server details.
5. It is reccomended to test connectifity to SQL and SMTP by launching the service executable manually and confirming that it works before we migrate it to WebJob.
> To recieve notification, there shoud be a task which is already expired, and owner of the task must have email configured in his profile and 'EnableNotifications' checkbox selected
#### Create ToDoNotifications WebJob
1. Create a zip file from ToDoNotifications files
2. Open https://portal.azure.com
3. Navigate to Web App component and select 'WebJobs' menu.
4. Click on 'Add', give WebJob a name, select previously created zip file, select 'Triggered' type, select 'Scheduled' trigger and paste in Cron expression '0 */5 * * * *' (every 5 minutes).
5. You can run WebJob manually or wait max 5 minutes for it to be triggered automatically. Confirm that everything works as expected.
