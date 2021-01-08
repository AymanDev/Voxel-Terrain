# Voxel Terrain
This is my experimental project on creating Voxel Terrain tool. 
I went through some iterations already from Object oriented to Jobified generation system.
Right now this tool able to generate voxels and mesh for chunks. 

#### !WARNING!
This is not ready-production tool. This project using a lot experimental and preview packages.
Also there not so much features.

#### Completed Goals:
- [X] Custom chunk size by all coordinates 
- [X] Jobified voxel generation
- [X] Simple customization for terrain generation
- [X] Generating new chunks by tracking position of any game object
- [X] Customizable chunks spawn/despawn distance
- [X] Chunk caching
- [X] Queue system for preparing and loading chunks

### Goals in progress:
* Move to ECS *(kinda useless right now because of Hybrid Renderer V2 is kinda slow right now)*
  * Optimize terrain systems
* Optimize chunk spawn system *(right now calculation for loading or unloading chunks is unnecessary hard and slow)*
#### Goals to achieve:
* Add support for colored voxels 
* Add support for textured voxels


