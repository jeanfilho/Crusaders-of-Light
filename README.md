
# Voronoi Level Procedural Generation
This is a project for data-driven, procedurally generated game levels in Unity using Voronoi diagrams
<a href="http://www.youtube.com/watch?feature=player_embedded&v=cGz6bf4KkOY
" target="_blank"><img width="1140" height="623" alt="image" src="https://github.com/user-attachments/assets/02096935-9a18-4649-864c-9bfd791bd906" />

## Requirements
- Unity 6.2

IMPORTANT: Due to changes in Unity, some of the code and assets are not behaving as intended. However, the procedural generation part of the project is still working and can be played with in the "TerrainGeneration" scene. The GameObject responsible for generating the level is "LevelCreator" with the equally named script. To generate a new level, press the "Generate" button at the bottom of the script

## Procedural Generation
The generation process is inspired by Kate Compton's GDC talk ["Practical Procedural Generation for Everyone"](https://www.youtube.com/watch?v=WumyfLEa6bU)

It's composed of 3 steps:
- Story generation
- Terrain generation
- Scenery generation

### Story Generation
This step creates the rough structure of how the level is going to be played. It creates a connected graph of start, end, special, main path and side path nodes.
The visualization of this structure would look like this:

<img width="337" height="808" alt="image" src="https://github.com/user-attachments/assets/ac40ffc9-24e5-4cb6-b36d-5bee1af65bbe" />

Players start at the green node, walk through the yellow nodes towards the end node. Optionally, they can explore the side orange node to find extra loot.

### Terrain Generation
Now that a Story structure has been generated, it's time to create the terrain itself. The first step is to generate a voronoi diagram over the heightmap using random uniformely distributed sample points. Once this is done, the voronoi samples are more evenly distributed using lloyd relaxation steps.
After the voronoi diagram is ready, the algorithm tries to fit the Story diagram into the voronoi diagram. It uses [Ullmann's algorithm](https://adriann.github.io/Ullman%20subgraph%20isomorphism.html) to fit the Story graph into the voronoi graph. If a match is not found, it simply regenerates the voronoi diagram with increasing sample count until a match is found. Border cells are ignored in this step.

<img width="761" height="764" alt="image" src="https://github.com/user-attachments/assets/437a7ff7-f47a-4c03-b1cd-52502a974130" />

With a match found, the paths through the connected voronoi cells are painted onto the terrain using Bezier curve segments and blocking geometry (trees and fences, depending on the specified theme) is added to non-connected edges. This prevents the players from skipping the path to the end node. Each cell is also used to deform the terrain heightmap using multiple octaves of perlin noise.

<img width="956" height="683" alt="image" src="https://github.com/user-attachments/assets/5c9b81a2-abb9-4bfa-8383-6216f89b745e" />

### Scenery Generation
Finally, we have the Story and the Terrain structure ready. Lamp posts and buildings are placed with an offset to the side of the road. Every cell except for the end is filled with vegetation using poisson disk sampling. The end cell has castle walls and towers placed along the non-connecting edges and vertices.

<img width="1025" height="706" alt="image" src="https://github.com/user-attachments/assets/102addcd-5b76-4f6e-a1a6-7be955fd6ce0" />

<img width="1117" height="804" alt="image" src="https://github.com/user-attachments/assets/ce945337-097f-450e-9a4f-361b80048ab3" />

## Adding new terrain types
You can create new types of maps by modifying the LevelCreator script and adding a new BiomeSettings object.

## University docs
More detail on the university pages [2018](https://collab.dvb.bayern/display/TUMGameslab2018/Crusaders+of+Light:+Dark+Dimensions) and [2017/2018](https://collab.dvb.bayern/display/TUMgameslab1718/RogueGen)
