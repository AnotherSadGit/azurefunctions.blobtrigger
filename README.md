Azure Function with Blob Trigger
================================
Simon Elms, 9 Mar 2022

References
----------
Based on a combination of:

1. "Quickstart: Create your first C# function in Azure using Visual Studio", https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-your-first-function-visual-studio - Azure Function with Http Trigger created in Visual Studio

2. "Create a function in Azure that's triggered by Blob storage", https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-storage-blob-triggered-function - Azure Function with Http Trigger created in Azure Portal, not in Visual Studio

3. "How to Create Azure Blob Trigger Functions", https://andrewhalil.com/2020/06/28/creating-azure-blob-trigger-functions/ - Azure Function with Blob Trigger created in Visual Studio

General Notes
-------------
### Warning: 
The Visual Studio project containing the Azure Function class represents the Function App, not the function.  This means that the host.json file, the local.settings.json file and the Application settings in the Azure Portal apply at the Function App level, not at the level of an individual Azure Function.  So they will be shared by all Azure Functions within the Function App.  

In addition, when you publish your project you're publishing all the functions within it, not just a single function.

### local.settings.json and Git
By default the local.settings.json file, containing the versions of the Application settings used when running locally in Visual Studio, is EXCLUDED from the Git repository by a local .gitignore file that generated automatically when the project is created in Visual Studio.

### Viewing Settings in the Azure Portal
To view the Application settings browse to the Function App and, from the LH blade menu, Settings section, select Configuration.

To view the contents of the host.json file browse to the Function App and, from the LH blade menu, Functions section, select 'App files'.

Creating the Project
--------------------
1. When creating the Azure Function > Blob Trigger project in Visual Studio, set:
	* Storage account: Can browse to it.  For this example we're using 'apidefaultaustralia9e44';
	* Connection string setting name: {Value of the BlobTrigger attribute Connection property} - this is the local.settings.json key the connection string will be stored against, not the connection string itself, eg 'AzureStorageConnectionString';
	* Path: {Blob Storage container name}[/{sub-folder}] - defaults to container name 'sample-workitems'.  This will be added to the BlobTrigger attribute default property (= the BlobPath property).

2. The local.settings.json file will be created automatically with the project.  Two settings will be created automatically with the json file:
	* AzureWebJobsStorage;
	* FUNCTIONS_WORKER_RUNTIME

3. A boilerplate class will be added to the project called Function1.cs.  Rename file and class to something more appropriate, eg BlogTrigger.

4. Change the value of the FunctionName attribute on the Run method of the class from "Function1" to something more appropriate, eg "BlobTrigger".

Configuring the Project
-----------------------
1. Add the connection string key that was used when creating the project to the local.settings.json file.  In the example above "AzureStorageConnectionString".

2. Set the value of the connection string in the local.settings.json file (in this example against key "AzureStorageConnectionString").  To get the connection string value:
	* Azure Portal > Storage accounts > appropriate storage account (eg 'apidefaultaustralia9e44') > LH menu Blade > Security + networking section > Access keys
		- Access keys pane opens
	* Click 'Show keys' at top of Access keys pane;
	* Under key1 click the button to copy the Connection string to the clipboard (Connection string, NOT the Key value).

Publishing the Function to Azure
--------------------------------
1. In Visual Studio Solution Explorer right-click the project and select Publish - the Publish dialog will open;

2. In the Publish dialog set (after each screen is filled in click Next to move to the next one):
	* Target: Azure
	* Specific target: Azure Function App (Windows)
	* Functions instance: 
		* Microsoft account: (default)
		* Subscription name: Visual Studio Premium with MSDN (default)
		* View: Resource group (default)
		* Function Apps: 
			- Lists Resource groups (eg Api-Default-Australia-East)
			- Either: 
				- Expand a resource group and select an existing Function App; or
				- Click the '+' button to the right, on the same line as the Function Apps title, to create a new Function App:
					* Name: Of new Function App, must be globally unique.  The only valid characters in the name are a-z, 0-9, and - (NOT _);
					* Subscription: Subscription to use, eg Visual Studio Premium with MSDN;
					* Resource Group: Can either select an existing Resource Group from the list or select New to create a new one;
					* Plan Type: Consumption;
					* Location: Region, eg Australia East
					* Azure Storage: Select a storage account to use.  It must be of type StorageV2 (general purpose v2), not blob storage or StorageV1.
					NOTE: Creating the Function App is not fast, it can take a minute or two.
		* Run from package file: Ticked (This is the deployment method recommended by Microsoft as it gives the best performance),
	* Deployment type: Publish (generates pubxml file) (default)
	* Finish - the Publish dialog is closed, leaving the Publish tag open.

3. In the Publish tab > Hosting section click the Ellipsis button then select Manage Azure App Service Settings - the Application settings dialog will open;

4. In the Application settings dialog there are two values for each setting - Local and Remote.  For the setting matching the connection string key (in this example AzureStorageConnectionString) put the cursor in the Remote textbox and click the 'Insert value from local' link to copy the value from the Local setting to the Remote one (NOTE: Local settings are read from local.settings.json, Remote settings are read from the settings of the Function App in Azure.  NOTE ALSO: By default local.settings.json is excluded from Git source control).  Once the connection string is copied to the Remote setting click OK - both the local.settings.json and the settings of the Function App in Azure will be updated then the dialog will close;

5. In the Publish tab click the Publish button.  The project will be built and then published to Azure.  This may take a few minutes.

Testing the Function in Azure
-----------------------------
The boilerplate function code writes to the Azure logs when it is triggered.  The function is triggered by uploading a file into the specified container (in this case container 'sample-workitems').

1. In the Azure Portal select Home > Function App, then select the appropriate Function App from the list;

2. In the Function App LH blade menu select Functions section > Functions, then select the newly published Azure Function from the list;

3. In the Function LH blade menu select Developer section > Code + Test;

4. At the bottom of the page expand Logs (upward arrow) and ensure the Logs pane show they are connected;

5. In another browser tab open Azure Portal, select Home > Storage accounts, then select the appropriate storage account (in this case 'apidefaultaustralia9e44');

6. In the Storage account LH blade menu select Data storage section > Containers, then select the appropriate container that will trigger the Azure Function (in this case 'sample-workitems');

7. In the container select the Upload button, to upload a small file sitting on your local machine (eg a one-line text file, Test.txt);

8. Back in the Function Code + Test page the logs should show the log message logged by the Azure Function.  The message will look similar to:
```
	2022-03-25T10:42:33  Welcome, you are now connected to log-streaming service. The default timeout is 2 hours. Change the timeout with the App Setting SCM_LOGSTREAM_TIMEOUT (in seconds).  
	2022-03-25T10:42:38.224 [Information] Executing 'LocationParser' (Reason='New blob detected: samples-workitems/Test.txt', Id=4cfab55c-ce61-4543-a1ef-2114acd48b8a)  
	2022-03-25T10:42:38.224 [Information] Trigger Details: MessageId: 49bafa9e-8f20-4b1f-828d-2ffff78f15ea, DequeueCount: 1, InsertionTime: 2022-03-25T10:42:38.000+00:00, BlobCreated: 2022-03-25T10:37:28.000+00:00, BlobLastModified: 2022-03-25T10:42:36.000+00:00  
	2022-03-25T10:42:38.225 [Information] C# Blob trigger function Processed blobName:Test.txtSize: 15 Bytes  
	2022-03-25T10:42:38.225 [Information] Executed 'LocationParser' (Succeeded, Id=4cfab55c-ce61-4543-a1ef-2114acd48b8a, Duration=6ms)  
```
	The line starting "C# Blob trigger function Processed blob..." is written to the log by the function.

	As an addition check, if you go up to the parent Function App LH blade menu and select Overview, the Memory working set, Function Execution Count and MB Milliseconds graphs should each show a spike, indicating the function ran.  NOTE: If you leave the Function Code + Test page the Logs will be cleared, so check the Function logs first, before checking the parent Function App.

Determining Where Function Deployed to After Initial Deployment
---------------------------------------------------------------
Either: 

* In Visual Studio via the Publish tab:
	* Open the Publish tab: In Solution Explorer right-click on the project, select Publish;
	* The Hosting section of the Publish tab lists:
		* Subscription GUID;
		* Resource group;
		* Resource name: This is the Function App within the Resource group the Azure Function has been deployed to.

* Via the Publish Profile XML file, under the project > Properties > PublishProfiles.  It will be named for the Function App the function is being deployed to and the deployment method, for example 'BlobStorageTriggerLocalDev - Zip Deploy.pubxml':
	* The ResourceId element contains the same information as the Hosting section of the Publish tab.  For example:
		`<ResourceId>/subscriptions/xxxxxxx3-xxxd-xxxa-xxx2-xxxxxxxxxxx1/resourceGroups/Api-Default-Australia-East/providers/Microsoft.Web/sites/BlobStorageTriggerLocalDev</ResourceId>`

Adding Additional Functionality
-------------------------------
See the other README files in the same folder as this one for specifics of other functionality that can be added to the Azure Function.  For example, README_DependencyInjection.md explains how to add dependency injection to an Azure Function.