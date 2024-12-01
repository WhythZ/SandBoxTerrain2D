# SandboxTerrain2D

## License
This repo adopts [MIT License](https://spdx.org/licenses/MIT)

## About
This repo is a simple demo for 2D sandbox terrain generation

![img.png](https://github.com/WhythZ/SandboxTerrain2D/tree/master/Showcase/img.png)

The structure of the scripts is as below
- Manager<>: Generic singleton abstract class of managers
    - GameManager: Manage the game process
    - TerrainManager: Manage the generation of different structures by perlin noise
    - TilemapManager: Manage the instantiation of the blocks of tiles
- TileObject: Abstract class of different types of tiles
    - _00_Air
    - _01_Dirt
    - ...

## Environment
|Software|Version|
|---|---|
|Unity Editor|2022.3.48f1c1|
|Visual Studio|2022|
|Windows|10/11|