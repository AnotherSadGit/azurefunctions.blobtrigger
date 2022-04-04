Reading from the Application Settings in an Azure Function
==========================================================
Simon Elms, 26 Mar 2022

Warning: 
--------
Application settings are saved at Function App level, not at the level of an individual Azure Function.  So they will be shared by all Azure Functions within the Function App.  To view the Application settings in the Azure Portal browse to the Function App and, from the LH blade menu, Settings section, select Configuration.

Adding BlobPath to Application Settings
---------------------------------------
### References:
* Stackoverflow question "Azure Blob Trigger. Configure Blob Path in Configuration Files", https://stackoverflow.com/questions/50059407/azure-blob-trigger-configure-blob-path-in-configuration-files

### Introduction
The BlobPath is the path in Azure Blob Storage that the Azure Function will be triggered by.  If any file on that path changes (eg if a file is added, deleted or overwritten) the Azure Function will run.

The BlobPath is a read-only property of the BlobTriggerAttribute, which can only be set in the BlobTriggerAttribute constructor. 

When the Azure Function is created (after the function has been renamed), the out of the box BlogTriggerAttribute looks like:
`public void Run([BlobTrigger("samples-workitems/{name}", Connection = "AzureStorageConnectionString")] Stream myBlob, ...)`

To avoid hard-coding the BlobPath it can be added to the Application Settings.

### Adding BlobPath to Application Settings
1. In the local.settings.json file, add a new setting.  For example, call it "BlobPath".  Set the value to the container and folder paths part of the BlobPath in the BlobTriggerAttribute constructor.  In this example just the container name "samples-workitems", since there is no folder;

2. Change the BlobPath in the BlobTriggerAttribute constructor to read the container and folder paths from the settings.  It's the same syntax as reading an environment variable in the cmd prompt: '`%{setting name}%`'.  In this example it will now be:
	`public void Run([BlobTrigger("%BlobPath%/{name}", Connection = "AzureStorageConnectionString")]Stream myBlob, ...)`

3. Copy the new settings from Local settings to Remote settings, via the Publish tab > Application settings dialog:
	a. Open the Publish tab (Solution Explorer, right-click on project, select Publish);
	b. In the Hosts section click the ellipsis button then select 'Manage Azure App Service settings' - the Application settings dialog will open;
	c. In the Application settings dialog find the new setting, in this example 'BlobPath', set the cursor in the Remote textbox, then click the link 'Insert value from Local' to copy the value from the Local setting to the Remote one;
	d. Click OK to close the Application settings dialog and return to the Publish tab.

4. Publish the update via the Publish tab > Publish button.

5. When the updated function is tested it works, with the log output being:

```
	2022-03-25T23:58:39.882 [Information] Executing 'BlobTrigger' (Reason='New blob detected: samples-workitems/Test.txt', Id=6ed9d275-8472-4aa4-b62e-0987de5e5bae)
	2022-03-25T23:58:39.886 [Information] Trigger Details: MessageId: baab008b-9e8a-4e30-8c60-28c0dca96b2d, DequeueCount: 1, InsertionTime: 2022-03-25T23:58:39.000+00:00, BlobCreated: 2022-03-25T10:37:28.000+00:00, BlobLastModified: 2022-03-25T23:58:32.000+00:00
	2022-03-25T23:58:39.898 [Information] C# Blob trigger function Processed blobName:Test.txtSize: 15 Bytes
	2022-03-25T23:58:39.975 [Information] Executed 'BlobTrigger' (Succeeded, Id=6ed9d275-8472-4aa4-b62e-0987de5e5bae, Duration=99ms)
```

The line starting "C# Blob trigger function Processed blob..." is written to the log by the function.  It indicates the function was triggered by the uploading of file 'Test.txt' which is correct - that file was uploaded to container 'samples-workitems' to test the function.
	
Reading from the Application Settings in the Function
-----------------------------------------------------
### Reference:
* Stackoverflow question "Reading settings from a Azure Function", https://stackoverflow.com/questions/43556311/reading-settings-from-a-azure-function

The way in Azure Functions v2 is to read the settings via the ASP.NET Core Configuration system. 

The way in Azure Functions v1 was to read the settings as environment variables.  For example:

	`var value = Environment.GetEnvironmentVariable("your_key_here");`

	or 

	`var value = Environment.GetEnvironmentVariable("your_key_here", EnvironmentVariableTarget.Process);`

### Using the ASP.NET Core Configuration system to read settings
1. Add a using statement: `using Microsoft.Extensions.Configuration;`

2. Add the ExecutionContext to the Azure Function method.  For example, change the method signature from:

	`public void Run([BlobTrigger("%BlobPath%/{name}", Connection = "AzureStorageConnectionString")]Stream myBlob, 
            string name, ILogger log)`

	to	

	`public void Run([BlobTrigger("%BlobPath%/{name}", Connection = "AzureStorageConnectionString")]Stream myBlob, 
            string name, ILogger log, ExecutionContext context)`

3. Add a method to get the IConfiguration Root.  For example:

	```
	public IConfigurationRoot GetConfigurationRoot(ExecutionContext context)
    {
        var configRoot = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        return configRoot;
    }
	```

4. Use the IConfiguration Root to access the Application settings (NOTE: In local.settings.json these settings are not in the root of the JSON document, they're in the "Values" section.).  For example:

    ```
	public void Run([BlobTrigger("%BlobPath%/{name}", Connection = "AzureStorageConnectionString")]Stream myBlob, 
        string name, ILogger log, ExecutionContext context)
    {
        log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

        var configRoot = GetConfigurationRoot(context);
        var blobPath = configRoot.GetValue<string>("BlobPath");
        log.LogInformation($"Value of BlobPath setting: '{blobPath}'");
    }
	```

Reading Settings Via the Options Pattern
----------------------------------------
See "The Options Pattern with Dependency Injection" section of [README_DependencyInjection.md](/README_DependencyInjection.md).