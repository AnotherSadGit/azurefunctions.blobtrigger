Including Folder Names in Blob Path
===================================
Simon Elms, 26 Mar 2022

Reference
----------
* Stackoverflow question "Microsoft Azure: How to create sub directory in a blob container", https://stackoverflow.com/questions/2619007/microsoft-azure-how-to-create-sub-directory-in-a-blob-container

Overview
---------
Azure Blob Storage does not support a folder hierarchy, it is a flat structure with a container holding blobs.  The container cannot hold folders, only blobs (files).

It does support a "virtual" folder structure, though.  Include a folder name in the blob path similar to a file system path (but with forward slashes).  For example, "samples-workitems/data/input-files".  Ensure this is entered into both the Local and Remote blob path settings.

When uploading a file to Blob Storage, in the Upload blob dialog expand the Advanced section and add the virtual folder path in the 'Upload to folder' textbox.  Do NOT include the container name.  In this example, enter 'data/input-files'.  After uploading the file you'll see the folder structure in the container, with a 'data' folder which contains another 'input-files' folder.

Test it by uploading a file to the container setting 'Upload to folder' to the folder path (eg 'data/input-files').  The function should run.  Then do it again, either uploading to the container root or to a parent folder in the path (eg 'data').  This time the function should NOT run.

NOTE: Having created the folder structure by uploading a file in the Azure Portal you can subsequently browse down from the root container to the folder containing the file and do an upload from there.  In that case the folder path is relative to the current folder so there is no need to set the 'Upload to folder' value.  Leave it blank and the file will be uploaded to the current folder.
