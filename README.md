0. When migrating with cmd > dotnet ef migrations ad Initial
   Get error "No executable found matching command "dotnet-ef""

   Solution:
   "I ran this from the command-line to get it to work again:
    Install-Package Microsoft.EntityFrameworkCore.Tools.DotNet -Version 1.0.0-preview3-22299
    It seems as the dotnet tools got corrupted for some reason that the dotnet restore couldnt resolve. 
    I tried to clear the package cache etc before I ran the above. 
    After I reinstalled the package dotnet crashed and after that it has begun to work again.
