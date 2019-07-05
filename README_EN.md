# unity-reference-viewer

[**Japanese**](README.md)

It's a tool for searching asset references on Unity and displaying them on the window.  
Example, Examine material using texture.  

Unity standard "ref:" is effective when the scale of the project is large and Unity may crash.  
It's characterized by whether the GUID of the target asset is included in the file contents is searched by the function of OS, not on Unity.  
  
It process relatively fast depending on the environment. So doesn't cache results.

## How to

Right-click the target asset or directory and select "Find References In Project".

## Exclude settings

You can specify the file you want to exclude from the search results in "ExcludeSettings.asset".  
Specifying a binary file etc can improve the accuracy of searching and speed up.

## In MacOS, Spotlight or Grep

Spotlight is a standard search function(mdfind) used on Mac.
It works very fast, but the search fails unless the file index is correctly created.
If you need to search exactly please use the Grep version.

## In WindowsOS

Since I was using only Mac at first, I made a hurry when I needed a Windows version.
Because me have not examined the search method, it may be possible to search faster.

## License

This software is released under the MIT License.  
https://opensource.org/licenses/mit-license.php

Copyright (c) 2019 ina-amagami (ina_amagami@gc-career.com)