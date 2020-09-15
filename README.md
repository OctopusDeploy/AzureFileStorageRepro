# AzureFileStorageRepro
Repro for the Azure File Storage issue where exceeding the quota doesn't throw an exception

Reproduction steps:
1. Create an Azure storage account (V2)
1. Create a file share and set the quota to 1 GiB
1. Follow [these instructions](https://docs.microsoft.com/en-us/azure/storage/files/storage-how-to-use-files-linux) and/or get the "Connect" sripts from Azure Portal File Share to mount the file share in Linux (I used Ubuntu 20)
1. Unzip `published.zip` and grant execute permission on `AzureFileStorageRepro`
1. Run `./AzureFileStorageRepro fill /mnt/xxx` to fill up the file share until there's only 4MB left
1. Run `./AzureFileStorageRepro repro /mnt/xxx` to write the final file that takes your over the limit - observe that there's no error thrown and the destination file has been truncated
