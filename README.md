# In-Memory Cache

In-memory cache implementation which can store arbitrary object types with a key (also of abitrary type).

Should automatically remove items based on size of cache, removing last accessed item first.

Must be able to be used across an application as a singleton.

Must not make use of existing dotnet solutions, but may use external libraries.

Must update consumer of evicted items.

## Build

- Run `nuke`
- This should generate a .nupkg for consumption inside the /dist folder
