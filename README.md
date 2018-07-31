# ToDoApp
The goal of this project is to demonstrate how basic on-premise application can be migrated to Microsoft Azure Platform-as-a-Service (PaaS) services. 
## Visual Studio Solution contents
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
You do not need Visual Studio or developer skill for this project, all the files are already built and available for download in this project 'Published' folder.
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
 * ToDoNotifications -> WebJob in Azure App Service, also Azure Logic App
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
Alternatively we will go through the steps how to recreate ToDoNotifications service functionality with a simple Azure Logic App.
#### Setup SendGrid SMTP
1. Open https://portal.azure.com
2. In the same Resource Group, add new components called 'SendGrid Email Delivery'. Specify the name for the component and password, this password will be stored later on in configuration file for ToDoNotifications service. Choose free tier, that has 25000 emails per month, this should be enough for our Lab.
3. Navigate to newly created SendGrid account and under 'All Settings -> Configurations' make anote of username and SMTP server address.
4. In the files of ToDoNotifications service, find file 'ToDoNotifications.exe.config' and update it with Azure SQL Database connection string and SMTP server details.
5. It is reccomended to test connectifity to SQL and SMTP by launching the service executable manually and confirming that it works before we migrate it to WebJob.
> To receive notification, there shoud be a task which is already expired, and owner of the task must have email configured in his profile and 'EnableNotifications' checkbox selected
#### Create ToDoNotifications WebJob
1. Create a zip file from ToDoNotifications files
2. Open https://portal.azure.com
3. Navigate to Web App component and select 'WebJobs' menu.
4. Click on 'Add', give WebJob a name, select previously created zip file, select 'Triggered' type, select 'Scheduled' trigger and paste in Cron expression '0 */5 * * * *' (every 5 minutes).
5. You can run WebJob manually or wait max 5 minutes for it to be triggered automatically. Confirm that everything works as expected.
> Keep in mind you will be receiving annoying emails every 5 minutes according to previous setup steps, you can disable those in the user profile or adjust Cron job settings.
#### Create ToDoNotifications Logic App
1. Open https://portal.azure.com
2. Navigate to Web App component and under 'WebJobs' menu, delete ToDoNotifications WebJob so it doesnt interfere with our LogicApp.
3. In the same Resource Group create new component 'Logic App', name it 'ToDoNotifications', amke sure location is same as for other components of the solution.
4. Navigate to newly created Logic App, this should open a Logic App Designer wizard.
5. Select 'Recurrence' as a triger for the Logic App, specify 5 minutes frequency and click on '+ New step' and select 'Add an action'
6. In a search field type 'sql server' and select action 'Execute a SQL query'
7. Specify Connection Name 'ToDoDB', under SQL server name, select your Azure SQL server, and your ToDoDB. In the Username and Password fields fill out your SQL credentials and press 'Create'
> Credentials from this step are saved as a separate object (api connection type) in the Azure Resource Group.
8. In the 'query' flied paste in following query 'select Email from UserProfile Where UserName IN (SELECT Owner FROM ToDoItem Where DateDeadline < GETDATE() Group by [Owner])  and EnableNotifications = '1'' and press '+ New step' and select 'Add and action'
9. In search field type 'Office 365 outlook' and select action 'Send an email (Office 365 Outlook)'. You will have to sign in to Office 365 through Logic App, so that your credentials can be used by Logic App
>This step assumes you have Office 365 account, if not, Gmail or other email senders can be used in Logic App as well.
10. Click in the 'To' field and Logic App should offer you a dynamic content from previous Logic App steps, in our case its 'Email' parameter from SQL query. Select it. Logic App will detect that SQL query can return multiple entries for the emails and will reorganize Logic App, so that the 'Send an email step' becomes as part of the 'For each' loop.
11. In the 'Subject' field specify 'Notification from ToDo', in the 'Body' field specify 'You have tasks, that are expired. Check those out at our amazing website!' and click 'Save'.
12. To test the Logic App, either wait for 5 minutes or execute it manually by pressing 'Run Trigger' button.
## Automatic deployment to Azure
Azure Resource Manager enables you to work with the resources in your solution as a group. You can deploy, update, or delete all the resources to Azure for your solution in a single, coordinated operation. To be able to achieve this we will need to create Azure Resource Manager (ARM) template. ARM template is a Json file describing all the components and their configuration for automated deployment. To be able to create ARM template for this solution, we need to do some preparation steps listed below.
### Preparation steps
#### Web Application
Web application files should be in a "MS Deploy/Web Deploy" package format (zip file). This file can either be created from Visual Studio publishing wizard, or if there is no possibility to do that, the package can be created from published files with MS Deploy tool.
1. Download MSDeploy tool from https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2008-R2-and-2008/dd569059(v=ws.10)
2. Create on local PC folder "D:\home\site\wwwroot"
3. Copy Web Application files to folder "D:\home\site\wwwroot"
4. Run commandline 'msdeploy.exe -verb:sync -source:iisApp="D:\home\site\wwwroot" -dest:package=D:\home\site\todosite.zip'
5. The file 'D:\home\site\todosite.zip' is Web Deploy package, it is the one that will be referenced in ARM template
#### Database
For database creation with ARM template we will need database schema and data in 'bacpac' file format. How to create this file is already described in Database migration section above.
#### WebJob
For WebJobs to be created and published with ARM template we dont need a separate file package. We can include WebJob files together with Web Application 'Web Deploy' zip package.
1. Refer to Web Application guide above
2. Before running 'msdeploy.exe' commandline, create an extra folder "D:\home\site\wwwroot\App_Data\jobs\job_type\job_name". Where __job_type__ is either 'continuous' or 'triggered', and __job_name__ is the desired name for the WebJob.
3. Copy WebJob files to the created folder
4. In the case of 'triggered' WebJob, you might want to setup a schedule for the job. To do that, create an extra file in WebJob folder called 'settings.job'.
5. Paste in the following json data to 'settings.job' file, and adjust your Cron expression for the desired schedule.
```json
{
    "schedule": "0 */5 * * * *"
}
```
6. When you have added WebJob to 'D:\home\site\wwwroot\App_Data\jobs\job_type\job_name', run 'msdeploy.exe' commandline as described above, and you will get a single file containig both the Web Application files and WebJob files.
#### ARM Template
We have already created an ARM template for this solution, It can be reviewed here:
 
[azuredeploy.json](https://github.com/Kravca/ToDoApp/blob/master/azuredeploy.json)

Here are some tips and tricks regarding ARM template:
1. The package files used in ARM template (Webdeploy, bacpac,..) need to be referenced from Azure Blob storage (it is not supported to use references from github)
2. Sometimes, its easier to start by creating all the objects manually in the Azure portal and then use 'Automation script' menu on the resource group, to extract auto-generated ARM template, instead of starting from blank Json.
3. If Web/Site object in the ARM template has site configuration section, application settings sections and MSDeploy extension, it is reccomended that site config section and application settings are dependant (deployed in the sequence after) on MSDeploy extension
4. __DO NOT__ store sensitive information like username, passwords and keys in the ARM template, at least allow to change those through parameters section 
5. Pay attention to 'apiVersion' for every resource as syntaxes and functionality might be different for different 'apiVersions'.
6. If you plan to deploy multiple instances of the same solution into the same Azure Subscription, you might need somekind of randomly generated values, those can be achieved by using [uniqueString(resourceGroup().id)] function. See ARM template for sample. More on ARM template function here - https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-template-functions.

If you want to deploy it to your Azure subscription you can do so, by pressing following button:

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

The following items will be created in Azure subscription:
1. App Service Plan
2. App Service (WebApp + WebJob)
3. Azure SQL Server
4. Azure SQL Database
5. Logic App (ToDoCleanUp)
6. API Connection (Logic App to Azure SQL Server)

>Specifying App Service authentication settings is supported by ARM templates, but unfortunately it is not supported to create Azure AD applications with ARM template. Therefore, after deployment Authentication need to be enabled manually by following these steps [Enable Authentication](./README.md#setup-azure-app-service-authentication) 

## Modernize App with Windows Containers
Check out this repository to gain knowledge on Windows Containers and how to modernize apps with those:

https://github.com/dotnet-architecture/eShopModernizing



